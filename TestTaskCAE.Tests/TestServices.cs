using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestTaskCAE.BaseClasses;
using TestTaskCAE.Services;

namespace TestTaskCAE.Tests
{
    [TestClass]
    public class TestServices
    {
        Mock<IServiceProvider> serviceProvider;
        Mock<IOrderDataService> sourceService;
        Mock<ILoggerFactory> iLoggerFactory;

        [TestInitialize]
        public void Setup()
        {
            iLoggerFactory = new Mock<ILoggerFactory>();
            iLoggerFactory
                .Setup(x => x.CreateLogger(It.IsAny<String>()))
                .Returns(new Mock<ILogger>().Object);

            sourceService = new Mock<IOrderDataService>();

            serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(ILoggerFactory)))
                .Returns(iLoggerFactory.Object);

            serviceProvider
                .Setup(x => x.GetService(typeof(IOrderDataService)))
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

        [TestMethod]
        public void Add_Order_No_Error()
        {
            // Arrange
            var salesService = new SalesService(sourceService.Object, iLoggerFactory.Object.CreateLogger<SalesService>());
            var ord = new Order()
            {
                Id = -1,
                Amount = 10,
                DateCreated = DateTime.Now
            };
            sourceService.Setup(nt => nt.Add(ord)).Returns(Task.Run(() => { }));

            // Action
            salesService.Add(ord).Wait();

            // Assert
            sourceService.Verify(x => x.Add(ord), Times.Once);
        }

        [TestMethod]
        public void Remove_Order_No_Error()
        {
            // Arrange
            var salesService = new SalesService(sourceService.Object, iLoggerFactory.Object.CreateLogger<SalesService>());
            var id = 100;
            sourceService.Setup(nt => nt.Remove(id));

            // Action
            salesService.Remove(id);

            // Assert
            sourceService.Verify(x => x.Remove(id), Times.Once);
        }

        [TestMethod]
        public void Update_Order_No_Error()
        {
            // Arrange
            var salesService = new SalesService(sourceService.Object, iLoggerFactory.Object.CreateLogger<SalesService>());
            var ord = new Order()
            {
                Id = -1,
                Amount = 10,
                DateCreated = DateTime.Now
            };
            sourceService.Setup(nt => nt.Update(ord));

            // Action
            salesService.Update(ord);

            // Assert
            sourceService.Verify(x => x.Update(ord), Times.Once);
        }

        [TestMethod]
        public void Get_Orders_Return_Orders()
        {
            // Arrange
            var salesService = new SalesService(sourceService.Object, iLoggerFactory.Object.CreateLogger<SalesService>());
            var idForCheck = 2;
            var orders = new List<Order>()
            {
                new Order()
                {
                    Id = 1,
                    Amount = 1.11M,
                    DateCreated = new DateTime()
                },
                new Order()
                {
                    Id = idForCheck,
                    Amount = 2.22M,
                    DateCreated = new DateTime()
                },
                new Order()
                {
                   Id = 3,
                    Amount = 3.33M,
                    DateCreated = new DateTime()
                },
                new Order()
                {
                    Id = 4,
                    Amount = 4.44M,
                    DateCreated = new DateTime()
                }
            };

            // Also check by others parameters should added.
            Int32? idFromRequest = null;
            sourceService.Setup(nt => nt.Get(It.IsAny<Int32?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>())).Callback<Int32?, DateTime?, DateTime?>((x,y,z)=> {
                idFromRequest = x;
            }).ReturnsAsync(()=> {
                var res = orders.Where(x => !idFromRequest.HasValue || x.Id == idFromRequest).ToList();
                return res;
            });

            // Action
            var result = salesService.Get(null, null, null).Result;
            var result2 = salesService.Get(idForCheck, null, null).Result;

            // Assert
            sourceService.Verify(x => x.Get(null, null, null), Times.Exactly(1));
            sourceService.Verify(x => x.Get(idForCheck, null, null), Times.Once);
            Assert.AreEqual(orders.Count, result.Count);
            Assert.AreEqual(result2.Count, 1);
            Assert.AreEqual(result2.First().Id, idForCheck);
        }
    }
}
