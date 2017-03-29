using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Nop.Plugin.Payments.Qualpay.Domain;
using Nop.Services.Logging;

namespace Nop.Plugin.Payments.Qualpay.Helpers
{
    /// <summary>
    /// Represents Qualpay helper
    /// </summary>
    public static class QualpayHelper
    {
        #region Utilities

        /// <summary>
        /// Gets Qualpay payment gateway URL
        /// <param name="qualpaySettings">Qualpay settings</param>
        /// </summary>
        /// <returns>URL</returns>
        private static string GetQualpayPaymentGatewayUrl(QualpaySettings qualpaySettings)
        {
            return qualpaySettings.UseSandbox
                ? "https://api-test.qualpay.com/pg/"
                : "https://api.qualpay.com/pg/";
        }

        /// <summary>
        /// Gets path to the appropriate Qualpay service
        /// <param name="requestType">Request type</param>
        /// </summary>
        /// <returns>Path</returns>
        private static string GetServicePath(QualpayRequestType requestType)
        {
            switch (requestType)
            {
                case QualpayRequestType.Authorization:
                    return "auth";
                case QualpayRequestType.Verify:
                    return "verify";
                case QualpayRequestType.Capture:
                    return "capture";
                case QualpayRequestType.Sale:
                    return "sale";
                case QualpayRequestType.Void:
                    return "void";
                case QualpayRequestType.Refund:
                    return "refund";
                case QualpayRequestType.Credit:
                    return "credit";
                case QualpayRequestType.Force:
                    return "force";
                case QualpayRequestType.Tokenization:
                    return "tokenize";
                case QualpayRequestType.BatchClose:
                    return "batchClose";
                default:
                    throw new NotSupportedException(requestType.ToString());
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Post request and get response to Qualpay payment gateway
        /// </summary>
        /// <param name="qualpayRequest">Request details</param>
        /// <param name="requestType">Request type</param>
        /// <param name="transactionId">transaction identifier</param>
        /// <param name="qualpaySettings">Qualpay settings</param>
        /// <param name="logger">Logger</param>
        /// <returns>Response from Qualpay payment gateway</returns>
        public static QualpayResponse PostRequest(QualpayRequest qualpayRequest, QualpayRequestType requestType, 
            string transactionId, QualpaySettings qualpaySettings, ILogger logger)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            //add credentials to request
            qualpayRequest.MerchantId = qualpaySettings.MerchantId;
            qualpayRequest.SecurityKey = qualpaySettings.SecurityKey;
            qualpayRequest.DeveloperId = "nopCommerce";

            //create service url
            var serviceUrl = string.Format("{0}{1}/{2}", GetQualpayPaymentGatewayUrl(qualpaySettings),
                GetServicePath(requestType), transactionId);

            //create request
            var request = (HttpWebRequest)WebRequest.Create(serviceUrl);
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";
            request.Accept = "application/json";

            //set post data
            var postData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(qualpayRequest));
            request.ContentLength = postData.Length;

            //post request
            try
            {
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(postData, 0, postData.Length);
                }

                //get response
                var httpResponse = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    return JsonConvert.DeserializeObject<QualpayResponse>(streamReader.ReadToEnd());
                }
            }
            catch (WebException ex)
            {
                try
                {
                    var httpResponse = (HttpWebResponse)ex.Response;
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        //log errors
                        var response = streamReader.ReadToEnd();
                        logger.Error(string.Format("Qualpay error: {0}", response), ex);

                        return JsonConvert.DeserializeObject<QualpayResponse>(response);
                    }
                }
                catch (Exception exc)
                {
                    logger.Error("Qualpay error", exc);
                    return null;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Qualpay error", ex);
                return null;
            }
        }

        #endregion
    }
}
