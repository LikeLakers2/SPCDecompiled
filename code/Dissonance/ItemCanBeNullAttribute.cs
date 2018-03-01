﻿using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Delegate)]
	internal sealed class ItemCanBeNullAttribute : Attribute
	{
		public ItemCanBeNullAttribute()
		{
		}
	}
}
