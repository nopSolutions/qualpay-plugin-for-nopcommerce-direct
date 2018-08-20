using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.Qualpay.Domain;
using Nop.Plugin.Payments.Qualpay.Domain.PaymentGateway;
using Nop.Plugin.Payments.Qualpay.Domain.Platform;
using Nop.Plugin.Payments.Qualpay.Models;
using Nop.Plugin.Payments.Qualpay.Services;
using Nop.Plugin.Payments.Qualpay.Validators;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Tax;

namespace Nop.Plugin.Payments.Qualpay
{
    /// <summary>
    /// Represents Qualpay payment gateway processor
    /// </summary>
    public class QualpayProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPaymentService _paymentService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IProductService _productService;
        private readonly ISettingService _settingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ITaxService _taxService;
        private readonly IWebHelper _webHelper;
        private readonly QualpayManager _qualpayManager;
        private readonly QualpaySettings _qualpaySettings;

        #endregion

        #region Ctor

        public QualpayProcessor(CurrencySettings currencySettings,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPaymentService paymentService,
            IPriceCalculationService priceCalculationService,
            IProductService productService,
            ISettingService settingService,
            IShoppingCartService shoppingCartService,
            ITaxService taxService,
            IWebHelper webHelper,
            QualpayManager qualpayManager,
            QualpaySettings qualpaySettings)
        {
            this._currencySettings = currencySettings;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._currencyService = currencyService;
            this._customerService = customerService;
            this._genericAttributeService = genericAttributeService;
            this._localizationService = localizationService;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._paymentService = paymentService;
            this._priceCalculationService = priceCalculationService;
            this._productService = productService;
            this._settingService = settingService;
            this._shoppingCartService = shoppingCartService;
            this._taxService = taxService;
            this._webHelper = webHelper;
            this._qualpayManager = qualpayManager;
            this._qualpaySettings = qualpaySettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get transaction line items
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="orderTotal">Order total</param>
        /// <param name="taxAmount">Tax amount</param>
        /// <returns>List of transaction items</returns>
        private IList<LineItem> GetItems(Customer customer, int storeId, decimal orderTotal, out decimal taxAmount)
        {
            var items = new List<LineItem>();

            //get current shopping cart
            var shoppingCart = customer.ShoppingCartItems
                .Where(shoppingCartItem => shoppingCartItem.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(storeId).ToList();

            //set tax amount
            taxAmount = _orderTotalCalculationService.GetTaxTotal(shoppingCart);

            //create transaction items from shopping cart items
            items.AddRange(shoppingCart.Where(shoppingCartItem => shoppingCartItem.Product != null).Select(shoppingCartItem =>
            {
                //item price
                var price = _taxService.GetProductPrice(shoppingCartItem.Product, _priceCalculationService.GetUnitPrice(shoppingCartItem),
                    false, shoppingCartItem.Customer, out _);

                return CreateItem(price, shoppingCartItem.Product.Name,
                    _productService.FormatSku(shoppingCartItem.Product, shoppingCartItem.AttributesXml),
                    shoppingCartItem.Quantity);
            }));

            //create transaction items from checkout attributes
            var checkoutAttributesXml = _genericAttributeService.GetAttribute<string>(customer,
                NopCustomerDefaults.CheckoutAttributes, storeId);

            if (!string.IsNullOrEmpty(checkoutAttributesXml))
            {
                var attributeValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(checkoutAttributesXml);
                items.AddRange(attributeValues.Where(attributeValue => attributeValue.CheckoutAttribute != null).Select(attributeValue =>
                {
                    return CreateItem(_taxService.GetCheckoutAttributePrice(attributeValue, false, customer),
                        $"{attributeValue.CheckoutAttribute.Name} ({attributeValue.Name})", "checkout");
                }));
            }

            //create transaction item for payment method additional fee
            var paymentAdditionalFee = _paymentService.GetAdditionalHandlingFee(shoppingCart, PluginDescriptor.SystemName);
            var paymentPrice = _taxService.GetPaymentMethodAdditionalFee(paymentAdditionalFee, false, customer);
            if (paymentPrice > decimal.Zero)
                items.Add(CreateItem(paymentPrice, $"Payment ({PluginDescriptor.FriendlyName})", "payment"));

            //create transaction item for shipping rate
            if (_shoppingCartService.ShoppingCartRequiresShipping(shoppingCart))
            {
                var shippingPrice = _orderTotalCalculationService.GetShoppingCartShippingTotal(shoppingCart, false);
                if (shippingPrice.HasValue && shippingPrice.Value > decimal.Zero)
                    items.Add(CreateItem(shippingPrice.Value, "Shipping rate", "shipping"));
            }

            //create transaction item for all discounts
            var amountDifference = orderTotal - items.Sum(lineItem => lineItem.UnitPrice * lineItem.Quantity).Value - taxAmount;
            if (amountDifference < decimal.Zero)
                items.Add(CreateItem(amountDifference, "Discount amount", "discounts"));

            return items;
        }

        /// <summary>
        /// Create transaction line item
        /// </summary>
        /// <param name="price">Price per unit</param>
        /// <param name="description">Item description</param>
        /// <param name="productCode">Item code (e.g. SKU)</param>
        /// <param name="quantity">Quntity</param>
        /// <returns>Transaction line item</returns>
        private LineItem CreateItem(decimal price, string description, string productCode, int quantity = 1)
        {
            return new LineItem
            {
                CreditType = ItemCreditType.Debit,
                Description = CommonHelper.EnsureMaximumLength(description, 25),
                MeasureUnit = "*",
                ProductCode = CommonHelper.EnsureMaximumLength(productCode, 12),
                Quantity = quantity,
                UnitPrice = price
            };
        }

        /// <summary>
        /// Get request parameters to create a customer in Vault
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Request parameters to create customer</returns>
        private CreateCustomerRequest CreateCustomerRequest(Customer customer)
        {
            return new CreateCustomerRequest
            {
                CustomerId = customer.Id.ToString(),
                Email = customer.Email,
                FirstName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.FirstNameAttribute),
                LastName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.LastNameAttribute),
                Company = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.CompanyAttribute),
                Phone = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.PhoneAttribute),
                ShippingAddresses = customer.ShippingAddress == null ? null : new List<Domain.Platform.ShippingAddress>
                {
                    new Domain.Platform.ShippingAddress
                    {
                        IsPrimary = true,
                        FirstName = customer.ShippingAddress.FirstName,
                        LastName = customer.ShippingAddress.LastName,
                        Address1 = customer.ShippingAddress?.Address1,
                        Address2 = customer.ShippingAddress.Address2,
                        City = customer.ShippingAddress?.City,
                        StateCode = customer.ShippingAddress?.StateProvince?.Abbreviation,
                        CountryName = customer.ShippingAddress?.Country?.ThreeLetterIsoCode,
                        Zip = customer.ShippingAddress?.ZipPostalCode,
                        Company = customer.ShippingAddress?.Company
                    }
                }
            };
        }

        /// <summary>
        /// Get frequency, subscription cycle interval and start date of recurring payment 
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Frequency type, cycle interval, start date</returns>
        private (PlanFrequency? frequency, int? interval, DateTime startDate) GetSubscriptionParameters(ProcessPaymentRequest processPaymentRequest)
        {
            switch (processPaymentRequest.RecurringCyclePeriod)
            {
                case RecurringProductCyclePeriod.Days:
                    if (processPaymentRequest.RecurringCycleLength % 30 == 0 || processPaymentRequest.RecurringCycleLength % 31 == 0)
                    {
                        return (PlanFrequency.Monthly, processPaymentRequest.RecurringCycleLength / 30,
                            DateTime.UtcNow.AddDays(processPaymentRequest.RecurringCycleLength));
                    }

                    if (processPaymentRequest.RecurringCycleLength % 7 > 0)
                        throw new NopException("Qualpay Payment Gateway error: Recurring Billing supports payments with the minimum frequency of once a week");

                    return (PlanFrequency.Weekly, processPaymentRequest.RecurringCycleLength / 7,
                        DateTime.UtcNow.AddDays(processPaymentRequest.RecurringCycleLength));

                case RecurringProductCyclePeriod.Weeks:
                    return (PlanFrequency.Weekly, processPaymentRequest.RecurringCycleLength,
                        DateTime.UtcNow.AddDays(processPaymentRequest.RecurringCycleLength * 7));

                case RecurringProductCyclePeriod.Months:
                    if (processPaymentRequest.RecurringCycleLength == 12)
                        return (PlanFrequency.Annually, null, DateTime.UtcNow.AddYears(1));

                    return (PlanFrequency.Monthly, processPaymentRequest.RecurringCycleLength,
                        DateTime.UtcNow.AddMonths(processPaymentRequest.RecurringCycleLength));

                case RecurringProductCyclePeriod.Years:
                    if (processPaymentRequest.RecurringCycleLength == 1)
                        return (PlanFrequency.Annually, null, DateTime.UtcNow.AddYears(1));

                    return (PlanFrequency.Monthly, processPaymentRequest.RecurringCycleLength * 12,
                        DateTime.UtcNow.AddYears(processPaymentRequest.RecurringCycleLength));
            }

            return (null, null, DateTime.UtcNow);
        }

        /// <summary>
        /// Create request parameters to create subscription for recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Request parameters</returns>
        private CreateSubscriptionRequest CreateSubscriptionRequest(ProcessPaymentRequest processPaymentRequest)
        {
            //whether Recurring Billing is enabled
            if (!_qualpaySettings.UseRecurringBilling)
                throw new NopException("Recurring payments are not available");

            var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId)
                ?? throw new NopException("Customer cannot be loaded");

            //Recurring Billing is available only for registered customers
            if (customer.IsGuest())
                throw new NopException("Recurring payments are available only for registered customers");

            //Qualpay Payment Gateway supports only USD currency
            var primaryStoreCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
            if (!primaryStoreCurrency.CurrencyCode.Equals("USD", StringComparison.InvariantCultureIgnoreCase))
                throw new NopException("USD is not primary store currency");

            //ensure that customer exists in Vault (recurring billing is available only for stored customers)
            var vaultCustomer = _qualpayManager.GetCustomerById(customer.Id.ToString())
                ?? _qualpayManager.CreateCustomer(CreateCustomerRequest(customer))
                ?? throw new NopException("Qualpay Payment Gateway error: Failed to create recurring payment.");

            var subscriptionRequest = new CreateSubscriptionRequest
            {
                Amount = Math.Round(processPaymentRequest.OrderTotal, 2),
                CurrencyIsoCode = QualpayDefaults.UsdNumericIsoCode,
                CustomerFirstName = customer.BillingAddress?.FirstName,
                CustomerLastName = customer.BillingAddress?.LastName,
                CustomerId = customer.Id.ToString(),
                IsSubscriptionOnPlan = false,
                PlanDescription = processPaymentRequest.OrderGuid.ToString(),
                PlanDuration = processPaymentRequest.RecurringTotalCycles - 1,
                SetupAmount = Math.Round(processPaymentRequest.OrderTotal, 2),
                Status = SubscriptionStatus.Active
            };

            //set frequency parameters
            var (frequency, interval, startDate) = GetSubscriptionParameters(processPaymentRequest);
            subscriptionRequest.PlanFrequency = frequency;
            subscriptionRequest.Interval = interval;
            subscriptionRequest.DateStart = startDate.ToString("yyyy-MM-dd");

            //whether the customer has chosen a previously saved card
            var selectedCard = GetPreviouslySavedBillingCard(processPaymentRequest, customer);
            if (selectedCard != null)
            {
                //ensure that the selected card is default card
                if (!selectedCard.IsPrimary ?? true)
                {
                    var updated = _qualpayManager.UpdateCustomerCard(new UpdateCustomerCardRequest
                    {
                        CardId = selectedCard.CardId,
                        CustomerId = customer.Id.ToString(),
                        IsPrimary = true,
                        BillingAddress1 = customer.BillingAddress?.Address1,
                        BillingZip = customer.BillingAddress?.ZipPostalCode
                    });
                    if (!updated)
                        throw new NopException("Qualpay Payment Gateway error: Failed to pay by the selected card.");
                }

                return subscriptionRequest;
            }

            //get card identifier
            var tokenizedCardId = GetTokenizedCardId(processPaymentRequest, customer);
            if (string.IsNullOrEmpty(tokenizedCardId))
                throw new NopException("Qualpay Payment Gateway error: Failed to pay by the selected card.");

            //add tokenized billing card to customer
            var created = _qualpayManager.CreateCustomerCard(new CreateCustomerCardRequest
            {
                Verify = true,
                IsPrimary = true,
                CardId = tokenizedCardId,
                CustomerId = customer.Id.ToString(),
                BillingAddress1 = customer.BillingAddress?.Address1,
                BillingZip = customer.BillingAddress?.ZipPostalCode
            });
            if (!created)
                throw new NopException("Qualpay Payment Gateway error: Failed to pay by the selected card.");

            return subscriptionRequest;
        }

        /// <summary>
        /// Create request parameters to authorize or sale
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Request parameters</returns>
        private TransactionRequest CreateTransactionRequest(ProcessPaymentRequest processPaymentRequest)
        {
            var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId)
                ?? throw new NopException("Customer cannot be loaded");

            //Qualpay Payment Gateway supports only USD currency
            var primaryStoreCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
            if (!primaryStoreCurrency.CurrencyCode.Equals("USD", StringComparison.InvariantCultureIgnoreCase))
                throw new NopException("USD is not a primary store currency");

            var transactionRequest = new TransactionRequest
            {
                PurchaseId = CommonHelper.EnsureMaximumLength(processPaymentRequest.OrderGuid.ToString(), 25),
                Amount = Math.Round(processPaymentRequest.OrderTotal, 2),
                CurrencyIsoCode = QualpayDefaults.UsdNumericIsoCode,
                SendEmailReceipt = !string.IsNullOrEmpty(customer.BillingAddress?.Email),
                CustomerEmail = customer.BillingAddress?.Email,
                Items = GetItems(customer, processPaymentRequest.StoreId, processPaymentRequest.OrderTotal, out decimal taxAmount),
                AmountTax = (double)Math.Round(taxAmount, 2)
            };

            //whether the customer has chosen a previously saved card
            var selectedCard = GetPreviouslySavedBillingCard(processPaymentRequest, customer);
            if (selectedCard != null)
            {
                //card exists, set it to the request parameters
                transactionRequest.CardId = selectedCard.CardId;
                transactionRequest.CustomerId = customer.Id.ToString();

                return transactionRequest;
            }

            //set card identifier to the request parameters
            transactionRequest.CardId = GetTokenizedCardId(processPaymentRequest, customer);

            //whether the customer has chosen to save card details for the future using
            var saveCardKey = _localizationService.GetResource("Plugins.Payments.Qualpay.Customer.Card.Save");
            if (!processPaymentRequest.CustomValues.ContainsKey(saveCardKey))
                return transactionRequest;

            //remove the value from payment custom values, since it is no longer needed
            processPaymentRequest.CustomValues.Remove(saveCardKey);

            //check whether customer is already exists in the Vault and try to create new one if does not exist
            var vaultCustomer = _qualpayManager.GetCustomerById(customer.Id.ToString())
                ?? _qualpayManager.CreateCustomer(CreateCustomerRequest(customer));

            if (vaultCustomer == null || string.IsNullOrEmpty(transactionRequest.CardId))
                return transactionRequest;

            //customer exists, thus add tokenized billing card to customer
            _qualpayManager.CreateCustomerCard(new CreateCustomerCardRequest
            {
                Verify = true,
                CardId = transactionRequest.CardId,
                CustomerId = customer.Id.ToString(),
                BillingAddress1 = customer.BillingAddress?.Address1,
                BillingZip = customer.BillingAddress?.ZipPostalCode
            });

            return transactionRequest;
        }

        /// <summary>
        /// Get selected by customer previously saved billing card
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <param name="customer">Customer</param>
        /// <returns>Billing card</returns>
        private BillingCard GetPreviouslySavedBillingCard(ProcessPaymentRequest processPaymentRequest, Customer customer)
        {
            var cardIdKey = _localizationService.GetResource("Plugins.Payments.Qualpay.Customer.Card");
            if (!processPaymentRequest.CustomValues.TryGetValue(cardIdKey, out object cardId))
                return null;

            //remove the value from payment custom values, since it is no longer needed
            processPaymentRequest.CustomValues.Remove(cardIdKey);

            //ensure that customer exists in Vault and has this card
            var selectedCard = _qualpayManager.GetCustomerCards(customer.Id.ToString())
                ?.FirstOrDefault(card => card?.CardId?.Equals(cardId.ToString()) ?? false)
                ?? throw new NopException("Qualpay Payment Gateway error: Failed to pay by the selected card.");

            return selectedCard;
        }

        /// <summary>
        /// Get tokenized card identifier
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <param name="customer">Customer</param>
        /// <returns>Card identifier</returns>
        private string GetTokenizedCardId(ProcessPaymentRequest processPaymentRequest, Customer customer)
        {
            var cardId = string.Empty;

            if (_qualpaySettings.UseEmbeddedFields)
            {
                //tokenized card identifier has already been received from Qualpay Embedded Fields 
                var tokenizedCardIdKey = _localizationService.GetResource("Plugins.Payments.Qualpay.Customer.Card.Token");
                if (processPaymentRequest.CustomValues.TryGetValue(tokenizedCardIdKey, out object tokenizedCardId))
                    cardId = tokenizedCardId.ToString();

                //remove the value from payment custom values, since it is no longer needed
                processPaymentRequest.CustomValues.Remove(tokenizedCardIdKey);
            }
            else
            {
                //or try to tokenize card data now
                cardId = _qualpayManager.TokenizeCard(new TokenizeRequest
                {
                    IsSingleUse = true,
                    CardholderName = processPaymentRequest.CreditCardName,
                    CardNumber = processPaymentRequest.CreditCardNumber,
                    Cvv2 = processPaymentRequest.CreditCardCvv2,
                    ExpirationDate = $"{processPaymentRequest.CreditCardExpireMonth:D2}{processPaymentRequest.CreditCardExpireYear.ToString().Substring(2)}",
                    AvsAddress = CommonHelper.EnsureMaximumLength(customer.BillingAddress?.Address1, 20),
                    AvsZip = customer.BillingAddress?.ZipPostalCode
                });
            }

            return cardId;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            //create request
            var transactionRequest = CreateTransactionRequest(processPaymentRequest);

            //get response
            var response =
                _qualpaySettings.PaymentTransactionType == TransactionType.Authorization ? _qualpayManager.Authorize(transactionRequest) :
                _qualpaySettings.PaymentTransactionType == TransactionType.Sale ? _qualpayManager.Sale(transactionRequest) :
                throw new ArgumentException("Transaction type is not supported", nameof(_qualpaySettings.PaymentTransactionType));

            //request succeeded
            var result = new ProcessPaymentResult
            {
                AvsResult = response.AvsResult,
                Cvv2Result = response.Cvv2Result,
                AuthorizationTransactionCode = response.AuthorizationCode
            };

            //set an authorization details
            if (_qualpaySettings.PaymentTransactionType == TransactionType.Authorization)
            {
                result.AuthorizationTransactionId = response.TransactionId;
                result.AuthorizationTransactionResult = response.Message;
                result.NewPaymentStatus = PaymentStatus.Authorized;
            }

            //or set a capture details
            if (_qualpaySettings.PaymentTransactionType == TransactionType.Sale)
            {
                result.CaptureTransactionId = response.TransactionId;
                result.CaptureTransactionResult = response.Message;
                result.NewPaymentStatus = PaymentStatus.Paid;
            }

            return result;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //nothing
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            //var result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart,
            //    _qualpaySettings.AdditionalFee, _qualpaySettings.AdditionalFeePercentage);

            //return result;
            return _paymentService.CalculateAdditionalFee(cart,
                _qualpaySettings.AdditionalFee, _qualpaySettings.AdditionalFeePercentage);
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            //capture full amount of the authorized transaction
            var captureResponse = _qualpayManager.CaptureTransaction(new CaptureRequest
            {
                TransactionId = capturePaymentRequest.Order.AuthorizationTransactionId,
                Amount = Math.Round(capturePaymentRequest.Order.OrderTotal, 2)
            });

            //request succeeded
            return new CapturePaymentResult
            {
                CaptureTransactionId = captureResponse.TransactionId,
                CaptureTransactionResult = captureResponse.Message,
                NewPaymentStatus = PaymentStatus.Paid
            };
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            //refund full or partial amount of the captured transaction
            var refundResponse = _qualpayManager.Refund(new RefundRequest
            {
                TransactionId = refundPaymentRequest.Order.CaptureTransactionId,
                Amount = Math.Round(refundPaymentRequest.AmountToRefund, 2)
            });

            //request succeeded
            return new RefundPaymentResult
            {
                NewPaymentStatus = refundPaymentRequest.IsPartialRefund ? PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded
            };
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            //void full amount of the authorized transaction
            var voidResponse = _qualpayManager.VoidTransaction(new VoidRequest
            {
                TransactionId = voidPaymentRequest.Order.AuthorizationTransactionId
            });

            //request succeeded
            return new VoidPaymentResult
            {
                NewPaymentStatus = PaymentStatus.Voided
            };
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            //create subscription for recurring billing
            var subscription = _qualpayManager.CreateSubscription(CreateSubscriptionRequest(processPaymentRequest));
            if (subscription?.SubscriptionId == null || subscription.TransactionResponse == null)
                throw new NopException("Qualpay Payment Gateway error: Failed to create recurring payment.");

            //request succeeded
            return new ProcessPaymentResult
            {
                SubscriptionTransactionId = subscription.SubscriptionId.ToString(),
                AuthorizationTransactionCode = subscription.TransactionResponse.AuthorizationCode,
                AuthorizationTransactionId = subscription.TransactionResponse.TransactionId,
                CaptureTransactionId = subscription.TransactionResponse.TransactionId,
                CaptureTransactionResult = subscription.TransactionResponse.Message,
                AuthorizationTransactionResult = subscription.TransactionResponse.Message,
                AvsResult = subscription.TransactionResponse.AvsResult,
                Cvv2Result = subscription.TransactionResponse.Cvv2Result,
                NewPaymentStatus = PaymentStatus.Paid
            };
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            //try to cancel recurring payment
            var cancelled = _qualpayManager
                .CancelSubscription(cancelPaymentRequest.Order.CustomerId.ToString(), cancelPaymentRequest.Order.SubscriptionTransactionId);

            if (!cancelled)
                throw new NopException("Qualpay Payment Gateway error: Failed to cancel recurring payment.");

            return new CancelRecurringPaymentResult();
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //let's ensure that at least 5 seconds passed after order is placed
            //P.S. there's no any particular reason for that. we just do it
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 5)
                return false;

            return true;
        }

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>List of validating errors</returns>
        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            if (_qualpaySettings.UseEmbeddedFields)
            {
                //try to get errors from Qualpay card tokenization
                if (form.TryGetValue("Errors", out StringValues errorsString) && !StringValues.IsNullOrEmpty(errorsString))
                    return errorsString.ToString().Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            else
            {
                //validate payment info (custom validation)
                var validationResult = new QualpayPaymentInfoValidator(_localizationService).Validate(new PaymentInfoModel
                {
                    CardholderName = form["CardholderName"],
                    CardNumber = form["CardNumber"],
                    CardCode = form["CardCode"],
                    ExpireMonth = form["ExpireMonth"],
                    ExpireYear = form["ExpireYear"],
                    BillingCardId = form["BillingCardId"],
                    SaveCardDetails = form.TryGetValue("SaveCardDetails", out StringValues saveCardDetails) &&
                        bool.TryParse(saveCardDetails.FirstOrDefault(), out bool saveCard) && saveCard
                });
                if (!validationResult.IsValid)
                    return validationResult.Errors.Select(error => error.ErrorMessage).ToList();
            }

            return new List<string>();
        }

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>Payment info holder</returns>
        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var paymentRequest = new ProcessPaymentRequest();

            //pass custom values to payment processor
            var cardId = form["BillingCardId"];
            if (!StringValues.IsNullOrEmpty(cardId) && !cardId.FirstOrDefault().Equals(Guid.Empty.ToString()))
                paymentRequest.CustomValues.Add(_localizationService.GetResource("Plugins.Payments.Qualpay.Customer.Card"), cardId.FirstOrDefault());

            var saveCardDetails = form["SaveCardDetails"];
            if (!StringValues.IsNullOrEmpty(saveCardDetails) && bool.TryParse(saveCardDetails.FirstOrDefault(), out bool saveCard) && saveCard)
                paymentRequest.CustomValues.Add(_localizationService.GetResource("Plugins.Payments.Qualpay.Customer.Card.Save"), true);

            if (_qualpaySettings.UseEmbeddedFields)
            {
                //card details is already validated and tokenized by Qualpay
                var tokenizedCardId = form["TokenizedCardId"];
                if (!StringValues.IsNullOrEmpty(tokenizedCardId))
                    paymentRequest.CustomValues.Add(_localizationService.GetResource("Plugins.Payments.Qualpay.Customer.Card.Token"), tokenizedCardId.FirstOrDefault());
            }
            else
            {
                //set card details
                paymentRequest.CreditCardName = form["CardholderName"];
                paymentRequest.CreditCardNumber = form["CardNumber"];
                paymentRequest.CreditCardExpireMonth = int.Parse(form["ExpireMonth"]);
                paymentRequest.CreditCardExpireYear = int.Parse(form["ExpireYear"]);
                paymentRequest.CreditCardCvv2 = form["CardCode"];
            }

            return paymentRequest;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/Qualpay/Configure";
        }

        /// <summary>
        /// Gets a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <param name="viewComponentName">View component name</param>
        public string GetPublicViewComponentName()
        {
            return QualpayDefaults.ViewComponentName;
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //settings
            _settingService.SaveSetting(new QualpaySettings
            {
                UseSandbox = true,
                UseEmbeddedFields = true,
                UseCustomerVault = true,
                PaymentTransactionType = TransactionType.Sale
            });

            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("Enums.Nop.Plugin.Payments.Qualpay.Domain.Authorization", "Authorization");
            _localizationService.AddOrUpdatePluginLocaleResource("Enums.Nop.Plugin.Payments.Qualpay.Domain.Sale", "Sale (authorization and capture)");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Customer", "Qualpay Vault Customer");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Customer.Card", "Use a previously saved card");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Customer.Card.ExpirationDate", "Expiration date");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Customer.Card.Id", "ID");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Customer.Card.MaskedNumber", "Card number");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Customer.Card.Save", "Add the card to Qualpay Vault for next time");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Customer.Card.Select", "Select a card");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Customer.Card.Token", "Use a tokenized card");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Customer.Card.Type", "Type");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Customer.Create", "Add to Vault");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Customer.Hint", "Qualpay Vault Customer ID");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Customer.NotExists", "The customer is not yet in the Qualpay Customer Vault");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.AdditionalFee", "Additional fee");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.AdditionalFeePercentage", "Additional fee. Use percentage");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.AdditionalFeePercentage.Hint", "Determine whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.MerchantEmail", "Email");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.MerchantEmail.Hint", "Enter your email to subscribe to Qualpay news.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.MerchantId", "Merchant ID");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.MerchantId.Hint", "Specify your Qualpay merchant identifier.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.PaymentTransactionType", "Transaction type");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.PaymentTransactionType.Hint", "Choose payment transaction type.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.SecurityKey", "Security key");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.SecurityKey.Hint", "Specify your Qualpay security key.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.UseCustomerVault", "Use Customer Vault");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.UseCustomerVault.Hint", "Determine whether to use Qualpay Customer Vault feature. The Customer Vault reduces the amount of associated payment data that touches your servers and enables subsequent payment billing information to be fulfilled by Qualpay.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.UseEmbeddedFields", "Use Embedded Fields");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.UseEmbeddedFields.Hint", "Determine whether to use Qualpay Embedded Fields feature. Your customer will remain on your website, but payment information is collected and processed on Qualpay servers. Since your server is not processing customer payment data, your PCI DSS compliance scope is greatly reduced.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.UseRecurringBilling", "Use Recurring Billing");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.UseRecurringBilling.Hint", "Determine whether to use Qualpay Recurring Billing feature. Support setting your customers up for recurring or subscription payments.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.UseSandbox", "Use Sandbox");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.UseSandbox.Hint", "Determine whether to enable sandbox (testing environment).");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.Webhook.Warning", "Webhook was not created (you'll not be able to handle recurring payments)");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.PaymentMethodDescription", "Pay by credit / debit card using Qualpay payment gateway");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Subscribe", "Stay informed");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Subscribe.Error", "An error has occurred, details in the log");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Subscribe.Success", "You have subscribed to Qualpay news");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Unsubscribe.Success", "You have unsubscribed from Qualpay news");

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<QualpaySettings>();

            //locales
            _localizationService.DeletePluginLocaleResource("Enums.Nop.Plugin.Payments.Qualpay.Domain.Authorization");
            _localizationService.DeletePluginLocaleResource("Enums.Nop.Plugin.Payments.Qualpay.Domain.Sale");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Customer");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Customer.Card");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Customer.Card.ExpirationDate");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Customer.Card.Id");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Customer.Card.MaskedNumber");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Customer.Card.Save");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Customer.Card.Select");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Customer.Card.Token");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Customer.Card.Type");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Customer.Create");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Customer.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Customer.NotExists");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.AdditionalFee");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.AdditionalFee.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.AdditionalFeePercentage");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.AdditionalFeePercentage.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.MerchantEmail");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.MerchantEmail.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.MerchantId");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.MerchantId.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.PaymentTransactionType");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.PaymentTransactionType.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.SecurityKey");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.SecurityKey.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.UseCustomerVault");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.UseCustomerVault.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.UseEmbeddedFields");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.UseEmbeddedFields.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.UseRecurringBilling");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.UseRecurringBilling.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.UseSandbox");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.UseSandbox.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.Webhook.Warning");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.PaymentMethodDescription");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Subscribe");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Subscribe.Error");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Subscribe.Success");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Unsubscribe.Success");

            base.Uninstall();
        }

        #endregion

        #region Properies

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get { return RecurringPaymentType.Automatic; }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get { return PaymentMethodType.Standard; }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public string PaymentMethodDescription
        {
            //return description of this payment method to be display on "payment method" checkout step. good practice is to make it localizable
            //for example, for a redirection payment method, description may be like this: "You will be redirected to Transaction site to complete the payment"
            get { return _localizationService.GetResource("Plugins.Payments.Qualpay.PaymentMethodDescription"); }
        }

        #endregion
    }
}