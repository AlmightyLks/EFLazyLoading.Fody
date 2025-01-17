﻿// <copyright file="OtherCustomer.cs" company="Spatial Focus GmbH">
// Copyright (c) Spatial Focus GmbH. All rights reserved.
// </copyright>

namespace SpatialFocus.EFLazyLoading.Tests.Assembly
{
	using System;
	using System.Collections.Generic;

	public class CustomerWithLazyLoaderField
	{
#pragma warning disable 169
		private readonly Action<object, string>? lazyLoader;
#pragma warning restore 169

		public CustomerWithLazyLoaderField(string name)
		{
			Name = name;
		}

		public virtual ICollection<Order> Orders { get; set; } = null!;

		public int Id { get; protected set; }

		public string Name { get; protected set; }
	}
}