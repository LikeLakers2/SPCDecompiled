﻿using System;
using UnityEngine;

namespace Kino
{
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Kino Image Effects/Analog Glitch")]
	[ExecuteInEditMode]
	public class AnalogGlitch : MonoBehaviour
	{
		public float scanLineJitter
		{
			get
			{
				return this._scanLineJitter;
			}
			set
			{
				this._scanLineJitter = value;
			}
		}

		public float verticalJump
		{
			get
			{
				return this._verticalJump;
			}
			set
			{
				this._verticalJump = value;
			}
		}

		public float horizontalShake
		{
			get
			{
				return this._horizontalShake;
			}
			set
			{
				this._horizontalShake = value;
			}
		}

		public float colorDrift
		{
			get
			{
				return this._colorDrift;
			}
			set
			{
				this._colorDrift = value;
			}
		}

		public AnalogGlitch()
		{
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (this._material == null)
			{
				this._material = new Material(this._shader);
				this._material.hideFlags = HideFlags.DontSave;
			}
			this._verticalJumpTime += Time.deltaTime * this._verticalJump * 11.3f;
			float y = Mathf.Clamp01(1f - this._scanLineJitter * 1.2f);
			float x = 0.002f + Mathf.Pow(this._scanLineJitter, 3f) * 0.05f;
			this._material.SetVector("_ScanLineJitter", new Vector2(x, y));
			Vector2 v = new Vector2(this._verticalJump, this._verticalJumpTime);
			this._material.SetVector("_VerticalJump", v);
			this._material.SetFloat("_HorizontalShake", this._horizontalShake * 0.2f);
			Vector2 v2 = new Vector2(this._colorDrift * 0.04f, Time.time * 606.11f);
			this._material.SetVector("_ColorDrift", v2);
			Graphics.Blit(source, destination, this._material);
		}

		[Range(0f, 1f)]
		[SerializeField]
		private float _scanLineJitter;

		[Range(0f, 1f)]
		[SerializeField]
		private float _verticalJump;

		[Range(0f, 1f)]
		[SerializeField]
		private float _horizontalShake;

		[Range(0f, 1f)]
		[SerializeField]
		private float _colorDrift;

		[SerializeField]
		private Shader _shader;

		private Material _material;

		private float _verticalJumpTime;
	}
}
