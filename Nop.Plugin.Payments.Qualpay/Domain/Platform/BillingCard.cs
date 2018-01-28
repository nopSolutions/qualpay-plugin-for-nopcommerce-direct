using System;

namespace Nop.Plugin.Payments.Qualpay.Domain.Platform
{
    /// <summary>
    /// Represents billing details of a card 
    /// </summary>
    public class BillingCard : BillingCardRequest
    {
        #region Methods

        /// <summary>
        /// Get a request path
        /// </summary>
        /// <returns>Request path</returns>
        public override string GetRequestMethod() => throw new NotImplementedException();

        /// <summary>
        /// Get a request method
        /// </summary>
        /// <returns>Request method</returns>
        public override string GetRequestPath() => throw new NotImplementedException();

        #endregion
    }
}