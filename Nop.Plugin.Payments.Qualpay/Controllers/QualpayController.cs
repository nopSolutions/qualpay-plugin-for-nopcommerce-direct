using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Plugin.Payments.Qualpay.Domain;
using Nop.Plugin.Payments.Qualpay.Domain.Platform;
using Nop.Plugin.Payments.Qualpay.Models;
using Nop.Plugin.Payments.Qualpay.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Controllers;

namespace Nop.Plugin.Payments.Qualpay.Controllers
{
    public class QualpayController : BaseAdminController
    {
        #region Fields

        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IEmailAccountService _emailAccountService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly QualpayManager _qualpayManager;

        #endregion

        #region Ctor

        public QualpayController(EmailAccountSettings emailAccountSettings,
            IEmailAccountService emailAccountService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreService storeService,
            IWorkContext workContext,
            QualpayManager qualpayManager)
        {
            this._emailAccountSettings = emailAccountSettings;
            this._emailAccountService = emailAccountService;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._settingService = settingService;
            this._storeService = storeService;
            this._workContext = workContext;
            this._qualpayManager = qualpayManager;
        }

        #endregion

        #region Methods

        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeId = GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<QualpaySettings>(storeId);

            //prepare model
            var model = new ConfigurationModel
            {
                MerchantId = settings.MerchantId,
                SecurityKey = settings.SecurityKey,
                UseSandbox = settings.UseSandbox,
                UseEmbeddedFields = settings.UseEmbeddedFields,
                UseCustomerVault = settings.UseCustomerVault,
                UseRecurringBilling = settings.UseRecurringBilling,
                PaymentTransactionTypeId = (int)settings.PaymentTransactionType,
                AdditionalFee = settings.AdditionalFee,
                AdditionalFeePercentage = settings.AdditionalFeePercentage,
                ActiveStoreScopeConfiguration = storeId,
                IsConfigured = !string.IsNullOrEmpty(settings.MerchantId) && long.TryParse(settings.MerchantId, out long merchantId)
            };

            if (storeId > 0)
            {
                model.SecurityKey_OverrideForStore = _settingService.SettingExists(settings, x => x.SecurityKey, storeId);
                model.UseSandbox_OverrideForStore = _settingService.SettingExists(settings, x => x.UseSandbox, storeId);
                model.UseEmbeddedFields_OverrideForStore = _settingService.SettingExists(settings, x => x.UseEmbeddedFields, storeId);
                model.UseCustomerVault_OverrideForStore = _settingService.SettingExists(settings, x => x.UseCustomerVault, storeId);
                model.UseRecurringBilling_OverrideForStore = _settingService.SettingExists(settings, x => x.UseRecurringBilling, storeId);
                model.PaymentTransactionTypeId_OverrideForStore = _settingService.SettingExists(settings, x => x.PaymentTransactionType, storeId);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(settings, x => x.AdditionalFee, storeId);
                model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(settings, x => x.AdditionalFeePercentage, storeId);
            }

            //prepare payment transaction types
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
            var storeId = GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<QualpaySettings>(storeId);

            //ensure that webhook is already exists and create the new one if does not (required for recurring billing)
            if (model.UseRecurringBilling &&
                (string.IsNullOrEmpty(settings.WebhookId) || _qualpayManager.GetWebhookById(settings.WebhookId)?.Status != WebhookStatus.Active))
            {
                var webhook = _qualpayManager.CreateWebhook(new CreateWebhookRequest
                {
                    EmailAddress = new[] { _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId)?.Email },
                    Events = new[]
                    {
                        QualpayDefaults.SubscriptionPaymentFailureWebhookEvent,
                        QualpayDefaults.SubscriptionPaymentSuccessWebhookEvent,
                        QualpayDefaults.ValidateUrlWebhookEvent
                    },
                    Label = QualpayDefaults.WebhookLabel,
                    NotificationUrl = Url.RouteUrl(QualpayDefaults.WebhookRouteName, null, Uri.UriSchemeHttps),
                    Status = WebhookStatus.Active
                });
                if (webhook?.WebhookId != null)
                {
                    settings.WebhookId = webhook.WebhookId.ToString();
                    settings.WebhookSecretKey = webhook.SecurityKey;
                }
                else
                    WarningNotification(_localizationService.GetResource("Plugins.Payments.Qualpay.Fields.Webhook.Warning"));
            }

            //save settings
            settings.MerchantId = model.MerchantId;
            settings.SecurityKey = model.SecurityKey;
            settings.UseSandbox = model.UseSandbox;
            settings.UseEmbeddedFields = model.UseEmbeddedFields;
            settings.UseCustomerVault = model.UseCustomerVault;
            settings.UseRecurringBilling = model.UseRecurringBilling;
            settings.PaymentTransactionType = (TransactionType)model.PaymentTransactionTypeId;
            settings.AdditionalFee = model.AdditionalFee;
            settings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSetting(settings, x => x.MerchantId, storeId, false);
            _settingService.SaveSettingOverridablePerStore(settings, x => x.SecurityKey, model.SecurityKey_OverrideForStore, storeId, false);
            _settingService.SaveSettingOverridablePerStore(settings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeId, false);
            _settingService.SaveSettingOverridablePerStore(settings, x => x.UseEmbeddedFields, model.UseEmbeddedFields_OverrideForStore, storeId, false);
            _settingService.SaveSettingOverridablePerStore(settings, x => x.UseCustomerVault, model.UseCustomerVault_OverrideForStore, storeId, false);
            _settingService.SaveSettingOverridablePerStore(settings, x => x.UseRecurringBilling, model.UseRecurringBilling_OverrideForStore, storeId, false);
            _settingService.SaveSettingOverridablePerStore(settings, x => x.PaymentTransactionType, model.PaymentTransactionTypeId_OverrideForStore, storeId, false);
            _settingService.SaveSettingOverridablePerStore(settings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeId, false);
            _settingService.SaveSettingOverridablePerStore(settings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeId, false);
            _settingService.SaveSetting(settings, x => x.WebhookId, storeId, false);
            _settingService.SaveSetting(settings, x => x.WebhookSecretKey, storeId, false);

            //now clear settings cache and display notification
            _settingService.ClearCache();
            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        #endregion
    }
}