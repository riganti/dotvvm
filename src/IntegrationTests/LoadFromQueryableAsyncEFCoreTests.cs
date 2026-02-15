#if NET8_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Testcontainers.MsSql;

namespace DotVVM.Framework.IntegrationTests
{
    [TestClass]
    public class LoadFromQueryableAsyncEFCoreTests
    {
        private MsSqlContainer? msSqlContainer;

        [TestInitialize]
        public async Task TestInitialize()
        {
            msSqlContainer = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .Build();
            await msSqlContainer.StartAsync();
        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            if (msSqlContainer != null)
            {
                await msSqlContainer.DisposeAsync();
            }
        }

        public class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions options) : base(options)
            {
            }

            public DbSet<TestEntity> Entities { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);
                
                modelBuilder.Entity<TestEntity>()
                    .Property(e => e.Id)
                    .ValueGeneratedNever();
            }
        }

        public record TestEntity(int Id, string Name, int Category);

        private async Task<TestDbContext> InitializeDbContext()
        {
            var connectionString = msSqlContainer!.GetConnectionString();
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            var context = new TestDbContext(options);
            await context.Database.EnsureCreatedAsync();

            // Add test data
            context.Entities.AddRange(
                new TestEntity(1, "Zebra", 1),
                new TestEntity(2, "Yak", 1),
                new TestEntity(3, "Xylophone", 2),
                new TestEntity(4, "Whale", 2),
                new TestEntity(5, "Violin", 3),
                new TestEntity(6, "Umbrella", 3),
                new TestEntity(7, "Tiger", 1),
                new TestEntity(8, "Snake", 2),
                new TestEntity(9, "Rabbit", 3),
                new TestEntity(10, "Quail", 1)
            );
            await context.SaveChangesAsync();
            return context;
        }

        [TestMethod]
        public async Task LoadFromQueryableAsync_Sorting_Ascending()
        {
            using var context = await InitializeDbContext();

            var dataSet = new GridViewDataSet<TestEntity>()
            {
                PagingOptions = { PageSize = 5, PageIndex = 0 },
                SortingOptions = { SortExpression = nameof(TestEntity.Name), SortDescending = false }
            };

            await dataSet.LoadFromQueryableAsync(context.Entities);

            Assert.AreEqual(5, dataSet.Items.Count);
            Assert.AreEqual(10, dataSet.PagingOptions.TotalItemsCount);
            Assert.AreEqual(10, dataSet.Items[0].Id); // Quail
            Assert.AreEqual(9, dataSet.Items[1].Id);  // Rabbit
            Assert.AreEqual(8, dataSet.Items[2].Id);  // Snake
            Assert.AreEqual(7, dataSet.Items[3].Id);  // Tiger
            Assert.AreEqual(6, dataSet.Items[4].Id);  // Umbrella
        }

        [TestMethod]
        public async Task LoadFromQueryableAsync_Sorting_Descending()
        {
            using var context = await InitializeDbContext();

            var dataSet = new GridViewDataSet<TestEntity>()
            {
                PagingOptions = { PageSize = 5, PageIndex = 0 },
                SortingOptions = { SortExpression = nameof(TestEntity.Name), SortDescending = true }
            };

            await dataSet.LoadFromQueryableAsync(context.Entities);

            Assert.AreEqual(5, dataSet.Items.Count);
            Assert.AreEqual(10, dataSet.PagingOptions.TotalItemsCount);
            Assert.AreEqual(1, dataSet.Items[0].Id);  // Zebra
            Assert.AreEqual(2, dataSet.Items[1].Id);  // Yak
            Assert.AreEqual(3, dataSet.Items[2].Id);  // Xylophone
            Assert.AreEqual(4, dataSet.Items[3].Id);  // Whale
            Assert.AreEqual(5, dataSet.Items[4].Id);  // Violin
        }

        [TestMethod]
        public async Task LoadFromQueryableAsync_Paging_FirstPage()
        {
            using var context = await InitializeDbContext();

            var dataSet = new GridViewDataSet<TestEntity>()
            {
                PagingOptions = { PageSize = 3, PageIndex = 0 },
                SortingOptions = { SortExpression = nameof(TestEntity.Id), SortDescending = false }
            };

            await dataSet.LoadFromQueryableAsync(context.Entities);

            Assert.AreEqual(3, dataSet.Items.Count);
            Assert.AreEqual(10, dataSet.PagingOptions.TotalItemsCount);
            Assert.AreEqual(1, dataSet.Items[0].Id);
            Assert.AreEqual(2, dataSet.Items[1].Id);
            Assert.AreEqual(3, dataSet.Items[2].Id);
        }

        [TestMethod]
        public async Task LoadFromQueryableAsync_Paging_SecondPage()
        {
            using var context = await InitializeDbContext();

            var dataSet = new GridViewDataSet<TestEntity>()
            {
                PagingOptions = { PageSize = 3, PageIndex = 1 },
                SortingOptions = { SortExpression = nameof(TestEntity.Id), SortDescending = false }
            };

            await dataSet.LoadFromQueryableAsync(context.Entities);

            Assert.AreEqual(3, dataSet.Items.Count);
            Assert.AreEqual(10, dataSet.PagingOptions.TotalItemsCount);
            Assert.AreEqual(4, dataSet.Items[0].Id);
            Assert.AreEqual(5, dataSet.Items[1].Id);
            Assert.AreEqual(6, dataSet.Items[2].Id);
        }

        [TestMethod]
        public async Task LoadFromQueryableAsync_Paging_WithPreFilter()
        {
            using var context = await InitializeDbContext();

            var dataSet = new GridViewDataSet<TestEntity>()
            {
                PagingOptions = { PageSize = 2, PageIndex = 0 },
                SortingOptions = { SortExpression = nameof(TestEntity.Name), SortDescending = false }
            };

            await dataSet.LoadFromQueryableAsync(context.Entities.Where(e => e.Category == 1));

            Assert.AreEqual(2, dataSet.Items.Count);
            Assert.AreEqual(4, dataSet.PagingOptions.TotalItemsCount);
            Assert.AreEqual(10, dataSet.Items[0].Id); // Quail
            Assert.AreEqual(7, dataSet.Items[1].Id);  // Tiger
        }
    }
}
#endif
