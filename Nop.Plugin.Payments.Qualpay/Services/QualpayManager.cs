using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Plugin.Payments.Qualpay.Domain;
using Nop.Plugin.Payments.Qualpay.Domain.PaymentGateway;
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
        /// Process Qualpay Payment Gateway request
        /// </summary>
        /// <typeparam name="TRequest">Request type</typeparam>
        /// <typeparam name="TResponse">Response type</typeparam>
        /// <param name="paymentGatewayRequest">Request</param>
        /// <param name="transactionId">Transaction identifier</param>
        /// <returns>Response</returns>
        private TResponse ProcessPaymentGatewayRequest<TRequest, TResponse>(TRequest paymentGatewayRequest)
            where TRequest : PaymentGatewayRequest where TResponse : PaymentGatewayResponse
        {
            //set credentials to request
            paymentGatewayRequest.DeveloperId = QualpayDefaults.DeveloperId;
            if (long.TryParse(_qualpaySettings.MerchantId, out long merchantId))
                paymentGatewayRequest.MerchantId = merchantId;
            
            //process request
            var response = ProcessRequest<TRequest, TResponse>(paymentGatewayRequest)
                ?? throw new NopException("An error occurred while processing. Error details in the log.");

            //whether request is succeeded
            if (response.ResponseCode != ResponseCode.Success)
                throw new NopException($"{response.ResponseCode}. {response.ResponseMessage}");

            return response;
        }

        /// <summary>
        /// Process request
        /// </summary>
        /// <typeparam name="TRequest">Request type</typeparam>
        /// <typeparam name="TResponse">Response type</typeparam>
        /// <param name="request">Request</param>
        /// <param name="url">Requesting URL</param>
        /// <param name="requestMethod">Request method</param>
        /// <returns>Response</returns>
        private TResponse ProcessRequest<TRequest, TResponse>(TRequest request) where TRequest: QualpayRequest where TResponse: QualpayResponse
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
            var encodedSecurityKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(_qualpaySettings.SecurityKey));
            webRequest.Headers.Add(HttpRequestHeader.Authorization, $"Basic {encodedSecurityKey}:");

            try
            {
                //create post data
                if (request.GetRequestMethod() != WebRequestMethods.Http.Get)
                {
                    var postData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));
                    webRequest.ContentLength = postData.Length;

                    using (var stream = webRequest.GetRequestStream())
                    {
                        stream.Write(postData, 0, postData.Length);
                    }
                }

                //get response
                var httpResponse = (HttpWebResponse)webRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    return JsonConvert.DeserializeObject<TResponse>(streamReader.ReadToEnd());
                }
            }
            catch (Exception exception)
            {
                var errorMessage = $"Qualpay payment error: {exception.Message}.";
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
                            return JsonConvert.DeserializeObject<TResponse>(errorResponse);
                        }
                    }
                }
                finally
                {
                    //log errors
                    _logger.Error(errorMessage, exception, _workContext.CurrentCustomer);
                }

                return null;
            }
        }

        #endregion

        #region Methods
        
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
        /// <param name="transactionRequest">Request parameters to sale transaction</param>
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
    }
}