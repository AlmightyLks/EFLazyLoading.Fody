// <copyright file="LazyLoading.cs" company="Spatial Focus GmbH">
// Copyright (c) Spatial Focus GmbH. All rights reserved.
// </copyright>

namespace SpatialFocus.EFLazyLoading.Fody
{
	using System.Linq;
	using Mono.Cecil;
	using Mono.Cecil.Cil;
	using Mono.Cecil.Rocks;

	public static class LazyLoading
	{
		public static void AddConstructorOverloads(this ClassWeavingContext context)
		{
			foreach (MethodDefinition constructor in context.TypeDefinition.GetConstructors().ToList())
			{
				context.WriteDebug($"Add constructor overload for {context.TypeDefinition.Name}({string.Join(", ", constructor.Parameters.Select(x => $"{x.ParameterType.Name} {x.Name}"))})");

				MethodDefinition method = new MethodDefinition(constructor.Name, constructor.Attributes, context.TypeDefinition.Module.TypeSystem.Void)
				{
					IsFamily = true,
				};

				foreach (ParameterDefinition parameterDefinition in constructor.Parameters)
				{
					method.Parameters.Add(new ParameterDefinition(parameterDefinition.Name, parameterDefinition.Attributes, parameterDefinition.ParameterType));
				}

				ParameterDefinition lazyLoaderParameter = new ParameterDefinition("lazyLoader", ParameterAttributes.None, context.References.LazyLoaderType);
				method.Parameters.Add(lazyLoaderParameter);

				context.TypeDefinition.Methods.Add(method);

				ILProcessorContext processor = method.Body.GetILProcessor().Start();

				processor = processor.Append(x => x.Create(OpCodes.Ldarg_0));

				foreach (ParameterDefinition parameterDefinition in constructor.Parameters)
				{
					processor = processor.Append(x => x.Create(OpCodes.Ldarg, parameterDefinition.Index + 1));
				}

				processor = processor.Append(x => x.Create(OpCodes.Call, constructor));

				processor = processor.Append(x => x.Create(OpCodes.Ldarg_0))
					.Append(x => x.Create(OpCodes.Ldarg, lazyLoaderParameter))
					.Append(x => x.Create(OpCodes.Stfld, context.LazyLoaderField))
					.Append(x => x.Create(OpCodes.Ret));
			}
		}

		public static void AddFieldDefinition(this ClassWeavingContext context)
		{
			context.WriteDebug($"Add lazy loader field to {context.TypeDefinition.Name}");

			context.LazyLoaderField = new FieldDefinition("lazyLoader", FieldAttributes.Private | FieldAttributes.InitOnly, context.References.LazyLoaderType);
			context.TypeDefinition.Fields.Add(context.LazyLoaderField);
		}

		public static void AddLazyLoadingToGetter(this NavigationPropertyWeavingContext context)
		{
			var processorCtx = context.PropertyDefinition.GetMethod.Body.GetILProcessor().Start();

			processorCtx
				.Prepend(ilProcessor => ilProcessor.Create(OpCodes.Ret))
				.Prepend(ilProcessor => ilProcessor.Create(OpCodes.Ldfld, context.FieldDefinition))
				.Prepend(ilProcessor => ilProcessor.Create(OpCodes.Ldarg_0))
				.Prepend(ilProcessor => ilProcessor.Create(OpCodes.Stfld, context.FieldDefinition))
				.Prepend(ilProcessor => ilProcessor.Create(OpCodes.Newobj, context.ClassWeavingContext.References.DefaultCollectionCtor))
				.Prepend(ilProcessor => ilProcessor.Create(OpCodes.Ldarg_0))
				.Prepend(ilProcessor => ilProcessor.Create(OpCodes.Br_S, processorCtx.Instruction))
				.Prepend(ilProcessor => ilProcessor.Create(OpCodes.Callvirt, context.ClassWeavingContext.References.LazyLoaderInvokeMethod))
				.Prepend(ilProcessor => ilProcessor.Create(OpCodes.Ldstr, context.PropertyDefinition.Name))
				.Prepend(ilProcessor => ilProcessor.Create(OpCodes.Ldarg_0))
				.Prepend(ilProcessor => ilProcessor.Create(OpCodes.Ldfld, context.ClassWeavingContext.LazyLoaderField))
				.Prepend(ilProcessor => ilProcessor.Create(OpCodes.Ldarg_0))
				.Prepend(ilProcessor => ilProcessor.Create(OpCodes.Brfalse_S, processorCtx.Instruction))
				.Prepend(ilProcessor => ilProcessor.Create(OpCodes.Ldfld, context.ClassWeavingContext.LazyLoaderField))
				.Prepend(ilProcessor => ilProcessor.Create(OpCodes.Ldarg_0));

			// Insert the new instructions into the method body
			//context.PropertyDefinition.GetMethod.Body.Instructions.Insert(0, returnFieldInstruction);

			// context.PropertyDefinition.GetMethod.Body.GetILProcessor().Start()
			// .Prepend(x => x.Create(OpCodes.Callvirt, context.ClassWeavingContext.References.LazyLoaderInvokeMethod))
			// .Prepend(x => x.Create(OpCodes.Ldstr, context.PropertyDefinition.Name))
			// .Prepend(x => x.Create(OpCodes.Ldarg_0))
			// .Prepend(x => x.Create(OpCodes.Ldfld, context.ClassWeavingContext.LazyLoaderField))
			// .Prepend(x => x.Create(OpCodes.Ldarg_0));
		}
	}
}