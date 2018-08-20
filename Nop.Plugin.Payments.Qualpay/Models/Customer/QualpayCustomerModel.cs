using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.Qualpay.Models.Customer
{
    /// <summary>
    /// Represents the Qualpay customer model
    /// </summary>
    public class QualpayCustomerModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Plugins.Payments.Qualpay.Customer")]
        public string QualpayCustomerId { get; set; }

        public bool CustomerExists { get; set; }
    }
}