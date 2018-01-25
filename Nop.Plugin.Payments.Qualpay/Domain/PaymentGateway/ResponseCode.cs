using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Qualpay.Domain.PaymentGateway
{
    /// <summary>
    /// Represents enumeration of Qualpay payment gateway response codes
    /// </summary>
    public enum ResponseCode
    {
        /// <summary>
        /// The request was successful
        /// </summary>
        [JsonProperty(PropertyName = "000")]
        Success,

        /// <summary>
        /// The request was invalid
        /// </summary>
        [JsonProperty(PropertyName = "100")]
        BadRequest,

        /// <summary>
        /// The credentials provided do not match the on-file values for the merchant
        /// </summary>
        [JsonProperty(PropertyName = "101")]
        InvalidCredentials,

        /// <summary>
        /// The transaction ID value could not be linked to a valid transaction
        /// </summary>
        [JsonProperty(PropertyName = "102")]
        InvalidtransactionId,

        /// <summary>
        /// The request was missing valid cardholder data
        /// </summary>
        [JsonProperty(PropertyName = "103")]
        MissingCardholderData,

        /// <summary>
        /// The request was either missing the amount or the value provided was invalid
        /// </summary>
        [JsonProperty(PropertyName = "104")]
        InvalidTransactionAmount,

        /// <summary>
        /// The request was missing authorization code.
        /// </summary>
        [JsonProperty(PropertyName = "105")]
        MissingAuthorizationCode,

        /// <summary>
        /// Invalid AVS data
        /// </summary>
        [JsonProperty(PropertyName = "106")]
        InvalidAvsData,

        /// <summary>
        /// The expiration date provided in the request was not properly formatted
        /// </summary>
        [JsonProperty(PropertyName = "107")]
        InvalidExpirationDate,

        /// <summary>
        /// The card number in the request message was non-numeric or contained either too few or too many digits
        /// </summary>
        [JsonProperty(PropertyName = "108")]
        InvalidCardNumber,

        /// <summary>
        /// There are any field exceeds the maximum allowed length
        /// </summary>
        [JsonProperty(PropertyName = "109")]
        FieldLengthValidationFailed,

        /// <summary>
        /// There are any of the dynamic DBA fields and the merchant has not been approved for dynamic DBA
        /// </summary>
        [JsonProperty(PropertyName = "110")]
        DynamicDbaNotNllowed,

        /// <summary>
        /// Unreferenced credit is submitted and the merchant is not authorized to process credits
        /// </summary>
        [JsonProperty(PropertyName = "111")]
        CreditsNotAllowed,

        /// <summary>
        /// Customer already exists or required customer fields are not included
        /// </summary>
        [JsonProperty(PropertyName = "112")]
        InvalidCustomerData,

        /// <summary>
        /// The transaction has already been captured or voided
        /// </summary>
        [JsonProperty(PropertyName = "401")]
        Voidfailed,

        /// <summary>
        /// The transaction has already been refunded, the original transaction has not been captured, 
        /// the total amount of all refunds exceeds the original transaction amount or the original transaction was not a sale
        /// </summary>
        [JsonProperty(PropertyName = "402")]
        RefundFailed,

        /// <summary>
        /// The amount exceeds the authorized amount (except when the merchant category code allows tips), 
        /// the transaction has already been captured or the authorization has been voided
        /// </summary>
        [JsonProperty(PropertyName = "403")]
        CaptureFailed,

        /// <summary>
        /// Batch close failed
        /// </summary>
        [JsonProperty(PropertyName = "404")]
        BatchCloseFailed,

        /// <summary>
        /// Tokenization failed
        /// </summary>
        [JsonProperty(PropertyName = "405")]
        TokenizationFailed,

        /// <summary>
        /// The authorization request timed out without returning a response.
        /// </summary>
        [JsonProperty(PropertyName = "998")]
        Timeout,

        /// <summary>
        /// The payment gateway application encountered an unexpected error while processing the request
        /// </summary>
        [JsonProperty(PropertyName = "999")]
        InternalError
    }
}