using FluentValidation;
using Nop.Plugin.Payments.Qualpay.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Payments.Qualpay.Validators
{
    /// <summary>
    /// Represents custom payment info validator
    /// </summary>
    public partial class PaymentInfoValidator : BaseNopValidator<PaymentInfoModel>
    {
        public PaymentInfoValidator(ILocalizationService localizationService)
        {
            //set validation rules
            RuleFor(model => model.CardholderName)
                .NotEmpty().WithMessage(localizationService.GetResource("Payment.CardholderName.Required"))
                .When(model => !model.UseStoredCard);
            RuleFor(model => model.CardNumber)
                .IsCreditCard().WithMessage(localizationService.GetResource("Payment.CardNumber.Wrong"))
                .When(model => !model.UseStoredCard);
            RuleFor(model => model.CardCode)
                .Matches(@"^[0-9]{3,4}$").WithMessage(localizationService.GetResource("Payment.CardCode.Wrong"))
                .When(model => !model.UseStoredCard);
            RuleFor(model => model.ExpireMonth)
                .NotEmpty().WithMessage(localizationService.GetResource("Payment.ExpireMonth.Required"))
                .When(model => !model.UseStoredCard);
            RuleFor(model => model.ExpireYear)
                .NotEmpty().WithMessage(localizationService.GetResource("Payment.ExpireYear.Required"))
                .When(model => !model.UseStoredCard);
        }
    }
}