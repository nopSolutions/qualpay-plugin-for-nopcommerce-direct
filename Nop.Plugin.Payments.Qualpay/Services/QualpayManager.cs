using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Plugin.Payments.Qualpay.Domain;
using Nop.Plugin.Payments.Qualpay.Domain.PaymentGateway;
using Nop.Plugin.Payments.Qualpay.Domain.Platform;
using Nop.Services.Logging;

namespace Nop.Plugin.Payments.Qualpay.Services
{
    /// <summary>
    /// Represents the Qualpay manager
    /// </summary>
    public class QualpayManager
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;
        private readonly QualpaySettings _qualpaySettings;

        #endregion

        #region Ctor

        public QualpayManager(ILogger logger,
            IWorkContext workContext,
            QualpaySettings qualpaySettings)
        {
            this._logger = logger;
            this._workContext = workContext;
            this._qualpaySettings = qualpaySettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get the Qualpay service base URL
        /// </summary>
        /// <returns>URL</returns               
        private string GetServiceBaseUrl()
        {
            return _qualpaySettings.UseSandbox ? "https://api-test.qualpay.com/" : "https://api.qualpay.com/";
        }

        /// <summary>
        /// Process Qualpay Platform request
        /// </summary>
        /// <typeparam name="TRequest">Request type</typeparam>
        /// <typeparam name="TResponse">Response type</typeparam>
        /// <param name="platformRequest">Request</param>
        /// <returns>Response</returns>
        private TResponse ProcessPlatformRequest<TRequest, TResponse>(TRequest platformRequest)
            where TRequest : PlatformRequest where TResponse : PlatformResponse
        {
            return HandleRequestAction(() =>
            {
                //process request
                var response = ProcessRequest<TRequest, TResponse>(platformRequest)
                    ?? throw new NopException("An error occurred while processing. Error details in the log.");

                //whether request is succeeded
                if (response.ResponseCode != PlatformResponseCode.Success)
                    throw new NopException($"{response.ResponseCode}. {response.Message}");

                return response;
            });
        }

        /// <summary>
        /// Process Qualpay Payment Gateway request
        /// </summary>
        /// <typeparam name="TRequest">Request type</typeparam>
        /// <typeparam name="TResponse">Response type</typeparam>
        /// <param name="paymentGatewayRequest">Request</param>
        /// <returns>Response</returns>
        private TResponse ProcessPaymentGatewayRequest<TRequest, TResponse>(TRequest paymentGatewayRequest)
            where TRequest : PaymentGatewayRequest where TResponse : PaymentGatewayResponse
        {
            var response = HandleRequestAction(() =>
            {
                //set credentials
                paymentGatewayRequest.DeveloperId = QualpayDefaults.DeveloperId;
                paymentGatewayRequest.MerchantId = long.Parse(_qualpaySettings.MerchantId);

                //process request
                return ProcessRequest<TRequest, TResponse>(paymentGatewayRequest);
            }) ?? throw new NopException("No response from the Qualpay Payment Gateway.");

            //whether request is succeeded
            if (response.ResponseCode != PaymentGatewayResponseCode.Success)
                throw new NopException($"Qualpay Payment Gateway error: {response.ResponseCode}. {response.Message}");

            return response;
        }

        /// <summary>
        /// Process request
        /// </summary>
        /// <typeparam name="TRequest">Request type</typeparam>
        /// <typeparam name="TResponse">Response type</typeparam>
        /// <param name="request">Request</param>
        /// <returns>Response</returns>
        private TResponse ProcessRequest<TRequest, TResponse>(TRequest request)
            where TRequest : QualpayRequest where TResponse : QualpayResponse
        {
            //create requesting URL
            var url = $"{GetServiceBaseUrl()}{request.GetRequestPath()}";

            //create web request
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = request.GetRequestMethod();
            webRequest.UserAgent = QualpayDefaults.UserAgent;
            webRequest.Accept = "application/json";
            webRequest.ContentType = "application/json; charset=utf-8";

            //add authorization header
            var encodedSecurityKey = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_qualpaySettings.SecurityKey}:"));
            webRequest.Headers.Add(HttpRequestHeader.Authorization, $"Basic {encodedSecurityKey}");

            //create post data
            if (request.GetRequestMethod() != WebRequestMethods.Http.Get)
            {
                var postData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));
                webRequest.ContentLength = postData.Length;

