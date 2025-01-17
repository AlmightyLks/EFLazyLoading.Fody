﻿// <copyright file="References.cs" company="Spatial Focus GmbH">
// Copyright (c) Spatial Focus GmbH. All rights reserved.
// </copyright>

namespace SpatialFocus.EFLazyLoading.Fody
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.CompilerServices;
	using Mono.Cecil;
	using Mono.Cecil.Rocks;

	public class References
	{
		protected References(ModuleWeaver moduleWeaver)
		{
			ModuleWeaver = moduleWeaver;
		}

		public TypeReference CompilerGeneratedAttributeType { get; set; } = null!;

		public MethodReference ExtensionLoadMethod { get; set; } = null!;

		public MethodReference LazyLoaderInvokeMethod { get; set; } = null!;

		public TypeReference LazyLoaderType { get; set; } = null!;

		public TypeReference CollectionInterface { get; set; } = null!;

		protected ModuleWeaver ModuleWeaver { get; }

		public static References Init(ModuleWeaver moduleWeaver)
		{
			if (moduleWeaver == null)
			{
				throw new ArgumentNullException(nameof(moduleWeaver));
			}

			References references = new References(moduleWeaver);

			TypeDefinition compilerGeneratedAttributeType = moduleWeaver.FindTypeDefinition(typeof(CompilerGeneratedAttribute).FullName);
			references.CompilerGeneratedAttributeType = moduleWeaver.ModuleDefinition.ImportReference(compilerGeneratedAttributeType);

			TypeDefinition lazyLoaderType = moduleWeaver.FindTypeDefinition(typeof(Action<,>).Name);
			references.LazyLoaderType = moduleWeaver.ModuleDefinition.ImportReference(lazyLoaderType)
				.MakeGenericInstanceType(moduleWeaver.TypeSystem.ObjectReference, moduleWeaver.TypeSystem.StringReference);

			MethodDefinition lazyLoaderInvokeMethod = references.LazyLoaderType.Resolve().Methods.Single(x => x.Name == "Invoke");
			references.LazyLoaderInvokeMethod = moduleWeaver.ModuleDefinition.ImportReference(lazyLoaderInvokeMethod)
				.MakeHostInstanceGeneric(moduleWeaver.TypeSystem.ObjectReference, moduleWeaver.TypeSystem.StringReference);

			TypeDefinition collectionInterface = moduleWeaver.FindTypeDefinition(typeof(ICollection<>).Name);
			references.CollectionInterface = moduleWeaver.ModuleDefinition.ImportReference(collectionInterface);

			return references;
		}
	}
}