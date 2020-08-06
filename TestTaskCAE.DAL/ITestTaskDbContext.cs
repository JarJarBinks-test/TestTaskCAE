using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Threading;
using System.Threading.Tasks;
using TestTaskCAE.DAL.Objects;

namespace TestTaskCAE.DAL
{
    public interface ITestTaskDbContext
    {
        public DbSet<DbOrder> Orders { get; set; }

        // TODO: For tests only. Bad method. For resolve this we could split base dbClass and main class.
        public DatabaseFacade Database { get; }
        public DbSet<TEntity> Set<TEntity>() where TEntity : class;
        public int SaveChanges();
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