                using (var stream = webRequest.GetRequestStream())
                    stream.Write(postData, 0, postData.Length);
            }

            //get response
            var httpResponse = (HttpWebResponse)webRequest.GetResponse();
            var responseMessage = string.Empty;
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                responseMessage = streamReader.ReadToEnd();

            //return result
            return JsonConvert.DeserializeObject<TResponse>(responseMessage);
        }

        /// <summary>
        /// Handle request action
        /// </summary>
        /// <typeparam name="T">Response type</typeparam>
        /// <param name="requestAction">Request action</param>
        /// <returns>Response</returns>
        private T HandleRequestAction<T>(Func<T> requestAction)
        {
            try
            {
                //ensure that plugin is configured
                if (string.IsNullOrEmpty(_qualpaySettings.MerchantId) || !long.TryParse(_qualpaySettings.MerchantId, out long merchantId))
                    throw new NopException("Plugin not configured.");

                //process request action
                return requestAction();

            }
            catch (Exception exception)
            {
                var errorMessage = $"Qualpay error: {exception.Message}.";
                try
                {
                    //try to get error response
                    if (exception is WebException webException)
                    {
                        var httpResponse = (HttpWebResponse)webException.Response;
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var errorResponse = streamReader.ReadToEnd();
                            errorMessage = $"{errorMessage} Details: {errorResponse}";
                            return JsonConvert.DeserializeObject<T>(errorResponse);
                        }
                    }
                }
                catch { }
                finally
                {
                    //log errors
                    _logger.Error(errorMessage, exception, _workContext.CurrentCustomer);
                }

                return default(T);
            }
        }

        #endregion

        #region Methods

        #region Platform

        /// <summary>
        /// Get a customer from Qualpay Customer Vault by the passed identifier
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>Vault Customer</returns>
        public VaultCustomer GetCustomerById(string customerId)
        {
            var getCustomerRequest = new GetCustomerRequest { CustomerId = customerId };
            return ProcessPlatformRequest<GetCustomerRequest, CustomerVaultResponse>(getCustomerRequest)?.VaultCustomer;
        }

        /// <summary>
        /// Create new customer in Qualpay Customer Vault
        /// </summary>
        /// <param name="createCustomerRequest">Request parameters to create customer</param>
        /// <returns>Vault Customer</returns>
        public VaultCustomer CreateCustomer(CreateCustomerRequest createCustomerRequest)
        {
            return ProcessPlatformRequest<CreateCustomerRequest, CustomerVaultResponse>(createCustomerRequest)?.VaultCustomer;
        }

        /// <summary>
        /// Get customer billing cards from Qualpay Customer Vault
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>Collection of customer billing cards</returns>
        public IEnumerable<BillingCard> GetCustomerCards(string customerId)
        {
            var getCustomerCardsRequest = new GetCustomerCardsRequest { CustomerId = customerId };
            var response = ProcessPlatformRequest<GetCustomerCardsRequest, CustomerVaultResponse>(getCustomerCardsRequest);
            return response?.VaultCustomer?.BillingCards;
        }

        /// <summary>
        /// Create customer billing card in Qualpay Customer Vault
        /// </summary>
        /// <param name="createCustomerCardRequest">Request parameters to create card</param>
        /// <returns>True if customer card successfully created in the Vault; otherwise false</returns>
        public bool CreateCustomerCard(CreateCustomerCardRequest createCustomerCardRequest)
        {
            var response = ProcessPlatformRequest<CreateCustomerCardRequest, CustomerVaultResponse>(createCustomerCardRequest);
            return response?.ResponseCode == PlatformResponseCode.Success;
        }

        /// <summary>
        /// Update customer billing card in Qualpay Customer Vault
        /// </summary>
        /// <param name="updateCustomerCardRequest">Request parameters to update card</param>
        /// <returns>True if customer card successfully updated in the Vault; otherwise false</returns>
        public bool UpdateCustomerCard(UpdateCustomerCardRequest updateCustomerCardRequest)
        {
            var response = ProcessPlatformRequest<UpdateCustomerCardRequest, CustomerVaultResponse>(updateCustomerCardRequest);
            return response?.ResponseCode == PlatformResponseCode.Success;
        }

        /// <summary>
        /// Delete customer billing card from Qualpay Customer Vault
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <param name="cardId">Card identifier</param>
        /// <returns>True if customer card successfully deleted from the Vault; otherwise false</returns>
        public bool DeleteCustomerCard(string customerId, string cardId)
        {
            var deleteCustomerCardRequest = new DeleteCustomerCardRequest { CustomerId = customerId, CardId = cardId };
            var response = ProcessPlatformRequest<DeleteCustomerCardRequest, CustomerVaultResponse>(deleteCustomerCardRequest);
            return response?.ResponseCode == PlatformResponseCode.Success;
        }

        /// <summary>
        /// Get transient key from Qualpay Embedded Fields
        /// </summary>
        /// <returns>Embedded key</returns>
        public EmbeddedKey GetTransientKey()
        {
            var getTransientKeyRequest = new GetTransientKeyRequest();
            return ProcessPlatformRequest<GetTransientKeyRequest, EmbeddedFieldsResponse>(getTransientKeyRequest)?.EmbeddedKey;
        }

        /// <summary>
        /// Get a webhook by the identifier
        /// </summary>
        /// <param name="webhookId">Webhook identifier</param>
        /// <returns>Webhook</returns>
        public Webhook GetWebhookById(string webhookId)
        {
            var getWebhookRequest = new GetWebhookRequest
            {
                WebhookId = long.TryParse(webhookId, out long webhookIdValue) ? (long?)webhookIdValue : null
            };
            return ProcessPlatformRequest<GetWebhookRequest, WebhookResponse>(getWebhookRequest)?.Webhook;
        }

        /// <summary>
        /// Create webhook
        /// </summary>
        /// <param name="createWebhookRequest">Request parameters to create webhook</param>
        /// <returns>Webhook</returns>
        public Webhook CreateWebhook(CreateWebhookRequest createWebhookRequest)
        {
            createWebhookRequest.WebhookNode = _qualpaySettings.MerchantId;
            return ProcessPlatformRequest<CreateWebhookRequest, WebhookResponse>(createWebhookRequest)?.Webhook;
        }
        
        /// <summary>
        /// Validate whether webhook request is initiated by Qualpay and return received data details if is valid
        /// </summary>
        /// <typeparam name="T">Data details type</typeparam>
        /// <param name="request">Request</param>
        /// <returns>True if webhook request is valid; otherwise false and received data details</returns>
        public (bool requestIsValid, WebhookEvent<T> webhookEvent) ValidateWebhook<T>(HttpRequest request) where T: PlatformRequest
        {
            return HandleRequestAction(() =>
            {
                //try to get request message
                var message = string.Empty;
                using (var streamReader = new StreamReader(request.Body, Encoding.UTF8))
                    message = streamReader.ReadToEnd();

                if (string.IsNullOrEmpty(message))
                    throw new NopException("Webhook request is empty.");

                //ensure that request is signed using a signature header
                if (!request.Headers.TryGetValue(QualpayDefaults.WebhookSignatureHeaderName, out StringValues signatures))
                    throw new NopException("Webhook request not signed by a signature header.");

                //get encrypted string from the request message
                var encryptedString = string.Empty;
                using (var hashAlgorithm = new HMACSHA256(Encoding.UTF8.GetBytes(_qualpaySettings.WebhookSecretKey)))
                    encryptedString = Convert.ToBase64String(hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(message)));

                //equal this encrypted string with received signatures
                if (!signatures.Any(signature => signature.Equals(encryptedString)))
                    throw new NopException("Webhook request isn't valid.");

                //request is valid, so log received message
                _logger.Information($"Qualpay Webhook. Webhook request is received: {message}");

                //and try to get data details from webhook message
                var webhookEvent = JsonConvert.DeserializeObject<WebhookEvent<T>>(message);
                if (webhookEvent?.Data is T data)
                    return (true, webhookEvent);

                return (true, null);
            });
        }

        /// <summary>
        /// Get subscription transactions
        /// </summary>
        /// <param name="subscriptionId">Subscription identifier</param>
        /// <returns>Collection of transactions</returns>
        public IEnumerable<SubscriptionTransaction> GetSubscriptionTransactions(long? subscriptionId)
        {
            var request = new GetSubscriptionTransactionsRequest { SubscriptionId = subscriptionId };
            return ProcessPlatformRequest<GetSubscriptionTransactionsRequest, SubscriptionTransactionsResponse>(request)?.Transactions;
        }

        /// <summary>
        /// Create subscription
        /// </summary>
        /// <param name="createSubscriptionRequest">Request parameters to create subscription</param>
        /// <returns>Subscription</returns>
        public Subscription CreateSubscription(CreateSubscriptionRequest createSubscriptionRequest)
        {
            if (long.TryParse(_qualpaySettings.MerchantId, out long merchantId))
                createSubscriptionRequest.MerchantId = merchantId;

            return ProcessPlatformRequest<CreateSubscriptionRequest, SubscriptionResponse>(createSubscriptionRequest)?.Subscription;
        }

        /// <summary>
        /// Cancel subscription
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <param name="subscriptionId">Subscription identifier</param>
        /// <returns>True if subscription successfully cancelled; otherwise false</returns>
        public bool CancelSubscription(string customerId, string subscriptionId)
        {
            var cancelSubscriptionRequest = new CancelSubscriptionRequest { CustomerId = customerId };
            if (long.TryParse(subscriptionId, out long subscriptionIdInt))
                cancelSubscriptionRequest.SubscriptionId = subscriptionIdInt;
            if (long.TryParse(_qualpaySettings.MerchantId, out long merchantId))
                cancelSubscriptionRequest.MerchantId = merchantId;

            var response = ProcessPlatformRequest<CancelSubscriptionRequest, SubscriptionResponse>(cancelSubscriptionRequest);
            return response?.ResponseCode == PlatformResponseCode.Success;
        }

        #endregion

        #region Payment Gateway

        /// <summary>
        /// Tokenize card data
        /// </summary>
        /// <param name="tokenizeRequest">Request parameters to tokenize card</param>
        /// <returns>Card identifier</returns>
        public string TokenizeCard(TokenizeRequest tokenizeRequest)
        {
            return ProcessPaymentGatewayRequest<TokenizeRequest, TokenizeResponse>(tokenizeRequest)?.CardId;
        }

        /// <summary>
        /// Authorize a transaction
        /// </summary>
        /// <param name="transactionRequest">Request parameters to authorize transaction</param>
        /// <returns>Response</returns>
        public TransactionResponse Authorize(TransactionRequest transactionRequest)
        {
            transactionRequest.TransactionType = TransactionType.Authorization;
            return ProcessPaymentGatewayRequest<TransactionRequest, TransactionResponse>(transactionRequest);
        }

        /// <summary>
        /// Sale
        /// </summary>
        /// <param name="transactionRequest">Request parameters to sale</param>
        /// <returns>Response</returns>
        public TransactionResponse Sale(TransactionRequest transactionRequest)
        {
            transactionRequest.TransactionType = TransactionType.Sale;
            return ProcessPaymentGatewayRequest<TransactionRequest, TransactionResponse>(transactionRequest);
        }

        /// <summary>
        /// Capture an authorized transaction
        /// </summary>
        /// <param name="captureRequest">Request parameters to capture transaction</param>
        /// <returns>Response</returns>
        public CaptureResponse CaptureTransaction(CaptureRequest captureRequest)
        {
            return ProcessPaymentGatewayRequest<CaptureRequest, CaptureResponse>(captureRequest);
        }

        /// <summary>
        /// Void an authorized transaction
        /// </summary>
        /// <param name="voidRequest">Request parameters to void transaction</param>
        /// <returns>Response</returns>
        public VoidResponse VoidTransaction(VoidRequest voidRequest)
        {
            return ProcessPaymentGatewayRequest<VoidRequest, VoidResponse>(voidRequest);
        }

        /// <summary>
        /// Refund a charged transaction
        /// </summary>
        /// <param name="refundRequest">Request parameters to refund transaction</param>
        /// <returns>Response</returns>
        public RefundResponse Refund(RefundRequest refundRequest)
        {
            return ProcessPaymentGatewayRequest<RefundRequest, RefundResponse>(refundRequest);
        }

        #endregion

        #endregion
    }
}