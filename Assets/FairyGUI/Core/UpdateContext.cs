﻿using System.Collections.Generic;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// UpdateContext is for internal use.
	/// </summary>
	public class UpdateContext
	{
		public struct ClipInfo
		{
			public Rect rect;
			public Vector4 clipBox;
			public bool soft;
			public Vector4 softness;//left-top-right-bottom
			public uint clipId;
			public bool stencil;
		}

		Stack<ClipInfo> _clipStack;

		public bool clipped;
		public ClipInfo clipInfo;

		public int counter;
		public int renderingOrder;
		public int batchingDepth;
		public int rectMaskDepth;
		public int stencilReferenceValue;
		public float alpha;
		public bool grayed;

		public static uint frameId;
		public static EventCallback0 OnBegin;
		public static EventCallback0 OnEnd;

		public UpdateContext()
		{
			_clipStack = new Stack<ClipInfo>();
			frameId = 1;
		}

		public void Begin()
		{
			frameId++;
			if (frameId == 0)
				frameId = 1;
			counter = 0;
			renderingOrder = 0;
			batchingDepth = 0;
			rectMaskDepth = 0;
			stencilReferenceValue = 0;
			alpha = 1;
			grayed = false;

			clipped = false;
			_clipStack.Clear();

			if (OnBegin != null)
				OnBegin.Invoke();

			OnBegin = null;
		}

		public void End()
		{
			if (OnEnd != null)
				OnEnd.Invoke();

			OnEnd = null;
		}

		public void EnterClipping(uint clipId, Rect? clipRect, Vector4? softness)
		{
			_clipStack.Push(clipInfo);

			if (clipRect == null)
			{
				if (stencilReferenceValue == 0)
					stencilReferenceValue = 1;
				else
					stencilReferenceValue = stencilReferenceValue << 1;
				clipInfo.clipId = clipId;
				clipInfo.stencil = true;
				clipped = true;
			}
			else
			{
				rectMaskDepth++;
				clipInfo.stencil = false;

				Rect rect = (Rect)clipRect;
				if (clipped)
					rect = ToolSet.Intersection(ref clipInfo.rect, ref rect);
				clipped = true;

				/* clipPos = xy * clipBox.zw + clipBox.xy
					* 利用这个公式，使clipPos变为当前顶点距离剪切区域中心的距离值，剪切区域的大小为2x2
					* 那么abs(clipPos)>1的都是在剪切区域外
					*/

				clipInfo.rect = rect;
				rect.x = rect.x + rect.width / 2f;
				rect.y = rect.y + rect.height / 2f;
				rect.width /= 2f;
				rect.height /= 2f;
				if (rect.width == 0 || rect.height == 0)
					clipInfo.clipBox = new Vector4(-2, -2, 0, 0);
				else
					clipInfo.clipBox = new Vector4(-rect.x / rect.width, -rect.y / rect.height,
						1.0f / rect.width, 1.0f / rect.height);
				clipInfo.clipId = clipId;
				clipInfo.soft = softness != null;
				if (clipInfo.soft)
				{
					clipInfo.softness = (Vector4)softness;
					float vx = clipInfo.rect.width * Screen.height * 0.25f;
					float vy = clipInfo.rect.height * Screen.height * 0.25f;

					if (clipInfo.softness.x > 0)
						clipInfo.softness.x = vx / clipInfo.softness.x;
					else
						clipInfo.softness.x = 10000f;

					if (clipInfo.softness.y > 0)
						clipInfo.softness.y = vy / clipInfo.softness.y;
					else
						clipInfo.softness.y = 10000f;

					if (clipInfo.softness.z > 0)
						clipInfo.softness.z = vx / clipInfo.softness.z;
					else
						clipInfo.softness.z = 10000f;

					if (clipInfo.softness.w > 0)
						clipInfo.softness.w = vy / clipInfo.softness.w;
					else
						clipInfo.softness.w = 10000f;
				}
			}
		}

		public void LeaveClipping()
		{
			if (clipInfo.stencil)
				stencilReferenceValue = stencilReferenceValue >> 1;
			else
				rectMaskDepth--;

			clipInfo = _clipStack.Pop();
			clipped = _clipStack.Count > 0;
		}
	}
}
