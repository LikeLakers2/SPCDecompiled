﻿using System;

namespace UnityEngine.PostProcessing
{
	[Serializable]
	public class BloomModel : PostProcessingModel
	{
		public BloomModel.Settings settings
		{
			get
			{
				return this.m_Settings;
			}
			set
			{
				this.m_Settings = value;
			}
		}

		public BloomModel()
		{
		}

		public override void Reset()
		{
			this.m_Settings = BloomModel.Settings.defaultSettings;
		}

		[SerializeField]
		private BloomModel.Settings m_Settings = BloomModel.Settings.defaultSettings;

		[Serializable]
		public struct BloomSettings
		{
			public float thresholdLinear
			{
				get
				{
					return Mathf.GammaToLinearSpace(this.threshold);
				}
				set
				{
					this.threshold = Mathf.LinearToGammaSpace(value);
				}
			}

			public static BloomModel.BloomSettings defaultSettings
			{
				get
				{
					return new BloomModel.BloomSettings
					{
						intensity = 0.5f,
						threshold = 1.1f,
						softKnee = 0.5f,
						radius = 4f,
						antiFlicker = false
					};
				}
			}

			[Tooltip("Blend factor of the result image.")]
			[Min(0f)]
			public float intensity;

			[Tooltip("Filters out pixels under this level of brightness.")]
			[Min(0f)]
			public float threshold;

			[Tooltip("Makes transition between under/over-threshold gradual (0 = hard threshold, 1 = soft threshold).")]
			[Range(0f, 1f)]
			public float softKnee;

			[Tooltip("Changes extent of veiling effects in a screen resolution-independent fashion.")]
			[Range(1f, 7f)]
			public float radius;

			[Tooltip("Reduces flashing noise with an additional filter.")]
			public bool antiFlicker;
		}

		[Serializable]
		public struct LensDirtSettings
		{
			public static BloomModel.LensDirtSettings defaultSettings
			{
				get
				{
					return new BloomModel.LensDirtSettings
					{
						texture = null,
						intensity = 3f
					};
				}
			}

			[Tooltip("Dirtiness texture to add smudges or dust to the lens.")]
			public Texture texture;

			[Tooltip("Amount of lens dirtiness.")]
			[Min(0f)]
			public float intensity;
		}

		[Serializable]
		public struct Settings
		{
			public static BloomModel.Settings defaultSettings
			{
				get
				{
					return new BloomModel.Settings
					{
						bloom = BloomModel.BloomSettings.defaultSettings,
						lensDirt = BloomModel.LensDirtSettings.defaultSettings
					};
				}
			}

			public BloomModel.BloomSettings bloom;

			public BloomModel.LensDirtSettings lensDirt;
		}
	}
}
