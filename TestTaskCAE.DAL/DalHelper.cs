using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using TestTaskCAE.BaseClasses;

namespace TestTaskCAE.DAL
{
    public static class DalHelper
    {
        public static async Task<R> DoWithResult<T, R>(this ITestTaskDbContext context, Func<DbSet<T>, Task<R>> act, Boolean shouldSave = false)
            where T : class
        {
            var res = await act(context.Set<T>());
            if (!shouldSave)
                return res;
            
            await context.SaveChangesAsync();
            return res;
        }

        public static async Task Do<T>(this ITestTaskDbContext context, Func<DbSet<T>, Task> act, Boolean shouldSave = false)
            where T : class
        {
            await act(context.Set<T>());
            if (!shouldSave)
                return;

            await context.SaveChangesAsync();
            return;
        }

        public static void Do<T>(this ITestTaskDbContext context, Action<DbSet<T>> act, Boolean shouldSave = false)
            where T : class
        {
            act(context.Set<T>());
            if (!shouldSave)
                return;

            context.SaveChanges();
            return;
        }

        // TODO: we can use AutoMapper.
        public static BaseClasses.Order DbOrderToOrder(this Objects.DbOrder dataOrder) =>
            new BaseClasses.Order() { 
                Id = dataOrder.Id, 
                DateCreated = dataOrder.DateCreated, 
                Amount = dataOrder.Amount 
            };

        public static Objects.DbOrder OrderToDbOrder(this BaseClasses.Order order) =>
            new Objects.DbOrder() { 
                Id = order.Id, 
                DateCreated = order.DateCreated, 
                Amount = order.Amount 
            };

        public static void AddDalServices(this IServiceCollection services)
        {
            services.AddTransient<ITestTaskDbContext, TestTaskDbContext>(x => {
                var ob = new DbContextOptionsBuilder<TestTaskDbContext>();
                ob.UseSqlServer(x.GetService<IConfiguration>().GetConnectionString("TestTask"));
                return new TestTaskDbContext(ob.Options);
            });
        }

        public static IQueryable<T> ApplyOrderFilter<T>(this IQueryable<T> filter, Int32? orderId, DateTime? start, DateTime? end) where T : Order
        {
            if (orderId.HasValue)
                filter = filter.Where(c => c.Id == orderId);

            if (start.HasValue)
                filter = filter.Where(x => x.DateCreated >= start);

            if (end.HasValue)
                filter = filter.Where(x => x.DateCreated < end);

            return filter;
        }
    }
}
