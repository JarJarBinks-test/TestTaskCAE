using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestTaskCAE.BaseClasses
{
    public interface IOrderDataService
    {
        public Task<List<Order>> Get(Int32? orderId, DateTime? start, DateTime? end);
        public Task Add(Order order);
        public void Remove(Int32 orderId);
        public void Update(Order order);
    }
}
