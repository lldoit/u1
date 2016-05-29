using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	public class CaptureCamera : MonoBehaviour
	{
		[System.NonSerialized]
		public Transform cachedTransform;
		[System.NonSerialized]
		public Camera cachedCamera;

		[System.NonSerialized]
		static CaptureCamera _main;

		[System.NonSerialized]
		static int _layer = -1;
		static int _hiddenLayer = -1;

		public const string Name = "Capture Camera";
		public const string LayerName = "VUI";
		public const string HiddenLayerName = "Hidden VUI";

		void OnEnable()
		{
			cachedCamera = this.GetComponent<Camera>();
			cachedTransform = this.gameObject.transform;

			if (this.gameObject.name == Name)
				_main = this;
		}

		public static void CheckMain()
		{
			if (_main != null && _main.cachedCamera != null)
				return;

			GameObject go = GameObject.Find(Name);
			if (go != null)
			{
				_main = go.GetComponent<CaptureCamera>();
				return;
			}

			GameObject cameraObject = new GameObject(Name);
			Camera camera = cameraObject.AddComponent<Camera>();
			camera.depth = 0;
			camera.cullingMask = 1 << layer;
			camera.clearFlags = CameraClearFlags.Depth;
			camera.orthographic = true;
			camera.orthographicSize = 5;
			camera.nearClipPlane = -30;
			camera.farClipPlane = 30;
			camera.enabled = false;
			cameraObject.AddComponent<CaptureCamera>();
		}

		public static int layer
		{
			get
			{
				if (_layer == -1)
				{
					_layer = LayerMask.NameToLayer(LayerName);
					if (_layer == -1)
					{
						_layer = 30;
						Debug.LogWarning("Please define two layers named '" + CaptureCamera.LayerName + "' and '" + CaptureCamera.HiddenLayerName + "'");
					}
				}

				return _layer;
			}
		}

		public static int hiddenLayer
		{
			get
			{
				if (_hiddenLayer == -1)
				{
					_hiddenLayer = LayerMask.NameToLayer(HiddenLayerName);
					if (_hiddenLayer == -1)
					{
						Debug.LogWarning("Please define two layers named '" + CaptureCamera.LayerName + "' and '" + CaptureCamera.HiddenLayerName + "'");
						_hiddenLayer = 31;
					}
				}

				return _hiddenLayer;
			}
		}

		public static RenderTexture CreateRenderTexture(float width, float height)
		{
			RenderTexture texture = new RenderTexture(Mathf.RoundToInt(width), Mathf.RoundToInt(height), 0, RenderTextureFormat.ARGB32);
			texture.antiAliasing = 1;
			texture.filterMode = FilterMode.Bilinear;
			texture.anisoLevel = 0;
			texture.useMipMap = false;
			texture.wrapMode = TextureWrapMode.Clamp;
			return texture;
		}

		public static void Capture(Container container, RenderTexture texture)
		{
			CheckMain();

			Camera camera = _main.cachedCamera;
			camera.targetTexture = texture;
			camera.orthographicSize = texture.height / 2 * StageCamera.UnitsPerPixel;
			Vector3 v = container.cachedTransform.position;
			v.x += camera.orthographicSize * camera.aspect;
			v.y -= camera.orthographicSize;
			_main.cachedTransform.localPosition = v;

			int oldLayer = container.layer;
			container.layer = _layer;
			container.SetChildrenLayer(_layer);

			RenderTexture old = RenderTexture.active;
			RenderTexture.active = texture;
			GL.Clear(true, true, Color.clear);
			camera.Render();
			RenderTexture.active = old;

			container.layer = oldLayer;
			container.SetChildrenLayer(oldLayer);
		}
	}
}
