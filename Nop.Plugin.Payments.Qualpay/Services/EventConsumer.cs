using System;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Events;
using Nop.Plugin.Payments.Qualpay.Models.Customer;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.UI;

namespace Nop.Plugin.Payments.Qualpay.Services
{
    /// <summary>
    /// Represents plugin event consumer
    /// </summary>
    public class EventConsumer :
        IConsumer<AdminTabStripCreated>,
        IConsumer<EntityInserted<RecurringPayment>>,
        IConsumer<PageRenderingEvent>
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly PaymentSettings _paymentSettings;
        private readonly QualpayManager _qualpayManager;
        private readonly QualpaySettings _qualpaySettings;

        #endregion

        #region Ctor

        public EventConsumer(ICustomerService customerService,
            ILocalizationService localizationService,
            IOrderService orderService,
            IPaymentService paymentService,
            PaymentSettings paymentSettings,
            QualpayManager qualpayManager,
            QualpaySettings qualpaySettings)
        {
            this._customerService = customerService;
            this._localizationService = localizationService;
            this._orderService = orderService;
            this._paymentService = paymentService;
            this._paymentSettings = paymentSettings;
            this._qualpayManager = qualpayManager;
            this._qualpaySettings = qualpaySettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handle admin tabstrip created event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        public void HandleEvent(AdminTabStripCreated eventMessage)
        {
            if (eventMessage?.Helper == null)
                return;

            //we need customer details page
            var qualpayCustomerTabId = "tab-qualpay";
            var customerTabsId = "customer-edit";
            if (!eventMessage.TabStripName.Equals(customerTabsId))
                return;

            //check whether the payment plugin is installed and is active
            if (!_paymentService.LoadPaymentMethodBySystemName(QualpayDefaults.SystemName)?.IsPaymentMethodActive(_paymentSettings) ?? true)
                return;

            //whether Qualpay Customer Vault feature is enabled
            if (!_qualpaySettings.UseCustomerVault)
                return;

            //get the view model
            if (!(eventMessage.Helper.ViewData.Model is CustomerModel customerModel))
                return;

            //check whether a customer exists and isn't guest
            var customer = _customerService.GetCustomerById(customerModel.Id);
            if (customer?.IsGuest() ?? true)
                return;

            //try to get a customer from the Vault 
            var vaultCustomer = _qualpayManager.GetCustomerById(customer.Id.ToString());

            //prepare model
            var model = new QualpayCustomerModel
            {
                Id = customerModel.Id,
                CustomerExists = vaultCustomer != null,
                QualpayCustomerId = vaultCustomer?.CustomerId
            };

            //compose script to create a new tab
            var qualpayCustomerTab = new HtmlString($@"
                <script type='text/javascript'>
                    $(document).ready(function() {{
                        $(`
                            <li>
                                <a data-tab-name='{qualpayCustomerTabId}' data-toggle='tab' href='#{qualpayCustomerTabId}'>
                                    {_localizationService.GetResource("Plugins.Payments.Qualpay.Customer")}
                                </a>
                            </li>
                        `).appendTo('#{customerTabsId} .nav-tabs:first');
                        $(`
                            <div class='tab-pane' id='{qualpayCustomerTabId}'>
                                {
                                    eventMessage.Helper.Partial("~/Plugins/Payments.Qualpay/Views/Customer/_CreateOrUpdate.Qualpay.cshtml", model).RenderHtmlContent()
                                        .Replace("</script>", "<\\/script>") //we need escape a closing script tag to prevent terminating the script block early
                                }
                            </div>
                        `).appendTo('#{customerTabsId} .tab-content:first');
                    }});
                </script>");

            //add this tab as a block to render on the customer details page
            eventMessage.BlocksToRender.Add(qualpayCustomerTab);
        }

        /// <summary>
        /// Recurring payment inserted event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        public void HandleEvent(EntityInserted<RecurringPayment> eventMessage)
        {
            var recurringPayment = eventMessage?.Entity;
            if (recurringPayment == null)
                return;

            //add first payment to history right after creating recurring payment
            recurringPayment.RecurringPaymentHistory.Add(new RecurringPaymentHistory
            {
                RecurringPayment = recurringPayment,
                CreatedOnUtc = DateTime.UtcNow,
                OrderId = recurringPayment.InitialOrderId,
            });
            _orderService.UpdateRecurringPayment(recurringPayment);
        }

        /// <summary>
        /// Handle page rendering event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        public void HandleEvent(PageRenderingEvent eventMessage)
        {
            if (eventMessage?.Helper?.ViewContext == null)
                return;

            //check whether the payment plugin is installed and is active
            if (!_paymentService.LoadPaymentMethodBySystemName(QualpayDefaults.SystemName)?.IsPaymentMethodActive(_paymentSettings) ?? true)
                return;

            //add Embedded Fields sсript and styles to the one page checkout
            var matchedRoutes = eventMessage.Helper.ViewContext.RouteData.Routers.OfType<INamedRouter>();
            if (matchedRoutes.Any(route => route.Name.Equals(QualpayDefaults.OnePageCheckoutRouteName)))
            {
                eventMessage.Helper.AddScriptParts(ResourceLocation.Footer, QualpayDefaults.EmbeddedFieldsScriptPath, excludeFromBundle: true);
                eventMessage.Helper.AddCssFileParts(QualpayDefaults.EmbeddedFieldsStylePath, excludeFromBundle: true);
            }
        }

        #endregion
    }
}