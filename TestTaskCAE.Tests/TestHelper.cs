using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace TestTaskCAE.Tests
{
    public static class TestHelper
    {
        public static Mock<DbSet<T>> ToDbSetMock<T>(IEnumerable<T> sourceList) where T : class
        {
            var data = sourceList.AsQueryable();
            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());
            return dbSet;
        }
    }
}
