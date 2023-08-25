// <copyright file="ClassWeavingContextExtension.cs" company="Spatial Focus GmbH">
// Copyright (c) Spatial Focus GmbH. All rights reserved.
// </copyright>

namespace SpatialFocus.EFLazyLoading.Fody
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Mono.Cecil;

	public static class ClassWeavingContextExtension
	{
		public static ICollection<NavigationPropertyWeavingContext> GetNavigationPropertyCandidates(this ClassWeavingContext context)
		{
			return context.TypeDefinition.Properties.Select(Transform)
				.Where(x => x != null)
				.ToList()!;

			NavigationPropertyWeavingContext? Transform(PropertyDefinition propertyDefinition)
			{
				bool isReadOnlyCollectionInterface = propertyDefinition.PropertyType.GetElementType().Resolve() == context.References.CollectionInterface.Resolve();

				if (!isReadOnlyCollectionInterface)
				{
					return null;
				}

				GenericInstanceType instance = (GenericInstanceType)propertyDefinition.PropertyType;
				TypeDefinition? genericArgument = instance.GenericArguments.FirstOrDefault()?.Resolve();

				if (genericArgument == null || !genericArgument.IsClass || genericArgument.IsString() || genericArgument.IsEnum || genericArgument.IsValueType || genericArgument.IsPrimitive)
				{
					return null;
				}

				string propertyName = propertyDefinition.Name;

				FieldDefinition? fieldDefinition = context.TypeDefinition.Fields.SingleOrDefault(fieldDefinition =>
					string.Equals(fieldDefinition.Name, GetBackingFieldName(propertyName), StringComparison.OrdinalIgnoreCase));

				if (fieldDefinition == null)
				{
					return null;
				}

				return new NavigationPropertyWeavingContext(context, propertyDefinition, fieldDefinition);

				string GetBackingFieldName(string propertyName)
				{
					return string.Format("<{0}>k__BackingField", propertyName);
				}
			}
		}
	}
}