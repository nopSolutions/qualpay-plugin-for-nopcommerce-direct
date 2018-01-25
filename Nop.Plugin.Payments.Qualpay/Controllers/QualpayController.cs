using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Plugin.Payments.Qualpay.Domain;
using Nop.Plugin.Payments.Qualpay.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Controllers;

namespace Nop.Plugin.Payments.Qualpay.Controllers
{
    public class QualpayController : BaseAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public QualpayController(ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreService storeService,
            IWorkContext workContext)
        {
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._settingService = settingService;
            this._storeService = storeService;
            this._workContext = workContext;
        }

        #endregion

        #region Methods

        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

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
                Text = TransactionType.Authorization.GetLocalizedEnum(_localizationService, _workContext),
                Value = ((int)TransactionType.Authorization).ToString()
            });
            model.PaymentTransactionTypes.Add(new SelectListItem
            {
                Text = TransactionType.Sale.GetLocalizedEnum(_localizationService, _workContext),
                Value = ((int)TransactionType.Sale).ToString()
            });

            return View("~/Plugins/Payments.Qualpay/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<QualpaySettings>(storeScope);

            //save settings
            settings.MerchantId = model.MerchantId;
            settings.SecurityKey = model.SecurityKey;
            settings.UseSandbox = model.UseSandbox;
            settings.PaymentTransactionType = (TransactionType)model.PaymentTransactionTypeId;
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

        #endregion
    }
}