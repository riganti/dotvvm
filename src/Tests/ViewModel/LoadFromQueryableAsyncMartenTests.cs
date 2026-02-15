#if NET8_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using Marten;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Testcontainers.PostgreSql;

namespace DotVVM.Framework.Tests.ViewModel
{
    [TestClass]
    public class LoadFromQueryableAsyncMartenTests
    {
        private PostgreSqlContainer? postgreSqlContainer;

        [TestInitialize]
        public async Task TestInitialize()
        {
            postgreSqlContainer = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .Build();
            await postgreSqlContainer.StartAsync();
        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            if (postgreSqlContainer != null)
            {
                await postgreSqlContainer.DisposeAsync();
            }
        }

        public class TestEntity
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public int Category { get; set; }
        }

        private async Task<IDocumentStore> InitializeDocumentStore()
        {
            var connectionString = postgreSqlContainer!.GetConnectionString();
            var store = DocumentStore.For(options =>
            {
                options.Connection(connectionString);
                options.AutoCreateSchemaObjects = Weasel.Core.AutoCreate.All;
            });

            // Add test data
            using var session = store.LightweightSession();
            session.Store(new TestEntity { Id = 1, Name = "Zebra", Category = 1 });
            session.Store(new TestEntity { Id = 2, Name = "Yak", Category = 1 });
            session.Store(new TestEntity { Id = 3, Name = "Xylophone", Category = 2 });
            session.Store(new TestEntity { Id = 4, Name = "Whale", Category = 2 });
            session.Store(new TestEntity { Id = 5, Name = "Violin", Category = 3 });
            session.Store(new TestEntity { Id = 6, Name = "Umbrella", Category = 3 });
            session.Store(new TestEntity { Id = 7, Name = "Tiger", Category = 1 });
            session.Store(new TestEntity { Id = 8, Name = "Snake", Category = 2 });
            session.Store(new TestEntity { Id = 9, Name = "Rabbit", Category = 3 });
            session.Store(new TestEntity { Id = 10, Name = "Quail", Category = 1 });
            await session.SaveChangesAsync();

            return store;
        }

        [TestMethod]
        public async Task LoadFromQueryableAsync_Sorting_Ascending()
        {
            using var store = await InitializeDocumentStore();
            using var session = store.QuerySession();

            var dataSet = new GridViewDataSet<TestEntity>()
            {
                PagingOptions = { PageSize = 5, PageIndex = 0 },
                SortingOptions = { SortExpression = nameof(TestEntity.Name), SortDescending = false }
            };

            await dataSet.LoadFromQueryableAsync(session.Query<TestEntity>());

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
            using var store = await InitializeDocumentStore();
            using var session = store.QuerySession();

            var dataSet = new GridViewDataSet<TestEntity>()
            {
                PagingOptions = { PageSize = 5, PageIndex = 0 },
                SortingOptions = { SortExpression = nameof(TestEntity.Name), SortDescending = true }
            };

            await dataSet.LoadFromQueryableAsync(session.Query<TestEntity>());

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
            using var store = await InitializeDocumentStore();
            using var session = store.QuerySession();

            var dataSet = new GridViewDataSet<TestEntity>()
            {
                PagingOptions = { PageSize = 3, PageIndex = 0 },
                SortingOptions = { SortExpression = nameof(TestEntity.Id), SortDescending = false }
            };

            await dataSet.LoadFromQueryableAsync(session.Query<TestEntity>());

            Assert.AreEqual(3, dataSet.Items.Count);
            Assert.AreEqual(10, dataSet.PagingOptions.TotalItemsCount);
            Assert.AreEqual(1, dataSet.Items[0].Id);
            Assert.AreEqual(2, dataSet.Items[1].Id);
            Assert.AreEqual(3, dataSet.Items[2].Id);
        }

        [TestMethod]
        public async Task LoadFromQueryableAsync_Paging_SecondPage()
        {
            using var store = await InitializeDocumentStore();
            using var session = store.QuerySession();

            var dataSet = new GridViewDataSet<TestEntity>()
            {
                PagingOptions = { PageSize = 3, PageIndex = 1 },
                SortingOptions = { SortExpression = nameof(TestEntity.Id), SortDescending = false }
            };

            await dataSet.LoadFromQueryableAsync(session.Query<TestEntity>());

            Assert.AreEqual(3, dataSet.Items.Count);
            Assert.AreEqual(10, dataSet.PagingOptions.TotalItemsCount);
            Assert.AreEqual(4, dataSet.Items[0].Id);
            Assert.AreEqual(5, dataSet.Items[1].Id);
            Assert.AreEqual(6, dataSet.Items[2].Id);
        }

        [TestMethod]
        public async Task LoadFromQueryableAsync_Paging_WithPreFilter()
        {
            using var store = await InitializeDocumentStore();
            using var session = store.QuerySession();

            var dataSet = new GridViewDataSet<TestEntity>()
            {
                PagingOptions = { PageSize = 2, PageIndex = 0 },
                SortingOptions = { SortExpression = nameof(TestEntity.Name), SortDescending = false }
            };

            await dataSet.LoadFromQueryableAsync(session.Query<TestEntity>().Where(e => e.Category == 1));

            Assert.AreEqual(2, dataSet.Items.Count);
            Assert.AreEqual(4, dataSet.PagingOptions.TotalItemsCount);
            Assert.AreEqual(10, dataSet.Items[0].Id); // Quail
            Assert.AreEqual(7, dataSet.Items[1].Id);  // Tiger
        }
    }
}
#endif
