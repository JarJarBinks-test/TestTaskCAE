using Microsoft.EntityFrameworkCore;
using TestTaskCAE.DAL.Objects;

namespace TestTaskCAE.DAL
{
    public class TestTaskDbContext : DbContext, ITestTaskDbContext
    {
        /* FIXME: Only for generate migrations
        public TestTaskDbContext() : base(CreateDefaultParams().Options)
        {
        }

        public static DbContextOptionsBuilder<TestTaskDbContext> CreateDefaultParams()
        {
            var prm = new DbContextOptionsBuilder<TestTaskDbContext>();
            prm.UseSqlServer("");
            return prm;
        }
        */

        public TestTaskDbContext(DbContextOptions<TestTaskDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<DbOrder> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("TestTask");

            var orderEntity = modelBuilder.Entity<DbOrder>();
            orderEntity.HasKey(x => x.Id);
            orderEntity.Property(x => x.Id).ValueGeneratedOnAdd();

            orderEntity.Property(x => x.DateCreated).IsRequired(true);
            orderEntity.Property(x => x.Amount).IsRequired(true);
        }
    }
}
