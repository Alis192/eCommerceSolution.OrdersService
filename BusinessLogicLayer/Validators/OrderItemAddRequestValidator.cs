using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using FluentValidation;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Validators
{
    public class OrderItemAddRequestValidator : AbstractValidator<OrderItemAddRequest>
    {
        public OrderItemAddRequestValidator()
        {
            RuleFor(temp => temp.ProductID).NotEmpty().WithErrorCode("Product ID can't be blank");

            RuleFor(temp => temp.UnitPrice)
                .NotEmpty().WithErrorCode("Unit price can't be blank")
                .GreaterThan(0).WithErrorCode("Unit Price should be greater than 0");

            RuleFor(temp => temp.Quantity)
                .NotEmpty().WithErrorCode("Quantity can't be blank")
                .GreaterThan(0).WithErrorCode("Quantity should be greater than 0");
        }
    }
}
