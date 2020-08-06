using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestTaskCAE.BaseClasses;

namespace TestTaskCAE.Services
{
    public interface ISalesService
    {
        public Task Add(Order order);

        public void Remove(Int32 orderId);

        public void Update(Order order);

        public Task<List<Order>> Get(Int32? orderId = null, DateTime? start = null, DateTime? end = null);
    }
}
