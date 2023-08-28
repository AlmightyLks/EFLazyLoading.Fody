// <copyright file="ILProcessorContext.cs" company="Spatial Focus GmbH">
// Copyright (c) Spatial Focus GmbH. All rights reserved.
// </copyright>

namespace SpatialFocus.EFLazyLoading.Fody
{
	using System;
	using System.Linq;
	using Mono.Cecil.Cil;

	public class ILProcessorContext
	{
		public ILProcessorContext(ILProcessor processor, Instruction? currentInstruction)
		{
			Processor = processor;
			Instruction = currentInstruction;
		}

		public Instruction? Instruction { get; set; }

		public ILProcessor Processor { get; }

		public ILProcessorContext Append(Func<ILProcessor, Instruction> action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			Instruction instruction = action(Processor);

			if (Instruction == null)
			{
				if (Processor.Body.Instructions.Count == 0)
				{
					Processor.Append(instruction);
				}
				else
				{
					Processor.InsertBefore(Processor.Body.Instructions.First(), instruction);
				}
			}
			else
			{
				Processor.InsertAfter(Instruction, instruction);
			}

			return new ILProcessorContext(Processor, instruction);
		}

		public ILProcessorContext Prepend(Func<ILProcessor, Instruction> action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			Instruction instruction = action(Processor);

			if (Instruction == null)
			{
				if (Processor.Body.Instructions.Count == 0)
				{
					Processor.Append(instruction);
				}
				else
				{
					Processor.InsertBefore(Processor.Body.Instructions.First(), instruction);
				}
			}
			else
			{
				Processor.InsertBefore(Instruction, instruction);
			}

			return new ILProcessorContext(Processor, instruction);
		}
	}
}