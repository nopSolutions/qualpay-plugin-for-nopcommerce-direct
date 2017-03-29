using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.Qualpay.Domain;
using Nop.Plugin.Payments.Qualpay.Models;
using Nop.Plugin.Payments.Qualpay.Validators;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.Qualpay.Controllers
{
    public class QualpayController : BasePaymentController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public QualpayController(ILocalizationService localizationService,
            ISettingService settingService,
            IStoreContext storeContext,
            IStoreService storeService,
            IWorkContext workContext)
        {
            this._localizationService = localizationService;
            this._settingService = settingService;
            this._storeService = storeService;
            this._storeContext = storeContext;
            this._workContext = workContext;
        }

        #endregion

        #region Methods

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<QualpaySettings>(storeScope);

            //prepare model
            var model = new ConfigurationModel
            {
                MerchantId = settings.MerchantId,
                SecurityKey = settings.SecurityKey,
                UseSandbox = settings.UseSandbox,
                PaymentTransactionTypeId = (int)settings.PaymentTransactionType,
                AdditionalFee = settings.AdditionalFee,
                AdditionalFeePercentage = settings.AdditionalFeePercentage,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.SecurityKey_OverrideForStore = _settingService.SettingExists(settings, x => x.SecurityKey, storeScope);
                model.UseSandbox_OverrideForStore = _settingService.SettingExists(settings, x => x.UseSandbox, storeScope);
                model.PaymentTransactionTypeId_OverrideForStore = _settingService.SettingExists(settings, x => x.PaymentTransactionType, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(settings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(settings, x => x.AdditionalFeePercentage, storeScope);
            }

            //prepare payment transaction modes
            model.PaymentTransactionTypes.Add(new SelectListItem
            {
                Text = QualpayRequestType.Authorization.GetLocalizedEnum(_localizationService, _workContext),
                Value = ((int)QualpayRequestType.Authorization).ToString()
            });
            model.PaymentTransactionTypes.Add(new SelectListItem
            {
                Text = QualpayRequestType.Sale.GetLocalizedEnum(_localizationService, _workContext),
                Value = ((int)QualpayRequestType.Sale).ToString()
            });

            return View("~/Plugins/Payments.Qualpay/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<QualpaySettings>(storeScope);

            //save settings
            settings.MerchantId = model.MerchantId;
            settings.SecurityKey = model.SecurityKey;
            settings.UseSandbox = model.UseSandbox;
            settings.PaymentTransactionType = (QualpayRequestType)model.PaymentTransactionTypeId;
            settings.AdditionalFee = model.AdditionalFee;
            settings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSetting(settings, x => x.MerchantId, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(settings, x => x.SecurityKey, model.SecurityKey_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(settings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(settings, x => x.PaymentTransactionType, model.PaymentTransactionTypeId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(settings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(settings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            var model = new PaymentInfoModel();

            //years
            for (var i = 0; i < 15; i++)
            {
                var year = (DateTime.Now.Year + i).ToString();
                model.ExpireYears.Add(new SelectListItem
                {
                    Text = year,
                    Value = year,
                });
            }

            //months
            for (var i = 1; i <= 12; i++)
            {
                model.ExpireMonths.Add(new SelectListItem
                {
                    Text = i.ToString("D2"),
                    Value = i.ToString(),
                });
            }

            //set postback values
            model.CardholderName = Request.Form["CardholderName"];
            model.CardNumber = Request.Form["CardNumber"];
            model.CardCode = Request.Form["CardCode"];
            var selectedMonth = model.ExpireMonths.FirstOrDefault(x => x.Value.Equals(Request.Form["ExpireMonth"], StringComparison.InvariantCultureIgnoreCase));
            if (selectedMonth != null)
                selectedMonth.Selected = true;
            var selectedYear = model.ExpireYears.FirstOrDefault(x => x.Value.Equals(Request.Form["ExpireYear"], StringComparison.InvariantCultureIgnoreCase));
            if (selectedYear != null)
                selectedYear.Selected = true;

            //whether to save card details in Qualpay Vault
            var saveCardDetails = false;
            if (Request.Form.AllKeys.Contains("SaveCardDetails"))
                bool.TryParse(Request.Form.GetValues("SaveCardDetails")[0], out saveCardDetails);
            model.SaveCardDetails = saveCardDetails;

            //check whether customer already has stored card
            var storedCardId = _workContext.CurrentCustomer.GetAttribute<string>("QualpayVaultCardId", _storeContext.CurrentStore.Id);
            var useStoredId = !string.IsNullOrEmpty(storedCardId);
            if (Request.Form.AllKeys.Contains("UseStoredCard"))
                bool.TryParse(Request.Form.GetValues("UseStoredCard")[0], out useStoredId);
            model.UseStoredCard = useStoredId;

            return View("~/Plugins/Payments.Qualpay/Views/PaymentInfo.cshtml", model);
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();

            //validate
            var validator = new PaymentInfoValidator(_localizationService);
            var model = new PaymentInfoModel
            {
                CardholderName = form["CardholderName"],
                CardNumber = form["CardNumber"],
                CardCode = form["CardCode"],
                ExpireMonth = form["ExpireMonth"],
                ExpireYear = form["ExpireYear"]
            };

            //don't validate card details on using stored card
            var useStoredCard = false;
            if (form.AllKeys.Contains("UseStoredCard"))
                bool.TryParse(form.GetValues("UseStoredCard")[0], out useStoredCard);
            model.UseStoredCard = useStoredCard;

            var validationResult = validator.Validate(model);
            if (!validationResult.IsValid)
                warnings.AddRange(validationResult.Errors.Select(error => error.ErrorMessage));

            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentRequest = new ProcessPaymentRequest
            {
                CreditCardName = form["CardholderName"],
                CreditCardNumber = form["CardNumber"],
                CreditCardExpireMonth = int.Parse(form["ExpireMonth"]),
                CreditCardExpireYear = int.Parse(form["ExpireYear"]),
                CreditCardCvv2 = form["CardCode"]
            };

            //pass custom values to payment processor
            var saveCardDetails = false;
            if (form.AllKeys.Contains("SaveCardDetails") && bool.TryParse(form.GetValues("SaveCardDetails")[0], out saveCardDetails) && saveCardDetails)
                paymentRequest.CustomValues.Add(_localizationService.GetResource("Plugins.Payments.Qualpay.SaveCardDetails"), true);

            var useStoredCard = false;
            if (form.AllKeys.Contains("UseStoredCard") && bool.TryParse(form.GetValues("UseStoredCard")[0], out useStoredCard) && useStoredCard)
                paymentRequest.CustomValues.Add(_localizationService.GetResource("Plugins.Payments.Qualpay.UseStoredCard"), true);

            return paymentRequest;
        }

        #endregion
    }
}