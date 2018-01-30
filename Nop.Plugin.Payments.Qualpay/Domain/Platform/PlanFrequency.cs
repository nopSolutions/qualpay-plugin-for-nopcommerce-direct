
namespace Nop.Plugin.Payments.Qualpay.Domain.Platform
{
    /// <summary>
    /// Represents recurring billing plan frequency enumeration
    /// </summary>
    public enum PlanFrequency
    {
        /// <summary>
        /// Weekly
        /// </summary>
        Weekly = 0,

        /// <summary>
        /// Bi-Weekly
        /// </summary>
        BiWeekly = 1,

        /// <summary>
        /// Monthly
        /// </summary>
        Monthly = 3,

        /// <summary>
        /// Quarterly
        /// </summary>
        Quarterly = 4,

        /// <summary>
        /// Bi-Annually
        /// </summary>
        BiAnnually = 5,

        /// <summary>
        /// Annually
        /// </summary>
        Annually = 6
    }
}