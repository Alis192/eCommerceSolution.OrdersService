﻿using AutoMapper;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using eCommerce.OrdersMicroservice.DataAccessLayer.RepositoryContracts;
using FluentValidation;
using FluentValidation.Results;
using MongoDB.Driver;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Services
{
    public class OrdersService : IOrdersService
    {
        private readonly IValidator<OrderAddRequest> _orderAddRequestValidator;
        private readonly IValidator<OrderItemAddRequest> _orderItemAddRequestValidator;
        private readonly IValidator<OrderUpdateRequest> _orderUpdateRequestValidator;
        private readonly IValidator<OrderItemUpdateRequest> _orderItemUpdateRequestValidator;
        private readonly IMapper _mapper;
        private readonly IOrdersRepository _ordersRepository;
        private readonly UsersMicroserviceClient _usersMicroserviceClient;
        private readonly ProductsMicroserviceClient _productsMicroserviceClient;

        public OrdersService(IOrdersRepository ordersRepository, IMapper mapper,
            IValidator<OrderAddRequest> orderAddRequestValidator,
            IValidator<OrderItemAddRequest> orderItemAddRequestValidator,
            IValidator<OrderUpdateRequest> orderUpdateRequestValidator,
            IValidator<OrderItemUpdateRequest> orderItemUpdateRequestValidator,
            UsersMicroserviceClient usersMicroserviceClient,
            ProductsMicroserviceClient productsMicroserviceClient)
        {
            _mapper = mapper;
            _ordersRepository = ordersRepository;
            _orderAddRequestValidator = orderAddRequestValidator;
            _orderItemAddRequestValidator = orderItemAddRequestValidator;
            _orderUpdateRequestValidator = orderUpdateRequestValidator;
            _orderItemUpdateRequestValidator = orderItemUpdateRequestValidator;
            _usersMicroserviceClient = usersMicroserviceClient;
            _productsMicroserviceClient = productsMicroserviceClient;
        }


        public async Task<OrderResponse?> AddOrder(OrderAddRequest orderAddRequest)
        {
            //Check for null parameter
            if (orderAddRequest == null)
            {
                throw new ArgumentNullException(nameof(orderAddRequest));
            }

            //Validate the request
            ValidationResult orderAddRequestValidationResult = await _orderAddRequestValidator.ValidateAsync(orderAddRequest);

            if (!orderAddRequestValidationResult.IsValid)
            {
                string errors = string.Join(", ", orderAddRequestValidationResult.Errors.Select(e => e.ErrorMessage));

                throw new ArgumentException(errors);
            }

            List<ProductDTO> products = new List<ProductDTO>();

            //Validate order items using Fluent Validation
            foreach (OrderItemAddRequest orderItemAddRequest in orderAddRequest.OrderItems)
            {
                ValidationResult orderItemValidationResult = await _orderItemAddRequestValidator.ValidateAsync(orderItemAddRequest);
                if (!orderItemValidationResult.IsValid)
                {
                    string errors = string.Join(", ", orderItemValidationResult.Errors.Select(e => e.ErrorMessage));
                    throw new ArgumentException(errors);
                }

                //TO DO: Add logic for checking Product ID from Product Microservice
                ProductDTO? product = await _productsMicroserviceClient.GetProductByProductID(orderItemAddRequest.ProductID);

                if (product == null)
                {
                    throw new ArgumentException("Invalid Product ID");
                }

                products.Add(product);
            }


            //TO DO: Communicate with user service to validate user
            UserDTO? user = await _usersMicroserviceClient.GetUserByUserID(orderAddRequest.UserID);

            if (user == null)
            {
                throw new ArgumentException("Invalid User ID");
            }

            //Covert OrderAddRequest to Order
            Order orderInput = _mapper.Map<Order>(orderAddRequest);

            //Generate values
            foreach (OrderItem orderItem in orderInput.OrderItems)
            {
                orderItem.TotalPrice = orderItem.Quantity * orderItem.UnitPrice;
            }

            orderInput.TotalBill = orderInput.OrderItems.Sum(temp => temp.TotalPrice);

            //Invoke repository
            Order? addedOrder = await _ordersRepository.AddOrder(orderInput);

            if (addedOrder == null)
            {
                return null;
            }

            OrderResponse addedOrderResponse = _mapper.Map<OrderResponse>(addedOrder);

            //TO DO: Load ProductName and Category in each OrderItem
            if (addedOrderResponse != null)
            {
                foreach (OrderItemResponse orderItemResponse in addedOrderResponse.OrderItems)
                {
                    ProductDTO? productDTO = products.Where(temp => temp.ProductID == orderItemResponse.ProductID).FirstOrDefault();

                    if (productDTO == null)
                        continue;

                    _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse);
                }
            }

            return addedOrderResponse;
        }

        public async Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest)
        {
            //Check for null parameter
            if (orderUpdateRequest == null)
            {
                throw new ArgumentNullException(nameof(orderUpdateRequest));
            }

            //Validate the request
            ValidationResult orderUpdateRequestValidationResult = await _orderUpdateRequestValidator.ValidateAsync(orderUpdateRequest);

            if (!orderUpdateRequestValidationResult.IsValid)
            {
                string errors = string.Join(", ", orderUpdateRequestValidationResult.Errors.Select(e => e.ErrorMessage));

                throw new ArgumentException(errors);
            }

            List<ProductDTO> products = new List<ProductDTO>();

            //Validate order items using Fluent Validation
            foreach (OrderItemUpdateRequest orderItemUpdateRequest in orderUpdateRequest.OrderItems)
            {
                ValidationResult orderItemUpdateRequestValidationResult = await _orderItemUpdateRequestValidator.ValidateAsync(orderItemUpdateRequest);
                if (!orderItemUpdateRequestValidationResult.IsValid)
                {
                    string errors = string.Join(", ", orderItemUpdateRequestValidationResult.Errors.Select(e => e.ErrorMessage));
                    throw new ArgumentException(errors);
                }

                //TO DO: Add logic for checking Product ID from Product Microservice
                ProductDTO? product = await _productsMicroserviceClient.GetProductByProductID(orderItemUpdateRequest.ProductID);

                if (product == null)
                {
                    throw new ArgumentException("Invalid Product ID");
                }

                products.Add(product);
            }


            //TO DO: Communicate with user service to validate user ID 
            UserDTO? user = await _usersMicroserviceClient.GetUserByUserID(orderUpdateRequest.UserID);

            if (user == null)
            {
                throw new ArgumentException("Invalid User ID");
            }

            //Covert OrderAddRequest to Order
            Order orderInput = _mapper.Map<Order>(orderUpdateRequest);

            //Generate values
            foreach (OrderItem orderItem in orderInput.OrderItems)
            {
                orderItem.TotalPrice = orderItem.Quantity * orderItem.UnitPrice;
            }

            orderInput.TotalBill = orderInput.OrderItems.Sum(temp => temp.TotalPrice);

            //Invoke repository
            Order? updatedOrder = await _ordersRepository.UpdateOrder(orderInput);

            if (updatedOrder == null)
            {
                return null;
            }

            OrderResponse updatedOrderResponse = _mapper.Map<OrderResponse>(updatedOrder);

            //TO DO: Load ProductName and Category in each OrderItem
            if (updatedOrderResponse != null)
            {
                foreach (OrderItemResponse orderItemResponse in updatedOrderResponse.OrderItems)
                {
                    ProductDTO? productDTO = products.Where(temp => temp.ProductID == orderItemResponse.ProductID).FirstOrDefault();

                    if (productDTO == null)
                        continue;

                    _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse);
                }
            }

            return updatedOrderResponse;
        }


        public async Task<bool> DeleteOrder(Guid orderID)
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, orderID);
            Order? existingOrder = await _ordersRepository.GetOrderByCondition(filter);

            if (existingOrder == null)
            {
                return false;
            }

            bool isDeleted = await _ordersRepository.DeleteOrder(orderID);

            return isDeleted;
        }

        public async Task<OrderResponse?> GetOrderByCondition(FilterDefinition<Order> filter)
        {
            Order? order = await _ordersRepository.GetOrderByCondition(filter);
            if (order == null)
            {
                return null;
            }
            OrderResponse orderResponse = _mapper.Map<OrderResponse>(order);

            //TO DO: Load ProductName and Category in each OrderItem
            if (orderResponse != null)
            {
                foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
                {
                    ProductDTO? productDTO = await _productsMicroserviceClient.GetProductByProductID(orderItemResponse.ProductID);

                    if (productDTO == null)
                        continue;

                    _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse);
                }
            }

            return orderResponse;
        }

        public async Task<List<OrderResponse?>> GetOrdersByCondition(FilterDefinition<Order> filter)
        {
            IEnumerable<Order?> orders = await _ordersRepository.GetOrdersByCondition(filter);

            IEnumerable<OrderResponse?> orderResponses = _mapper.Map<IEnumerable<OrderResponse>>(orders);

            foreach (OrderResponse? orderResponse in orderResponses)
            {
                if (orderResponse == null)
                {
                    continue;
                }

                foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
                {
                    ProductDTO? productDTO = await _productsMicroserviceClient.GetProductByProductID(orderItemResponse.ProductID);

                    if (productDTO == null)
                        continue;

                    _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse);
                }
            }

            return orderResponses.ToList();
        }

        public async Task<List<OrderResponse?>> GetOrders()
        {
            IEnumerable<Order> orders = await _ordersRepository.GetOrders();
            IEnumerable<OrderResponse?> orderResponses = _mapper.Map<IEnumerable<OrderResponse>>(orders);

            //TO DO: Load ProductName and Category in each OrderItem

            foreach (OrderResponse? orderResponse in orderResponses)
            {
                if (orderResponse == null)
                {
                    continue;
                }

                foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
                {
                    ProductDTO? productDTO = await _productsMicroserviceClient.GetProductByProductID(orderItemResponse.ProductID);

                    if (productDTO == null)
                        continue;

                    _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse);
                }
            }

            return orderResponses.ToList();
        }
    }

}
