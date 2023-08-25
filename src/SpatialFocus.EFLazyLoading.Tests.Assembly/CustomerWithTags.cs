// <copyright file="CustomerWithTags.cs" company="Spatial Focus GmbH">
// Copyright (c) Spatial Focus GmbH. All rights reserved.
// </copyright>

namespace SpatialFocus.EFLazyLoading.Tests.Assembly
{
	using System.Collections.Generic;

	// Lazy loader should only be injected for entities (classes), not for enums
	public class CustomerWithTags
	{
		public CustomerWithTags(string name)
		{
			Name = name;
		}

		public virtual ICollection<Tag> Tags { get; set; } = null!;

		public int Id { get; protected set; }

		public string Name { get; protected set; }
	}
}