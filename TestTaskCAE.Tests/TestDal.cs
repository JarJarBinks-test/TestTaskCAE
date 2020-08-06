using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TestTaskCAE.BaseClasses;
using TestTaskCAE.DAL;
using TestTaskCAE.DAL.Objects;

namespace TestTaskCAE.Tests
{
    [TestClass]
    public class TestDal
    {
        Mock<IServiceProvider> serviceProvider;
        Mock<ITestTaskDbContext> sourceService;
        Mock<ILoggerFactory> iLoggerFactory;

        [TestInitialize]
        public void Setup()
        {
            iLoggerFactory = new Mock<ILoggerFactory>();
            iLoggerFactory
                .Setup(x => x.CreateLogger(It.IsAny<String>()))
                .Returns(new Mock<ILogger>().Object);

            serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(ILoggerFactory)))
                .Returns(iLoggerFactory.Object);

            sourceService = new Mock<ITestTaskDbContext>();
            serviceProvider
                .Setup(x => x.GetService(typeof(ITestTaskDbContext)))
                .Returns(sourceService.Object);

            var serviceScope = new Mock<IServiceScope>();
            serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);

            var serviceScopeFactory = new Mock<IServiceScopeFactory>();
            serviceScopeFactory
                .Setup(x => x.CreateScope())
                .Returns(serviceScope.Object);

            serviceProvider
                .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(serviceScopeFactory.Object);
        }

        [TestCleanup]
        public void CleanUp()
        {
        }

        // FIXME: Disable for now. AsAsyncEnumerable issue.
        //[TestMethod]
        public void Get_Orders()
        {
            // Arrange
            var orderDataService = new OrderDataService(sourceService.Object, iLoggerFactory.Object.CreateLogger<OrderDataService>());
            var rnd = new Random();
            var ordersQty = 40;
            var currDate = DateTime.UtcNow;

            var itms = Enumerable.Range(1, ordersQty).Select(x => new Order()
            {
                Amount = rnd.Next(x * 1000) + new Random().Next(x) / 100,
                DateCreated = currDate.AddDays(x)
            }.OrderToDbOrder()).ToList();
            var idForCheck = itms[new Random().Next(ordersQty)].Id;
            var badIdForCheck = -10000;

            var firstNum = rnd.Next(ordersQty - 1);
            var firstDate = itms[firstNum].DateCreated;
            var secondNum = rnd.Next(firstNum, ordersQty);
            var secondDate = itms[secondNum].DateCreated;

            var ordersMock = TestHelper.ToDbSetMock(itms);
            sourceService.Setup(x => x.Orders).Returns(ordersMock.Object);
            sourceService.Setup(x => x.Set<DbOrder>()).Returns(ordersMock.Object);

            // Action
            var result = orderDataService.Get(null, null, null).Result;
            var resultId = orderDataService.Get(idForCheck, null, null).Result;
            var resultbadId = orderDataService.Get(badIdForCheck, null, null).Result;
            var resultDates = orderDataService.Get(null, firstDate, secondDate).Result;

            // Assert
            ordersMock.Verify(x => x.Where(It.IsAny<Expression<Func<Order, bool>>>()), Times.Exactly(4));
            sourceService.Verify(x => x.Set<DbOrder>(), Times.Exactly(4));

            Assert.AreEqual(ordersQty, result.Count);
            Assert.AreEqual(1, resultId.Count);
            Assert.AreEqual(0, resultbadId.Count);
            resultDates.ForEach(os =>
            {
                Assert.IsTrue(os.DateCreated >= firstDate && os.DateCreated < secondDate);
            });
        }

        [TestMethod]
        public void Add_Order()
        {
            // Arrange
            var orderDataService = new OrderDataService(sourceService.Object, iLoggerFactory.Object.CreateLogger<OrderDataService>());
            var rnd = new Random();
            var ordersQty = 20;
            var currDate = DateTime.UtcNow;
            var orders = Enumerable.Range(1, ordersQty).Select(x => new Order()
            {
                Amount = rnd.Next(x * 1000) + new Random().Next(x) / 100,
                DateCreated = currDate.AddDays(x)
            }).ToList();

            var itms = new List<DbOrder>();

            var ordersMock = TestHelper.ToDbSetMock(itms);
            ordersMock.Setup(x => x.AddRangeAsync(It.IsAny<DbOrder[]>())).Callback<DbOrder[]>((s) =>
            {
                s.ToList().ForEach(fe =>
                {
                    itms.Add(fe);
                });
            }).Returns(Task.Run(() => { }));
            sourceService.Setup(x => x.Orders).Returns(ordersMock.Object);
            sourceService.Setup(x => x.Set<DbOrder>()).Returns(ordersMock.Object);

            var ord = new Order()
            {
                Id = 1,
                Amount = 77.77M,
                DateCreated = DateTime.UtcNow.AddDays(15)
            }.OrderToDbOrder();

            var badOrder = new Order()
            {
                Id = Int32.MaxValue,
                Amount = 1.1M,
                DateCreated = DateTime.UtcNow.AddDays(30)
            }.OrderToDbOrder();

            // Action
            orders.ForEach(o => { orderDataService.Add(o.OrderToDbOrder()).Wait(); });

            // Assert
            ordersMock.Verify(x => x.AddRangeAsync(It.IsAny<DbOrder[]>()), Times.Exactly(orders.Count));
            Assert.AreEqual(orders.Count, itms.Count);
            orders.ForEach(os =>
            {
                Assert.IsTrue(itms.Any(x => x.Amount == os.Amount && x.DateCreated == os.DateCreated));
            });
        }

        [TestMethod]
        public void Remove_Order()
        {
            // Arrange
            var orderDataService = new OrderDataService(sourceService.Object, iLoggerFactory.Object.CreateLogger<OrderDataService>());
            var itms = new List<DbOrder>()
            {
                new Order() {
                    Id = 753,
                    Amount = 555M,
                    DateCreated = DateTime.UtcNow
                }.OrderToDbOrder(),
                new Order() {
                    Id = new Random().Next(),
                    Amount = 666M,
                    DateCreated = DateTime.UtcNow
                }.OrderToDbOrder(),
                new Order() {
                    Id = 1,
                    Amount = 555M,
                    DateCreated = DateTime.UtcNow
                }.OrderToDbOrder(),
            };
            

            var ordersMock = TestHelper.ToDbSetMock(itms);
            ordersMock.Setup(x => x.RemoveRange(It.IsAny<DbOrder[]>())).Callback<DbOrder[]>((s) =>
            {
                s.ToList().ForEach(fe =>
                {
                    itms.RemoveAll(x => x.Id == fe.Id);
                });
            });
            sourceService.Setup(x => x.Orders).Returns(ordersMock.Object);
            sourceService.Setup(x => x.Set<DbOrder>()).Returns(ordersMock.Object);

            var ordersCount = itms.Count;
            for (var i = itms.Count-1; i >= 0; i--)
            {
                var restCount = itms.Count;
                var orderForDelete = itms[i];
                // Action
                orderDataService.Remove(orderForDelete.Id);

                // Assert
                ordersMock.Verify(x => x.RemoveRange(It.IsAny<DbOrder[]>()), Times.Exactly(ordersCount - i));
                Assert.AreEqual(restCount - 1, itms.Count);
                Assert.IsTrue(itms.TrueForAll(x => x.Id != orderForDelete.Id));
            }            
        }        

        [TestMethod]
        public void Update_Order() {
            // Arrange
            var orderDataService = new OrderDataService(sourceService.Object, iLoggerFactory.Object.CreateLogger<OrderDataService>());
            var requestOrder = new Order()
            {
                Id = 1,
                Amount = 29.99M,
                DateCreated = DateTime.UtcNow
            };
            var itms = new List<DbOrder>() {
                requestOrder.OrderToDbOrder()
            };
            
            var ordersMock = TestHelper.ToDbSetMock(itms);
            ordersMock.Setup(d => d.UpdateRange(It.IsAny<DbOrder[]>())).Callback<DbOrder[]>((s) => {
                s.ToList().ForEach(fe => {
                    var fod = itms.FirstOrDefault(v => v.Id == fe.Id);
                    if (fod != null)
                    {
                        fod.DateCreated = fe.DateCreated;
                        fod.Amount = fe.Amount;
                    }
                });
            });
            sourceService.Setup(x => x.Orders).Returns(ordersMock.Object);
            sourceService.Setup(x => x.Set<DbOrder>()).Returns(ordersMock.Object);

            var ord = new Order()
            {
                Id = 1,
                Amount = 77.77M,
                DateCreated = DateTime.UtcNow.AddDays(15)
            }.OrderToDbOrder();

            var badOrder = new Order()
            {
                Id = Int32.MaxValue,
                Amount = 1.1M,
                DateCreated = DateTime.UtcNow.AddDays(30)
            }.OrderToDbOrder();

            // Action
            orderDataService.Update(ord);
            orderDataService.Update(badOrder);

            // Assert
            ordersMock.Verify(x => x.UpdateRange(It.IsAny<DbOrder[]>()), Times.Exactly(2));
            Assert.AreEqual(1, itms.Count);
            var updatedItem = itms.First();
            Assert.AreEqual(ord.Amount, updatedItem.Amount);
            Assert.AreEqual(ord.DateCreated, updatedItem.DateCreated);
        }
    }    
}
