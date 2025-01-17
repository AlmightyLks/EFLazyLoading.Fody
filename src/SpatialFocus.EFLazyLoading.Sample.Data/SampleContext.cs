﻿// <copyright file="SampleContext.cs" company="Spatial Focus GmbH">
// Copyright (c) Spatial Focus GmbH. All rights reserved.
// </copyright>

namespace SpatialFocus.EFLazyLoading.Sample.Data
{
	using Microsoft.EntityFrameworkCore;

	public class SampleContext : DbContext
	{
		public SampleContext(DbContextOptions<SampleContext> options) : base(options)
		{
		}

		public DbSet<Customer> Customers { get; set; } = null!;

		public DbSet<Order> Orders { get; set; } = null!;

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Customer>().HasMany(x => x.Orders).WithOne();
		}
	}
}