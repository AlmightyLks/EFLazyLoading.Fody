using System;
using System.Linq;
using Fody;
using Microsoft.EntityFrameworkCore;
using SpatialFocus.EFLazyLoading.Fody;
using SpatialFocus.EFLazyLoading.Tests.Assembly;
using Xunit;

namespace SpatialFocus.EFLazyLoading.Tests
{
	public class EntityFrameworkCoreTests
	{
		private static readonly TestResult TestResult;

		static EntityFrameworkCoreTests()
		{
			var weavingTask = new ModuleWeaver();
			EntityFrameworkCoreTests.TestResult = weavingTask.ExecuteTestRun($"{typeof(Customer).Namespace}.dll", ignoreCodes: new[] { "0x80131869" });
		}

		public EntityFrameworkCoreTests()
		{
			using var ctx = TestHelpers.CreateInstance<EFCTestContext>(TestResult.Assembly);
			for (int i = 0; i < 100; i++)
			{
				ctx.Customers.Add(new Customer(i.ToString()));
			}

			ctx.SaveChanges();
			ctx.ChangeTracker.Clear();

			foreach (var customer in ctx.Customers)
			{
				for (int i = 0; i < 100; i++)
				{
					// customer.Orders -> NRE
					customer.Orders.Add(new Order($"{customer.Id}-{i}", Random.Shared.Next()));
				}
			}

			ctx.SaveChanges();
		}

		[Fact]
		public void LazyLoadedNotEmpty()
		{
			using var ctx = new EFCTestContext();

			Assert.NotEmpty(ctx.Customers.First().Orders);
		}

		public class EFCTestContext : DbContext
		{
			public DbSet<Customer> Customers { get; set; }
			public DbSet<Order> Orders { get; set; }

			protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
			{
				optionsBuilder.UseInMemoryDatabase("EFC");
			}

			protected override void OnModelCreating(ModelBuilder modelBuilder)
			{
				modelBuilder.Entity<Customer>()
					.HasMany(x => x.Orders)
					.WithOne();
			}
		}
	}
}
