using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestTaskCAE.BaseClasses;

namespace TestTaskCAE.Services
{
    public class SalesService : ISalesService, IStructureDataService
    {
        readonly ILogger<SalesService> logger;
        readonly IOrderDataService orderDataService;
        public SalesService(IOrderDataService orderDataService, ILogger<SalesService> logger)
        {
            this.orderDataService = orderDataService ?? throw new ArgumentNullException(nameof(orderDataService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Add(Order order)
        {
            logger.LogInformation($"{nameof(Add)} {order}.");
            await orderDataService.Add(order);
        }

        public void Remove(Int32 orderId)
        {
            logger.LogInformation($"{nameof(Remove)} {orderId}.");
            orderDataService.Remove(orderId);
        }

        public void Update(Order order)
        {
            logger.LogInformation($"{nameof(Update)} {order}.");
            orderDataService.Update(order);
        }

        public async Task<List<Order>> Get(Int32? orderId = null, DateTime? start = null, DateTime? end = null)
        {
            logger.LogInformation($"{nameof(Get)} {orderId}.");
            return await orderDataService.Get(orderId, start, end);
        }

        public void EnsureStructureCreated()
        {
            ((IStructureDataService)orderDataService).EnsureStructureCreated();
        }        
    }
}
