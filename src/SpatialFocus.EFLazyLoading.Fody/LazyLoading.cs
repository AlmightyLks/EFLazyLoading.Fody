// <copyright file="LazyLoading.cs" company="Spatial Focus GmbH">
// Copyright (c) Spatial Focus GmbH. All rights reserved.
// </copyright>

namespace SpatialFocus.EFLazyLoading.Fody
{
	using System.Collections.ObjectModel;
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

		public static void AddLazyLoadingToGetter(this NavigationPropertyWeavingContext context, ModuleDefinition module)
		{
			var genericArgs = ((GenericInstanceType)context.PropertyDefinition.PropertyType).GenericArguments;
			var genericArg = genericArgs.First();
			var observableType = module.ImportReference(typeof(ObservableCollection<>)).MakeGenericInstanceType(genericArg);
			var observableCtor = module.ImportReference(observableType.Resolve().Methods.First(m => m.IsConstructor && m.Parameters.Count == 0)).MakeHostInstanceGeneric(genericArg);

			var processorCtx = context.PropertyDefinition.GetMethod.Body.GetILProcessor().Start();
			var returnInstruction = processorCtx.Instruction;

			processorCtx = processorCtx.Prepend(ilProcessor => ilProcessor.Create(OpCodes.Ret));
			processorCtx = processorCtx.Prepend(ilProcessor => ilProcessor.Create(OpCodes.Ldfld, context.FieldDefinition));
			processorCtx = processorCtx.Prepend(ilProcessor => ilProcessor.Create(OpCodes.Ldarg_0));
			processorCtx = processorCtx.Prepend(ilProcessor => ilProcessor.Create(OpCodes.Stfld, context.FieldDefinition));
			processorCtx = processorCtx.Prepend(ilProcessor => ilProcessor.Create(OpCodes.Newobj, observableCtor));
			processorCtx = processorCtx.Prepend(ilProcessor => ilProcessor.Create(OpCodes.Ldarg_0));
			Instruction? newObjInstruction = processorCtx.Instruction;
			processorCtx = processorCtx.Prepend(ilProcessor => ilProcessor.Create(OpCodes.Br_S, returnInstruction));
			processorCtx = processorCtx.Prepend(ilProcessor => ilProcessor.Create(OpCodes.Callvirt, context.ClassWeavingContext.References.LazyLoaderInvokeMethod));
			processorCtx = processorCtx.Prepend(ilProcessor => ilProcessor.Create(OpCodes.Ldstr, context.PropertyDefinition.Name));
			processorCtx = processorCtx.Prepend(ilProcessor => ilProcessor.Create(OpCodes.Ldarg_0));
			processorCtx = processorCtx.Prepend(ilProcessor => ilProcessor.Create(OpCodes.Ldfld, context.ClassWeavingContext.LazyLoaderField));
			processorCtx = processorCtx.Prepend(ilProcessor => ilProcessor.Create(OpCodes.Ldarg_0));
			processorCtx = processorCtx.Prepend(ilProcessor => ilProcessor.Create(OpCodes.Brfalse_S, newObjInstruction));
			processorCtx = processorCtx.Prepend(ilProcessor => ilProcessor.Create(OpCodes.Ldfld, context.ClassWeavingContext.LazyLoaderField));
			processorCtx = processorCtx.Prepend(ilProcessor => ilProcessor.Create(OpCodes.Ldarg_0));
		}
	}
}