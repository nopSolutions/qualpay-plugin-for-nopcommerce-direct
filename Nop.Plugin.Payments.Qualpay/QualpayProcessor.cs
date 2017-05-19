using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Nop.Core;
using nop = Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.Qualpay.Controllers;
using Nop.Plugin.Payments.Qualpay.Domain;
using Nop.Plugin.Payments.Qualpay.Helpers;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
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

        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPaymentService _paymentService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ISettingService _settingService;
        private readonly ITaxService _taxService;
        private readonly QualpaySettings _qualpaySettings;

        #endregion

        #region Ctor

        public QualpayProcessor(ICheckoutAttributeParser checkoutAttributeParser,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            ILogger logger,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPaymentService paymentService,
            IPriceCalculationService priceCalculationService,
            IProductAttributeParser productAttributeParser,
            ISettingService settingService,
            ITaxService taxService,
            QualpaySettings qualpaySettings)
        {
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._currencyService = currencyService;
            this._customerService = customerService;
            this._genericAttributeService = genericAttributeService;
            this._localizationService = localizationService;
            this._logger = logger;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._paymentService = paymentService;
            this._priceCalculationService = priceCalculationService;
            this._productAttributeParser = productAttributeParser;
            this._settingService = settingService;
            this._taxService = taxService;
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
        protected IList<LineItem> GetItems(nop.Customer customer, int storeId, decimal orderTotal, out decimal taxAmount)
        {
            var items = new List<LineItem>();

            //get current shopping cart
            var shoppingCart = customer.ShoppingCartItems
                .Where(shoppingCartItem => shoppingCartItem.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(storeId).ToList();

            //set tax amount
            taxAmount = _orderTotalCalculationService.GetTaxTotal(shoppingCart);

            //create transaction items from shopping cart items
            decimal taxRate;
            items.AddRange(shoppingCart.Where(shoppingCartItem => shoppingCartItem.Product != null).Select(shoppingCartItem =>
            {
                //item price
                var price = _taxService.GetProductPrice(shoppingCartItem.Product, _priceCalculationService.GetUnitPrice(shoppingCartItem),
                    false, shoppingCartItem.Customer, out taxRate);

                return CreateItem(price, shoppingCartItem.Product.Name, 
                    shoppingCartItem.Product.FormatSku(shoppingCartItem.AttributesXml, _productAttributeParser), shoppingCartItem.Quantity);
            }));

            //create transaction items from checkout attributes
            var checkoutAttributesXml = customer.GetAttribute<string>(nop.SystemCustomerAttributeNames.CheckoutAttributes, storeId);
            if (!string.IsNullOrEmpty(checkoutAttributesXml))
            {
                var attributeValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(checkoutAttributesXml);
                items.AddRange(attributeValues.Where(attributeValue => attributeValue.CheckoutAttribute != null).Select(attributeValue =>
                {
                    return CreateItem(_taxService.GetCheckoutAttributePrice(attributeValue, false, customer), 
                        string.Format("{0} ({1})", attributeValue.CheckoutAttribute.Name, attributeValue.Name), "checkout");
                }));
            }

            //create transaction item for payment method additional fee
            var paymentAdditionalFee = _paymentService.GetAdditionalHandlingFee(shoppingCart, PluginDescriptor.SystemName);
            var paymentPrice = _taxService.GetPaymentMethodAdditionalFee(paymentAdditionalFee, false, customer);
            if (paymentPrice > decimal.Zero)
                items.Add(CreateItem(paymentPrice, string.Format("Payment ({0})", PluginDescriptor.FriendlyName), "payment"));

            //create transaction item for shipping rate
            if (shoppingCart.RequiresShipping())
            {
                var shippingPrice = _orderTotalCalculationService.GetShoppingCartShippingTotal(shoppingCart, false);
                if (shippingPrice.HasValue && shippingPrice.Value > decimal.Zero)
                    items.Add(CreateItem(shippingPrice.Value, "Shipping rate", "shipping"));
            }

            //create transaction item for all discounts
            var amountDifference = orderTotal - items.Sum(lineItem => lineItem.UnitPrice * lineItem.Quantity) - taxAmount;
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
        protected LineItem CreateItem(decimal price, string description, string productCode, int quantity = 1)
        {
            return new LineItem
            {
                CreditType = ItemCreditType.Debit,
                Description = description,
                MeasureUnit = "*",
                ProductCode = productCode,
                Quantity = quantity,
                UnitPrice = price
            };
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
            var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);
            if (customer == null)
                throw new NopException("Customer cannot be loaded");

            var usdCurrency = _currencyService.GetCurrencyByCode("USD");
            if (usdCurrency == null)
                throw new NopException("USD currency cannot be loaded");

            //create request
            var qualpayRequest = new QualpayRequest();

            //set order number, max length is 25 
            qualpayRequest.PurchaseId = processPaymentRequest.OrderGuid.ToString().Substring(0, 25);

            //set amount in USD 
            var amount = _currencyService.ConvertFromPrimaryStoreCurrency(processPaymentRequest.OrderTotal, usdCurrency);
            qualpayRequest.Amount = Math.Round(amount, 2);
            qualpayRequest.CurrencyIsoCode = 840; // numeric ISO code of USD

            //add item lines
            decimal taxAmount;
            qualpayRequest.Items = GetItems(customer, processPaymentRequest.StoreId, 
                processPaymentRequest.OrderTotal, out taxAmount).ToArray();

            //set amount of items in USD 
            foreach (var item in qualpayRequest.Items)
            {
                var usdPrice = _currencyService.ConvertFromPrimaryStoreCurrency(item.UnitPrice, usdCurrency);
                item.UnitPrice = Math.Round(usdPrice, 2);
            }

            //set amount
            taxAmount = _currencyService.ConvertFromPrimaryStoreCurrency(taxAmount, usdCurrency);
            qualpayRequest.TaxAmount = Math.Round(taxAmount, 2);

            //parse custom values
            var useStoredCardKey = _localizationService.GetResource("Plugins.Payments.Qualpay.UseStoredCard");
            var useStoredCard = processPaymentRequest.CustomValues.ContainsKey(useStoredCardKey) &&
                Convert.ToBoolean(processPaymentRequest.CustomValues[useStoredCardKey]);

            var saveCardKey = _localizationService.GetResource("Plugins.Payments.Qualpay.SaveCardDetails");
            var saveCard = processPaymentRequest.CustomValues.ContainsKey(saveCardKey) &&
                Convert.ToBoolean(processPaymentRequest.CustomValues[saveCardKey]);

            var cardId = customer.GetAttribute<string>("QualpayVaultCardId", _genericAttributeService, processPaymentRequest.StoreId);
            if (useStoredCard)
            {
                //customer has stored card and want to use it
                qualpayRequest.CardId = cardId;
            }
            else
            {
                //or he sets card details
                qualpayRequest.CardholderName = processPaymentRequest.CreditCardName;
                qualpayRequest.CardNumber = processPaymentRequest.CreditCardNumber;
                qualpayRequest.Cvv2 = processPaymentRequest.CreditCardCvv2;
                qualpayRequest.ExpirationDate = string.Format("{0}{1}", processPaymentRequest.CreditCardExpireMonth.ToString("D2"),
                    processPaymentRequest.CreditCardExpireYear.ToString().Substring(2));
                //set billing address, max length is 20
                qualpayRequest.AvsAddress = customer.BillingAddress.Return(address => CommonHelper.EnsureMaximumLength(address.Address1, 20), null);
                qualpayRequest.AvsZipCode = customer.BillingAddress.Return(address => address.ZipPostalCode, null);

                //save or update credit card details in Qualpay Vault
                if (saveCard)
                {
                    qualpayRequest.IsTokenize = true;

                    //and customer details if not exist
                    if (string.IsNullOrEmpty(cardId))
                    {
                        qualpayRequest.CustomerId = customer.Id.ToString();
                        qualpayRequest.Customer = new Customer
                        {
                            CustomerEmail = customer.BillingAddress.Return(address => address.Email, null),
                            CustomerFirstName = customer.BillingAddress.Return(address => address.FirstName, null),
                            CustomerLastName = customer.BillingAddress.Return(address => address.LastName, null),
                            CustomerPhone = customer.BillingAddress.Return(address => address.PhoneNumber, null)
                        };
                    }
                }

            }

            //get response
            var response = QualpayHelper.PostRequest(qualpayRequest, 
                _qualpaySettings.PaymentTransactionType, null, _qualpaySettings, _logger);
            if (response == null)
                return new ProcessPaymentResult { Errors = new[] { "Qualpay Payment Gateway error" } };

            //request failed
            if (response.ResponseCode != ResponseCode.Success)
                return new ProcessPaymentResult { Errors = new[] { response.ResponseMessage } };

            //request succeeded
            var result = new ProcessPaymentResult
            {
                AvsResult = response.AvsResult,
                AuthorizationTransactionCode = response.AuthorizationCode
            };

            //set authorization details
            if (_qualpaySettings.PaymentTransactionType == QualpayRequestType.Authorization)
            {
                result.AuthorizationTransactionId = response.TransactionId;
                result.AuthorizationTransactionResult = response.ResponseMessage;
                result.NewPaymentStatus = PaymentStatus.Authorized;
            }

            //or capture details
            if (_qualpaySettings.PaymentTransactionType == QualpayRequestType.Sale)
            {   
                result.CaptureTransactionId = response.TransactionId;
                result.CaptureTransactionResult = response.ResponseMessage;
                result.NewPaymentStatus = PaymentStatus.Paid;
            }

            //save Qualpay Vault card ID
            if (saveCard)
                _genericAttributeService.SaveAttribute(customer, "QualpayVaultCardId", response.CardId, processPaymentRequest.StoreId);

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
            var result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart,
                _qualpaySettings.AdditionalFee, _qualpaySettings.AdditionalFeePercentage);

            return result;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var usdCurrency = _currencyService.GetCurrencyByCode("USD");
            if (usdCurrency == null)
                throw new NopException("USD currency cannot be loaded");

            //create request
            var qualpayRequest = new QualpayRequest();

            //capture full amount of the authorization 
            var amount = _currencyService.ConvertFromPrimaryStoreCurrency(capturePaymentRequest.Order.OrderTotal, usdCurrency);
            qualpayRequest.Amount = Math.Round(amount, 2);
            qualpayRequest.CurrencyIsoCode = 840; // numeric ISO code of USD

            //get response
            var response = QualpayHelper.PostRequest(qualpayRequest, QualpayRequestType.Capture, 
                capturePaymentRequest.Order.AuthorizationTransactionId, _qualpaySettings, _logger);
            if (response == null)
                return new CapturePaymentResult { Errors = new[] { "Qualpay Payment Gateway error" } };

            //request failed
            if (response.ResponseCode != ResponseCode.Success)
                return new CapturePaymentResult { Errors = new[] { response.ResponseMessage } };

            //request succeeded
            var result = new CapturePaymentResult
            {
                CaptureTransactionId = response.TransactionId,
                CaptureTransactionResult = response.ResponseMessage,
                NewPaymentStatus = PaymentStatus.Paid
            };

            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var usdCurrency = _currencyService.GetCurrencyByCode("USD");
            if (usdCurrency == null)
                throw new NopException("USD currency cannot be loaded");

            //create request
            var qualpayRequest = new QualpayRequest();

            //set amount in USD
            var amount = _currencyService.ConvertFromPrimaryStoreCurrency(refundPaymentRequest.AmountToRefund, usdCurrency);
            qualpayRequest.Amount = Math.Round(amount, 2);
            qualpayRequest.CurrencyIsoCode = 840; // numeric ISO code of USD

            //get response
            var response = QualpayHelper.PostRequest(qualpayRequest, QualpayRequestType.Refund,
                refundPaymentRequest.Order.CaptureTransactionId, _qualpaySettings, _logger);
            if (response == null)
                return new RefundPaymentResult { Errors = new[] { "Qualpay Payment Gateway error" } };

            //request failed
            if (response.ResponseCode != ResponseCode.Success)
                return new RefundPaymentResult { Errors = new[] { response.ResponseMessage } };

            //request succeeded
            var result = new RefundPaymentResult
            {
                NewPaymentStatus = refundPaymentRequest.IsPartialRefund ? PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded
            };

            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var usdCurrency = _currencyService.GetCurrencyByCode("USD");
            if (usdCurrency == null)
                throw new NopException("USD currency cannot be loaded");

            //create request
            var qualpayRequest = new QualpayRequest();

            //set amount in USD
            var amount = _currencyService.ConvertFromPrimaryStoreCurrency(voidPaymentRequest.Order.OrderTotal, usdCurrency);
            qualpayRequest.Amount = Math.Round(amount, 2);
            qualpayRequest.CurrencyIsoCode = 840; // numeric ISO code of USD

            //get response
            var response = QualpayHelper.PostRequest(qualpayRequest, QualpayRequestType.Void,
                voidPaymentRequest.Order.AuthorizationTransactionId, _qualpaySettings, _logger);
            if (response == null)
                return new VoidPaymentResult { Errors = new[] { "Qualpay Payment Gateway error" } };

            //request failed
            if (response.ResponseCode != ResponseCode.Success)
                return new VoidPaymentResult { Errors = new[] { response.ResponseMessage } };

            //request succeeded
            var result = new VoidPaymentResult
            {
                NewPaymentStatus = PaymentStatus.Voided
            };

            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");
            
            //let's ensure that at least 5 seconds passed after order is placed
            //P.S. there's no any particular reason for that. we just do it
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 5)
                return false;

            return true;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "Qualpay";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.Qualpay.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "Qualpay";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.Qualpay.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Get type of the controller
        /// </summary>
        /// <returns>Controller type</returns>
        public Type GetControllerType()
        {
            return typeof(QualpayController);
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //settings
            _settingService.SaveSetting(new QualpaySettings
            {
                UseSandbox = true
            });

            //locales
            this.AddOrUpdatePluginLocaleResource("Enums.QualpayRequestType.Authorization", "Authorization");
            this.AddOrUpdatePluginLocaleResource("Enums.QualpayRequestType.Sale", "Sale (authorization and capture)");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.AdditionalFeePercentage", "Additional fee. Use percentage");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.MerchantId", "Merchant ID");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.MerchantId.Hint", "Specify your Qualpay merchant identifier.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.PaymentTransactionType", "Payment transaction type");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.PaymentTransactionType.Hint", "Choose payment transaction type");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.SecurityKey", "Security key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.SecurityKey.Hint", "Specify your Qualpay payment gateway security key.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.UseSandbox", "Use Sandbox");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.Fields.UseSandbox.Hint", "Check to enable sandbox (testing environment).");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.SaveCardDetails", "Save the card data to Qualpay Vault for next time");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Qualpay.UseStoredCard", "Use a previously saved card");

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
            this.DeletePluginLocaleResource("Enums.QualpayRequestType.Authorization");
            this.DeletePluginLocaleResource("Enums.QualpayRequestType.Sale");
            this.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.AdditionalFee");
            this.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.AdditionalFee.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.AdditionalFeePercentage");
            this.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.AdditionalFeePercentage.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.MerchantId");
            this.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.MerchantId.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.PaymentTransactionType");
            this.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.PaymentTransactionType.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.SecurityKey");
            this.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.SecurityKey.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.UseSandbox");
            this.DeletePluginLocaleResource("Plugins.Payments.Qualpay.Fields.UseSandbox.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Qualpay.SaveCardDetails");
            this.DeletePluginLocaleResource("Plugins.Payments.Qualpay.UseStoredCard");
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
            get { return RecurringPaymentType.NotSupported; }
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

        #endregion
    }
}
