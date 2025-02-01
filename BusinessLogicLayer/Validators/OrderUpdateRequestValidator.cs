using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using FluentValidation;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Validators
{
    public class OrderUpdateRequestValidator : AbstractValidator<OrderUpdateRequest>
    {
        public OrderUpdateRequestValidator()
        {
            RuleFor(order => order.OrderID).NotEmpty().WithErrorCode("Order ID can't be blank");

            RuleFor(order => order.UserID).NotEmpty().WithErrorCode("User ID can't be blank");

            RuleFor(order => order.OrderDate).NotEmpty().WithErrorCode("Order Date can't be blank");

            RuleFor(order => order.OrderItems).NotEmpty().WithErrorCode("Order Items can't be blank");
        }
    }
}
