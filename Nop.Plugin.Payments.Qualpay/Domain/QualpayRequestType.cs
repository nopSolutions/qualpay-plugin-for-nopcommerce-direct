
namespace Nop.Plugin.Payments.Qualpay.Domain
{
    /// <summary>
    /// Represents enumeration of request types
    /// </summary>
    public enum QualpayRequestType
    {
        /// <summary>
        /// An authorization request is used to send cardholder data to the issuing bank for approval.
        /// An approved transaction will continue to be open until it expires or a capture message is received.
        /// </summary>
        Authorization,

        /// <summary>
        /// A verify request is used to send cardholder data to the issuing bank for validation. A verify message
        /// will return success if the cardholder information was verified by the issuer. 
        /// </summary>
        Verify,

        /// <summary>
        /// A capture request is used to capture a previously authorized transaction using the payment gateway
        /// identifier returned by the authorization message. A capture may be completed for any amount up to the authorized amount.
        /// </summary>
        Capture,

        /// <summary>
        /// A sale request is used to perform the function of an authorization and a capture in a single message.
        /// This message is used in retail and card not present environments where no physical goods are being shipped.
        /// </summary>
        Sale,

        /// <summary>
        /// A void request is used to void a previously authorized transaction. Authorizations can be voided at any time. Captured transactions 
        /// can be voided until the batch is closed. The batch close time is configurable and by default is 11 PM Eastern Time.
        /// </summary>
        Void,

        /// <summary>
        /// A refund request is used to issue a partial or full refund of a previously captured transaction using the payment gateway identifier. 
        /// Multiple refunds are allowed per captured transaction provided that the sum of all refunds does not exceed the original captured transaction amount.
        /// </summary>
        Refund,

        /// <summary>
        /// A credit request is used to issue a non-referenced credit to a cardholder. The credit message is enabled during the first 30 days of production activity
        /// </summary>
        Credit,

        /// <summary>
        /// A force request is used to force a declined transaction into the system. This would occur when the
        /// online authorization was declined and the merchant received an authorization from a voice or automated response (ARU) system. 
        /// </summary>
        Force,

        /// <summary>
        /// A tokenization request is used to securely store cardholder data on the Qualpay system. 
        /// Once stored, a unique card identifier is returned for use in future transactions.
        /// </summary>
        Tokenization,

        /// <summary>
        /// A batch close request will cause the open batch of transactions to be immediately closed. This message is normally
        /// used by POS devices that wish to control the timing of the batch close rather than relying on the daily automatic batch close.
        /// </summary>
        BatchClose
    }
}
