// <copyright file="CustomerWithTagsAndOrders.cs" company="Spatial Focus GmbH">
// Copyright (c) Spatial Focus GmbH. All rights reserved.
// </copyright>

namespace SpatialFocus.EFLazyLoading.Tests.Assembly
{
	using System.Collections.Generic;

	public class CustomerWithTagsAndOrders
	{
		public CustomerWithTagsAndOrders(string name)
		{
			Name = name;
		}

		public int NumberOfOrders => Orders.Count;

		public virtual ICollection<Order> Orders { get; set; } = null!;

		public virtual ICollection<Tag> Tags { get; set; } = null!;

		public int Id { get; protected set; }

		public string Name { get; protected set; }
	}
}