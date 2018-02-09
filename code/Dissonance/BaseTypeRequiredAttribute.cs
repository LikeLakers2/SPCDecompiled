﻿using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	[BaseTypeRequired(typeof(Attribute))]
	internal sealed class BaseTypeRequiredAttribute : Attribute
	{
		public BaseTypeRequiredAttribute([NotNull] Type baseType)
		{
			this.BaseType = baseType;
		}

		[NotNull]
		public Type BaseType { get; private set; }
	}
}
