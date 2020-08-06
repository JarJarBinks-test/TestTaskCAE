using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestTaskCAE.BaseClasses;
using TestTaskCAE.DAL.Objects;

namespace TestTaskCAE.DAL
{
    public class OrderDataService : IOrderDataService, IStructureDataService
    {
        ITestTaskDbContext context;
        ILogger<IOrderDataService> logger;

        public OrderDataService(ITestTaskDbContext dbContext, ILogger<IOrderDataService> logger)
        {
            context = (ITestTaskDbContext)dbContext??throw new ArgumentNullException(nameof(dbContext));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<Order>> Get(Int32? orderId, DateTime? start, DateTime? end)
        {
            var result = await context.DoWithResult<DbOrder, List<DbOrder>>(os =>
                os.AsQueryable().ApplyOrderFilter(orderId, start, end).ToListAsync());

            return result.Select(x => x.DbOrderToOrder()).ToList();
        }

        public async Task Add(Order order) => await context.Do<DbOrder>(os => os.AddRangeAsync(order.OrderToDbOrder()), true);

        public void Remove(Int32 orderId) => context.Do<DbOrder>(os => os.RemoveRange(new DbOrder() { Id = orderId }), true);

        public void Update(Order order) => context.Do<DbOrder>(os => os.UpdateRange(order.OrderToDbOrder()), true);

        public void EnsureStructureCreated()
        {
            try
            {
                context.Database.Migrate();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred creating the DB.");
                throw;
            }
        }
    }    
}
