// <copyright file="CustomerWithInjectedLazyLoader.cs" company="Spatial Focus GmbH">
// Copyright (c) Spatial Focus GmbH. All rights reserved.
// </copyright>

namespace SpatialFocus.EFLazyLoading.Tests.Assembly
{
	using System;
	using System.Collections.Generic;

	public class CustomerWithInjectedLazyLoader
	{
		private readonly Action<object, string>? lazyLoader;

		public CustomerWithInjectedLazyLoader(string name, Action<object, string> lazyLoader)
		{
			Name = name;
			this.lazyLoader = lazyLoader;
		}

		public int NumberOfOrders => this.Orders?.Count ?? 0;

		public virtual ICollection<Order> Orders { get; set; } = null!;

		public int Id { get; protected set; }

		public string Name { get; protected set; }
	}
}