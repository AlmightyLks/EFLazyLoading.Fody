// <copyright file="Customer.cs" company="Spatial Focus GmbH">
// Copyright (c) Spatial Focus GmbH. All rights reserved.
// </copyright>

namespace SpatialFocus.EFLazyLoading.Tests.Assembly
{
	using System.Collections.Generic;

	public class Customer
	{
		public Customer(string name)
		{
			Name = name;
		}

		public int NumberOfOrders => Orders.Count;

		public virtual ICollection<Order> Orders { get; set; } = null!;

		public int Id { get; protected set; }

		public string Name { get; protected set; }

		public class Nested
		{
			private readonly Customer customer;

			public Nested(Customer customer)
			{
				this.customer = customer;
			}

			public virtual ICollection<Order> Orders => this.customer.Orders;
		}
	}
}