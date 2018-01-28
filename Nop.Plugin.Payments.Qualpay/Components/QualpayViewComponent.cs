using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Payments.Qualpay.Models;
using Nop.Plugin.Payments.Qualpay.Services;
using Nop.Services.Localization;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.Qualpay.Components
{
    [ViewComponent(Name = QualpayDefaults.ViewComponentName)]
    public class QualpayViewComponent : NopViewComponent
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly QualpayManager _qualpayManager;

        #endregion

        #region Ctor

        public QualpayViewComponent(ILocalizationService localizationService, 
            IWorkContext workContext,
            QualpayManager qualpayManager)
        {
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._qualpayManager = qualpayManager;
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
            
            //whether current customer is guest
            model.IsGuest = _workContext.CurrentCustomer.IsGuest();
            if (!model.IsGuest)
            {
                //try to get customer billing cards
                try
                {
                    var vaultCustomer = _qualpayManager.GetCustomerCards(_workContext.CurrentCustomer.Id.ToString())?.VaultCustomer;
                    model.BillingCards = vaultCustomer?.BillingCards?.Where(card => card != null)
                        .Select(card => new SelectListItem { Text = card.CardNumber, Value = card.CardId }).ToList()
                        ?? new List<SelectListItem>();
                }
                catch { }                

                //add the special item for 'select card' with empty GUID value 
                if (model.BillingCards.Any())
                {
                    var selectCardText = _localizationService.GetResource("Plugins.Payments.Qualpay.Customer.Card.Select");
                    model.BillingCards.Insert(0, new SelectListItem { Text = selectCardText, Value = Guid.Empty.ToString() });
                }
            }

            return View("~/Plugins/Payments.Qualpay/Views/PaymentInfo.cshtml", model);
        }

        #endregion
    }
}