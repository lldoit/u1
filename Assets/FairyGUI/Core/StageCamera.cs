﻿using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// Stage Camera is an orthographic camera for UI rendering.
	/// </summary>
	[ExecuteInEditMode]
	[AddComponentMenu("FairyGUI/UI Camera")]
	public class StageCamera : MonoBehaviour
	{
		[System.NonSerialized]
		public Transform cachedTransform;
		[System.NonSerialized]
		public Camera cachedCamera;

		[System.NonSerialized]
		int screenWidth;
		[System.NonSerialized]
		int screenHeight;
		[System.NonSerialized]
		bool isMain;

		[System.NonSerialized]
		public static Camera main;

		[System.NonSerialized]
		public static int screenSizeVer = 1;

		public const string Name = "Stage Camera";
		public const string LayerName = "UI";

		public const float UnitsPerPixel = 0.02f;

		void OnEnable()
		{
			cachedTransform = this.transform;
			cachedCamera = this.GetComponent<Camera>();
			if (this.gameObject.name == Name)
			{
				main = cachedCamera;
				isMain = true;
			}
			OnScreenSizeChanged();
		}

		void Update()
		{
			if (screenWidth != Screen.width || screenHeight != Screen.height)
				OnScreenSizeChanged();
			
		}

		void OnScreenSizeChanged()
		{
			screenWidth = Screen.width;
			screenHeight = Screen.height;
			if (screenWidth == 0 || screenHeight == 0)
				return;

			cachedCamera.orthographicSize = screenHeight / 2 * UnitsPerPixel;
			cachedCamera.aspect = (float)screenWidth / screenHeight;
			cachedTransform.localPosition = new Vector3(cachedCamera.orthographicSize * cachedCamera.aspect, -cachedCamera.orthographicSize);

			if (isMain)
			{
				screenSizeVer++;
				if (Application.isPlaying)
					Stage.inst.HandleScreenSizeChanged();
				else
				{
					UIContentScaler scaler = GameObject.FindObjectOfType<UIContentScaler>();
					if (scaler != null)
						scaler.ApplyChange();
					else
						UIContentScaler.scaleFactor = 1;
				}
			}
		}

		void OnRenderObject()
		{
			//call only edit mode
			if (isMain && !Application.isPlaying)
			{
				UIPanel.RenderAllPanels();
			}
		}

		/// <summary>
		/// Check if there is a stage camera in the scene. If none, create one.
		/// </summary>
		public static void CheckMainCamera()
		{
			if (GameObject.Find(Name) == null)
			{
				int layer = LayerMask.NameToLayer(LayerName);
				CreateCamera(Name, 1 << layer);
			}
		}

		public static void CheckCaptureCamera()
		{
			if (GameObject.Find(Name) == null)
			{
				int layer = LayerMask.NameToLayer(LayerName);
				CreateCamera(Name, 1 << layer);
			}
		}

		public static Camera CreateCamera(string name, int cullingMask)
		{
			GameObject cameraObject = new GameObject(name);
			Camera camera = cameraObject.AddComponent<Camera>();
			camera.depth = 1;
			camera.cullingMask = cullingMask;
			camera.clearFlags = CameraClearFlags.Depth;
			camera.orthographic = true;
			camera.orthographicSize = 5;
			camera.nearClipPlane = -30;
			camera.farClipPlane = 30;

			cameraObject.AddComponent<StageCamera>();

			return camera;
		}
	}
}
