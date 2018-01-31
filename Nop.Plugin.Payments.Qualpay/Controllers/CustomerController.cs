using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Payments.Qualpay.Domain.Platform;
using Nop.Plugin.Payments.Qualpay.Models.Customer;
using Nop.Plugin.Payments.Qualpay.Services;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.Qualpay.Controllers
{
    public class CustomerController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IPermissionService _permissionService;
        private readonly QualpayManager _qualpayManager;

        #endregion

        #region Ctor

        public CustomerController(ICustomerService customerService,
            IPermissionService permissionService,
            QualpayManager qualpayManager)
        {
            this._customerService = customerService;
            this._permissionService = permissionService;
            this._qualpayManager = qualpayManager;
        }

        #endregion

        #region Utilities

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
                FirstName = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName),
                LastName = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName),
                Company = customer.GetAttribute<string>(SystemCustomerAttributeNames.Company),
                Phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone),
                ShippingAddresses = customer.ShippingAddress == null ? null : new List<ShippingAddress>
                {
                    new ShippingAddress
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

        #endregion

        #region Methods

        [HttpPost]
        public IActionResult CreateQualpayCustomer(int customerId)
        {
            //whether user has the authority
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //check whether customer exists
            var customer = _customerService.GetCustomerById(customerId)
                ?? throw new ArgumentException("No customer found with the specified id", nameof(customerId));

            //check whether customer is already exists in the Vault and try to create new one if does not exist
            var vaultCustomer = _qualpayManager.GetCustomerById(customer.Id.ToString())
                ?? _qualpayManager.CreateCustomer(CreateCustomerRequest(customer))
                ?? throw new NopException("Qualpay Customer Vault error: Failed to create customer. Error details in the log");

            //save selected tab
            SaveSelectedTabName();

            return Json(new { Result = true });
        }

        [HttpPost]
        public IActionResult QualpayCustomerCardList(int customerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

            //check whether customer exists
            var customer = _customerService.GetCustomerById(customerId)
                ?? throw new ArgumentException("No customer found with the specified id", nameof(customerId));

            //try to get customer billing cards
            var billingCards = _qualpayManager.GetCustomerCards(customer.Id.ToString())?.Where(card => card != null)?.ToList()
                ?? new List<BillingCard>();

            //prepare grid model
            var gridModel = new DataSourceResult
            {
                Data = billingCards.Select(card => new QualpayCustomerCardModel
                {
                    Id = card.CardId,
                    CardId = card.CardId,
                    CardType = card.CardType?.ToString(),
                    ExpirationDate = card.ExpirationDate,
                    MaskedNumber = card.CardNumber
                }),
                Total = billingCards.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult QualpayCustomerCardDelete(string id, int customerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //check whether customer exists
            var customer = _customerService.GetCustomerById(customerId)
                ?? throw new ArgumentException("No customer found with the specified id", nameof(customerId));

            //try to delete selected card
            if (!_qualpayManager.DeleteCustomerCard(customer.Id.ToString(), id))
                throw new NopException("Qualpay Customer Vault error: Failed to delete card. Error details in the log");

            return new NullJsonResult();
        }

        #endregion
    }
}