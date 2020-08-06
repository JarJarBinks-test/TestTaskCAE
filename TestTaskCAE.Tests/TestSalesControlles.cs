using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestTaskCAE.BaseClasses;
using TestTaskCAE.Controllers;
using TestTaskCAE.DAL;
using TestTaskCAE.Services;

namespace TestTaskCAE.Tests
{
    [TestClass]
    public class TestSalesControlles
    {
        Mock<IServiceProvider> mockServiceProvider;
        Mock<IOrderDataService> mockSourceService;
        Mock<ISalesService> mockSalesService;
        Mock<ILoggerFactory> mockILoggerFactory;


        [TestInitialize]
        public void Setup()
        {
            mockILoggerFactory = new Mock<ILoggerFactory>();
            mockILoggerFactory
                .Setup(x => x.CreateLogger(It.IsAny<String>()))
                .Returns(new Mock<ILogger>().Object);

            mockSourceService = new Mock<IOrderDataService>();
            mockSalesService = new Mock<ISalesService>();

            mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(x => x.GetService(typeof(ILoggerFactory)))
                .Returns(mockILoggerFactory.Object);

            mockServiceProvider
                .Setup(x => x.GetService(typeof(IOrderDataService)))
                .Returns(mockSourceService.Object);

            mockServiceProvider
                .Setup(x => x.GetService(typeof(ISalesService)))
                .Returns(mockSalesService.Object);

            var serviceScope = new Mock<IServiceScope>();
            serviceScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);

            var serviceScopeFactory = new Mock<IServiceScopeFactory>();
            serviceScopeFactory
                .Setup(x => x.CreateScope())
                .Returns(serviceScope.Object);

            mockServiceProvider
                .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(serviceScopeFactory.Object);
        }

        [TestCleanup]
        public void CleanUp()
        {
        }

        [TestMethod]
        public void Controller_Add_Order_No_Error()
        {
            // Arrange
            var salesController = new SalesController(mockSalesService.Object);
            var ord = new Order()
            {
                Id = -1,
                Amount = 10,
                DateCreated = DateTime.Now
            };
            Order obj = null;
            mockSalesService.Setup(nt => nt.Add(ord)).Callback<Order>(x=> {
                obj = x;
            }).Returns(Task.Run(() => { }));

            // Action
            var ar = salesController.Post(ord).Result;

            // Assert
            mockSalesService.Verify(x => x.Add(ord), Times.Once);
            Assert.AreEqual(ord, obj);
            Assert.IsInstanceOfType(ar, typeof(OkResult));
        }

        [TestMethod]
        public void Controller_Remove_Order_No_Error()
        {
            // Arrange
            var salesController = new SalesController(mockSalesService.Object);
            var eid = 77;
            var rid = -1;
            mockSalesService.Setup(nt => nt.Remove(eid)).Callback<Int32>(x => {
                rid = x;
            });

            // Action
            var ar = salesController.Delete(eid);

            // Assert
            mockSalesService.Verify(x => x.Remove(eid), Times.Once);
            Assert.AreEqual(eid, rid);
            Assert.IsInstanceOfType(ar, typeof(OkResult));
        }

        [TestMethod]
        public void Controller_Update_Order_No_Error()
        {
            // Arrange
            var salesController = new SalesController(mockSalesService.Object);
            var ord = new Order()
            {
                Id = 66,
                Amount = 11,
                DateCreated = DateTime.Now
            };
            Order obj = null;
            mockSalesService.Setup(nt => nt.Update(ord)).Callback<Order>(x => {
                obj = x;
            });

            // Action
            var ar = salesController.Put(ord);

            // Assert
            mockSalesService.Verify(x => x.Update(ord), Times.Once);
            Assert.AreEqual(ord, obj);
            Assert.IsInstanceOfType(ar, typeof(OkResult));
        }

        [TestMethod]
        public void Controller_Get_Orders_Return_Orders()
        {
            // Arrange
            var salesController = new SalesController(mockSalesService.Object);

            var rnd = new Random();
            var ordersQty = 20;
            var currDate = DateTime.UtcNow;
            var orders = Enumerable.Range(1, ordersQty).Select(x => new Order() {
                Id = x,
                Amount = rnd.Next(x * 1000) + new Random().Next(x) / 100,
                DateCreated = currDate.AddDays(x)
            }).ToList();
            var idForCheck = orders[new Random().Next(ordersQty)].Id;
            var badIdForCheck = -10000;

            var firstNum = rnd.Next(ordersQty - 1);
            var firstDate = orders[firstNum].DateCreated;
            var secondNum = rnd.Next(firstNum, ordersQty);
            var secondDate = orders[secondNum].DateCreated;

            Int32? idFromRequest = null;
            DateTime? startDateFromRequest = null;
            DateTime? endDateFromRequest = null;
            var ordersResults = new List<List<Order>>();
            mockSalesService.Setup(nt => nt.Get(It.IsAny<Int32?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>())).Callback<Int32?, DateTime?, DateTime?>((x, y, z) => {
                idFromRequest = x;
                startDateFromRequest = y;
                endDateFromRequest = z;
            }).ReturnsAsync(() => {
                var res = orders.AsQueryable().ApplyOrderFilter(idFromRequest, startDateFromRequest, endDateFromRequest).ToList();
                ordersResults.Add(res);
                return res;
            });

            // Action
            var result = salesController.Get().Result;
            var resultId = salesController.Get(idForCheck).Result;
            var resultbadId = salesController.Get(badIdForCheck).Result;
            var resultDates = salesController.Get(firstDate, secondDate).Result;

            // Assert
            mockSalesService.Verify(x => x.Get(null, null, null), Times.Once);
            mockSalesService.Verify(x => x.Get(idForCheck, null, null), Times.Once);
            mockSalesService.Verify(x => x.Get(badIdForCheck, null, null), Times.Once);
            mockSalesService.Verify(x => x.Get(null, firstDate, secondDate), Times.Once);

            Assert.AreEqual(4, ordersResults.Count);

            var okResultType = typeof(OkObjectResult);
            var nfResultType = typeof(NotFoundResult);
            Assert.IsInstanceOfType(result, okResultType);
            Assert.IsInstanceOfType(resultId, okResultType);
            Assert.IsInstanceOfType(resultbadId, nfResultType);
            Assert.IsInstanceOfType(resultDates, okResultType);

            Assert.AreEqual(ordersQty, ordersResults[0].Count);
            Assert.AreEqual(1, ordersResults[1].Count);
            Assert.AreEqual(0, ordersResults[2].Count);
            ordersResults[3].ForEach(os =>
            {
                Assert.IsTrue(os.DateCreated >= firstDate && os.DateCreated < secondDate);
            });
        }
    }
}
