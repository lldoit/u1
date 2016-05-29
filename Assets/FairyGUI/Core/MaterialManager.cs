using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// Every texture and shader combination has a MaterialManager.
	/// </summary>
	public class MaterialManager
	{
		public NMaterial sharedMaterial { get; private set; }
		public NMaterial grayedSharedMaterial { get; private set; }
		public NTexture texture { get; private set; }
		public string shaderName { get; private set; }

		MaterialPool[] _pools;

		static string[] GRAYED = new string[] { "GRAYED" };
		static string[] CLIPPED = new string[] { "CLIPPED" };
		static string[] CLIPPED_GRAYED = new string[] { "CLIPPED", "GRAYED" };
		static string[] SOFT_CLIPPED = new string[] { "SOFT_CLIPPED" };
		static string[] SOFT_CLIPPED_GRAYED = new string[] { "SOFT_CLIPPED", "GRAYED" };
		static string[] ALPHA_MASK = new string[] { "ALPHA_MASK" };

		public static MaterialManager GetInstance(NTexture texture, string shaderName)
		{
			NTexture rootTexture = texture.root;
			if (rootTexture.materialManagers == null)
				rootTexture.materialManagers = new Dictionary<string, MaterialManager>();

			MaterialManager mm;
			if (!rootTexture.materialManagers.TryGetValue(shaderName, out mm))
			{
				mm = new MaterialManager(rootTexture);
				mm.shaderName = shaderName;
				rootTexture.materialManagers.Add(shaderName, mm);
			}

			if (mm.sharedMaterial == null)
				mm.sharedMaterial = mm.CreateMaterial();

			return mm;
		}

		public MaterialManager(NTexture texture)
		{
			this.texture = texture;

			_pools = new MaterialPool[7];
			_pools[0] = new MaterialPool(this, null); //none
			_pools[1] = new MaterialPool(this, GRAYED); //grayed
			_pools[2] = new MaterialPool(this, CLIPPED); //clipped
			_pools[3] = new MaterialPool(this, CLIPPED_GRAYED); //clipped+grayed
			_pools[4] = new MaterialPool(this, SOFT_CLIPPED); //softClipped
			_pools[5] = new MaterialPool(this, SOFT_CLIPPED_GRAYED); //softClipped+grayed
			_pools[6] = new MaterialPool(this, ALPHA_MASK); //stencil mask
		}

		public Material GetContextMaterial(NGraphics grahpics, UpdateContext context)
		{
			if (context.clipped)
			{
				NMaterial mat;
				if (grahpics.maskFrameId == context.frameId)
				{
					mat = _pools[6].GetMaterial(context.frameId, context.clipInfo.clipId);
				}
				else if (context.rectMaskDepth == 0)
				{
					if (grahpics.grayed)
						mat = _pools[1].GetMaterial(context.frameId, context.clipInfo.clipId);
					else
						mat = _pools[0].GetMaterial(context.frameId, context.clipInfo.clipId);
				}
				else
				{
					if (context.clipInfo.soft)
					{
						if (grahpics.grayed)
							mat = _pools[5].GetMaterial(context.frameId, context.clipInfo.clipId);
						else
							mat = _pools[4].GetMaterial(context.frameId, context.clipInfo.clipId);

						mat.SetVector("_ClipSoftness", context.clipInfo.softness);
					}
					else
					{
						if (grahpics.grayed)
							mat = _pools[3].GetMaterial(context.frameId, context.clipInfo.clipId);
						else
							mat = _pools[2].GetMaterial(context.frameId, context.clipInfo.clipId);
					}

					mat.SetVector("_ClipBox", context.clipInfo.clipBox);
				}

				if (context.stencilReferenceValue > 0)
				{
					if (grahpics.maskFrameId == context.frameId)
					{
						if (context.stencilReferenceValue == 1)
						{
							mat.SetInt("_StencilComp", (int)UnityEngine.Rendering.CompareFunction.Always);
							mat.SetInt("_Stencil", 1);
							mat.SetInt("_StencilOp", (int)UnityEngine.Rendering.StencilOp.Replace);
							mat.SetInt("_StencilReadMask", 255);
							mat.SetInt("_ColorMask", 0);
						}
						else
						{
							mat.SetInt("_StencilComp", (int)UnityEngine.Rendering.CompareFunction.Equal);
							mat.SetInt("_Stencil", context.stencilReferenceValue | (context.stencilReferenceValue - 1));
							mat.SetInt("_StencilOp", (int)UnityEngine.Rendering.StencilOp.Replace);
							mat.SetInt("_StencilReadMask", context.stencilReferenceValue - 1);
							mat.SetInt("_ColorMask", 0);
						}
					}
					else
					{
						mat.SetInt("_StencilComp", (int)UnityEngine.Rendering.CompareFunction.Equal);
						mat.SetInt("_Stencil", context.stencilReferenceValue | (context.stencilReferenceValue - 1));
						mat.SetInt("_StencilOp", (int)UnityEngine.Rendering.StencilOp.Keep);
						mat.SetInt("_StencilReadMask", context.stencilReferenceValue | (context.stencilReferenceValue - 1));
						mat.SetInt("_ColorMask", 15);
					}
					mat.stencilSet = true;
				}
				else if (mat.stencilSet)
				{
					mat.SetInt("_StencilComp", (int)UnityEngine.Rendering.CompareFunction.Always);
					mat.SetInt("_Stencil", 0);
					mat.SetInt("_StencilOp", (int)UnityEngine.Rendering.StencilOp.Keep);
					mat.SetInt("_StencilReadMask", 255);
					mat.SetInt("_ColorMask", 15);

					mat.stencilSet = false;
				}
				return mat;
			}
			else if (grahpics.grayed)
			{
				if (grayedSharedMaterial == null)
				{
					grayedSharedMaterial = CreateMaterial();
					grayedSharedMaterial.EnableKeyword("GRAYED");
				}
				return grayedSharedMaterial;
			}
			else
			{
				return sharedMaterial;
			}
		}

		public NMaterial CreateMaterial()
		{
			Shader shader = ShaderConfig.Get(shaderName);
			if (shader == null)
			{
				Debug.LogWarning("FairyGUI: shader not found: " + shaderName);
				shader = Shader.Find("UI/Default");
			}
			NMaterial mat = new NMaterial(shader);
			mat.mainTexture = texture.nativeTexture;
			if (texture.alphaTexture != null)
			{
				mat.EnableKeyword("COMBINED");
				mat.SetTexture("_AlphaTex", texture.alphaTexture);
			}

			shader.hideFlags = DisplayOptions.hideFlags;
			mat.hideFlags = DisplayOptions.hideFlags;

			return mat;
		}

		public void Dispose()
		{
			if (Application.isPlaying)
			{
				if (sharedMaterial != null)
				{
					Material.Destroy(sharedMaterial);
					sharedMaterial = null;
				}

				if (grayedSharedMaterial != null)
				{
					Material.Destroy(grayedSharedMaterial);
					grayedSharedMaterial = null;
				}
			}
			else
			{
				if (sharedMaterial != null)
				{
					Material.DestroyImmediate(sharedMaterial);
					sharedMaterial = null;
				}

				if (grayedSharedMaterial != null)
				{
					Material.DestroyImmediate(grayedSharedMaterial);
					grayedSharedMaterial = null;
				}
			}

			foreach (MaterialPool pool in _pools)
				pool.Dispose();
		}
	}
}
