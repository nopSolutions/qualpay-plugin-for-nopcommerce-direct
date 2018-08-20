using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.Qualpay.Models;
using Nop.Plugin.Payments.Qualpay.Services;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.Qualpay.Components
{
    /// <summary>
    /// Represents payment info view component
    /// </summary>
    [ViewComponent(Name = QualpayDefaults.ViewComponentName)]
    public class QualpayViewComponent : NopViewComponent
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly QualpayManager _qualpayManager;
        private readonly QualpaySettings _qualpaySettings;

        #endregion

        #region Ctor

        public QualpayViewComponent(ILocalizationService localizationService,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            IWorkContext workContext,
            QualpayManager qualpayManager,
            QualpaySettings qualpaySettings)
        {
            this._localizationService = localizationService;
            this._shoppingCartService = shoppingCartService;
            this._storeContext = storeContext;
            this._workContext = workContext;
            this._qualpayManager = qualpayManager;
            this._qualpaySettings = qualpaySettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <returns>View component result</returns>
        public IViewComponentResult Invoke()
        {
            var model = new PaymentInfoModel();

            //prepare years
            for (var i = 0; i < 15; i++)
            {
                var year = (DateTime.Now.Year + i).ToString();
                model.ExpireYears.Add(new SelectListItem { Text = year, Value = year, });
            }

            //prepare months
            for (var i = 1; i <= 12; i++)
            {
                model.ExpireMonths.Add(new SelectListItem { Text = i.ToString("D2"), Value = i.ToString(), });
            }

            //get transient key for Qualpay Embedded Fields
            if (_qualpaySettings.UseEmbeddedFields)
                model.TransientKey = _qualpayManager.GetTransientKey()?.TransientKey;

            //get parameters for Qualpay Customer Vault
            if (_qualpaySettings.UseCustomerVault)
            {
                //whether current customer is guest
                model.IsGuest = _workContext.CurrentCustomer.IsGuest();
                if (!model.IsGuest)
                {
                    //try to get customer billing cards
                    model.BillingCards = _qualpayManager.GetCustomerCards(_workContext.CurrentCustomer.Id.ToString())
                        ?.Where(card => card != null)
                        ?.Select(card => new SelectListItem { Text = card.CardNumber, Value = card.CardId }).ToList()
                        ?? new List<SelectListItem>();

                    //add the special item for 'select card' with empty GUID value 
                    if (model.BillingCards.Any())
                    {
                        var selectCardText = _localizationService.GetResource("Plugins.Payments.Qualpay.Customer.Card.Select");
                        model.BillingCards.Insert(0, new SelectListItem { Text = selectCardText, Value = Guid.Empty.ToString() });
                    }
                }

                //whether it's a recurring order
                var currentShoppingCart = _workContext.CurrentCustomer.ShoppingCartItems
                    .Where(item => item.ShoppingCartType == ShoppingCartType.ShoppingCart)
                    .LimitPerStore(_storeContext.CurrentStore.Id).ToList();
                //model.IsRecurringOrder = currentShoppingCart.IsRecurring();
                model.IsRecurringOrder = _shoppingCartService.ShoppingCartIsRecurring(currentShoppingCart);
            }

            return View("~/Plugins/Payments.Qualpay/Views/PaymentInfo.cshtml", model);
        }

        #endregion
    }
}