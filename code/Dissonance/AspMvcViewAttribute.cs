﻿using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
	internal sealed class AspMvcViewAttribute : Attribute
	{
		public AspMvcViewAttribute()
		{
		}
	}
}
