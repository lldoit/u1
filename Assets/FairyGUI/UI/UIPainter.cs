using System;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	[ExecuteInEditMode]
	[AddComponentMenu("FairyGUI/UI Painter")]
	[RequireComponent(typeof(MeshCollider), typeof(MeshRenderer))]
	public class UIPainter : MonoBehaviour
	{
		/// <summary>
		/// 
		/// </summary>
		public Container container { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public string packageName;

		/// <summary>
		/// 
		/// </summary>
		public string componentName;

		/// <summary>
		/// 
		/// </summary>
		public int sortingOrder;

		[SerializeField]
		string packagePath;
		[SerializeField]
		Camera renderCamera = null;
		[SerializeField]
		bool fairyBatching = false;
		[SerializeField]
		bool touchDisabled = false;

		GComponent _ui;
		[NonSerialized]
		bool _created;
		[NonSerialized]
		bool _captured;
		[NonSerialized]
		Renderer _renderer;

		public RenderTexture texture { get; private set; }

		void OnEnable()
		{
			if (Application.isPlaying)
			{
				CreateContainer();

				if (!string.IsNullOrEmpty(packagePath) && UIPackage.GetByName(packageName) == null)
					UIPackage.AddPackage(packagePath);
			}
			else
			{
				if (!_renderTargets.Contains(this))
					_renderTargets.Add(this);
			}
		}

		void Update()
		{
			if (!Application.isPlaying)
			{
				if (_renderer == null)
					_renderer = this.GetComponent<Renderer>();
				if (_renderer != null && _renderer.sharedMaterial.mainTexture != texture)
					_renderer.sharedMaterial.mainTexture = texture;
			}
		}

		void OnDisable()
		{
			if (!Application.isPlaying)
				_renderTargets.Remove(this);
		}

		void OnGUI()
		{
			if (!Application.isPlaying)
			{
				if (packageName != null && componentName != null && !_captured)
					CaptureInEditMode();
			}
		}

		void Start()
		{
			if (!_created && Application.isPlaying)
				CreateUI();
		}

		void OnDestroy()
		{
			if (Application.isPlaying)
			{
				if (_ui != null)
				{
					_ui.Dispose();
					_ui = null;
				}

				container.Dispose();
				container = null;
			}
			else
				_renderTargets.Remove(this);

			DestroyTexture();
		}

		void CreateContainer()
		{
			this.container = new Container("UIPainter");
			this.container.renderMode = RenderMode.WorldSpace;
			this.container.renderCamera = renderCamera;
			this.container.touchable = !touchDisabled;
			this.container.fairyBatching = fairyBatching;
			this.container._panelOrder = sortingOrder;
			this.container.hitArea = new MeshColliderHitTest(this.gameObject.GetComponent<MeshCollider>());
			this.container._onUpdate = () =>
			{
				UpdateContext.OnEnd += Capture;
			};
			if (Application.isPlaying)
				SetSortingOrder(this.sortingOrder, true);

			this.container.layer = CaptureCamera.hiddenLayer;
		}

		/// <summary>
		/// Change the sorting order of the panel in runtime.
		/// </summary>
		/// <param name="value">sorting order value</param>
		/// <param name="apply">false if you dont want the default sorting behavior.</param>
		public void SetSortingOrder(int value, bool apply)
		{
			this.sortingOrder = value;
			container._panelOrder = value;

			if (!apply)
				return;

			int numChildren = Stage.inst.numChildren;
			int i = 0;
			int j;
			int curIndex = -1;
			for (; i < numChildren; i++)
			{
				DisplayObject obj = Stage.inst.GetChildAt(i);
				if (obj == this.container)
				{
					curIndex = i;
					continue;
				}

				if (obj == GRoot.inst.displayObject)
					j = 1000;
				else if (obj is Container)
					j = ((Container)obj)._panelOrder;
				else
					continue;

				if (sortingOrder <= j)
				{
					if (curIndex != -1)
						Stage.inst.AddChildAt(this.container, i - 1);
					else
						Stage.inst.AddChildAt(this.container, i);
					break;
				}
			}
			if (i == numChildren)
				Stage.inst.AddChild(this.container);
		}

		void Capture()
		{
			if (this._ui != null)
				CaptureCamera.Capture(this.container, this.texture);
		}

		/// <summary>
		/// 
		/// </summary>
		public GComponent ui
		{
			get
			{
				if (!_created && Application.isPlaying)
					CreateUI();

				return _ui;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void CreateUI()
		{
			if (_ui != null)
			{
				DestroyTexture();
				_ui.Dispose();
				_ui = null;
			}

			_created = true;

			if (string.IsNullOrEmpty(packageName) || string.IsNullOrEmpty(componentName))
				return;

			_ui = (GComponent)UIPackage.CreateObject(packageName, componentName);
			if (_ui != null)
			{
				this.container.AddChild(_ui.displayObject);
				((MeshColliderHitTest)this.container.hitArea).SetArea(0, 0, _ui.width, _ui.height);
				CreateTexture(Mathf.CeilToInt(_ui.width), Mathf.CeilToInt(_ui.height));
				this.GetComponent<Renderer>().sharedMaterial.mainTexture = texture;
			}
			else
				Debug.LogError("Create " + componentName + "@" + packageName + " failed!");
		}

		void CreateTexture(int width, int height)
		{
			texture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
			texture.hideFlags = DisplayOptions.hideFlags;
			texture.antiAliasing = 1;
			texture.filterMode = FilterMode.Bilinear;
			texture.anisoLevel = 0;
			texture.useMipMap = false;
			texture.wrapMode = TextureWrapMode.Clamp;
		}

		void DestroyTexture()
		{
			if (texture != null)
			{
				if (Application.isPlaying)
					RenderTexture.Destroy(texture);
				else
					RenderTexture.DestroyImmediate(texture);
				texture = null;
			}
		}

		#region edit mode functions
		public static bool packageListReady;
		public static List<UIPainter> _renderTargets = new List<UIPainter>();

		public static int activePanelCount
		{
			get { return _renderTargets.Count; }
		}

		void CaptureInEditMode()
		{
			if (!packageListReady || UIPackage.GetByName(packageName) == null)
				return;

			_captured = true;
			GameObject tempGo = new GameObject("Temp Object");
			tempGo.layer = CaptureCamera.layer;

			UIObjectFactory.packageItemExtensions.Clear();
			UIObjectFactory.loaderConstructor = null;
			DisplayOptions.SetEditModeHideFlags();
			DisplayOptions.defaultRoot = new Transform[] { tempGo.transform };
			GComponent view = (GComponent)UIPackage.CreateObject(packageName, componentName);
			DisplayOptions.defaultRoot = null;

			if (view != null)
			{
				if (texture != null)
					DestroyTexture();

				CreateTexture(Mathf.CeilToInt(view.width), Mathf.CeilToInt(view.height));

				Container root = (Container)view.displayObject;
				root.layer = CaptureCamera.layer;
				root.SetChildrenLayer(CaptureCamera.layer);
				root.gameObject.hideFlags = HideFlags.None;
				root.gameObject.SetActive(true);

				GameObject cameraObject = new GameObject("Temp Capture Camera");
				Camera camera = cameraObject.AddComponent<Camera>();
				camera.depth = 0;
				camera.cullingMask = 1 << CaptureCamera.layer;
				camera.clearFlags = CameraClearFlags.Depth;
				camera.orthographic = true;
				camera.orthographicSize = view.height / 2;
				camera.nearClipPlane = -30;
				camera.farClipPlane = 30;
				camera.enabled = false;
				camera.targetTexture = texture;

				Vector3 pos = root.cachedTransform.position;
				pos.x += camera.orthographicSize * camera.aspect;
				pos.y -= camera.orthographicSize;
				pos.z = 0;
				cameraObject.transform.localPosition = pos;

				UpdateContext context = new UpdateContext();
				//run two times
				context.Begin();
				view.displayObject.Update(context);
				context.End();

				context.Begin();
				view.displayObject.Update(context);
				context.End();

				RenderTexture old = RenderTexture.active;
				RenderTexture.active = texture;
				GL.Clear(true, true, Color.clear);
				camera.Render();
				RenderTexture.active = old;

				camera.targetTexture = null;
				view.Dispose();
				GameObject.DestroyImmediate(cameraObject);
				GameObject.DestroyImmediate(tempGo);

				this.GetComponent<Renderer>().sharedMaterial.mainTexture = texture;
			}
		}

		void OnUpdateSource(object[] data)
		{
			if (Application.isPlaying)
				return;

			this.packageName = (string)data[0];
			this.packagePath = (string)data[1];
			this.componentName = (string)data[2];

			if ((bool)data[3])
				_captured = false;
		}

		public static void ReloadAllPanels()
		{
			if (Application.isPlaying)
				return;

			int cnt = _renderTargets.Count;
			for (int i = 0; i < cnt; i++)
			{
				UIPainter panel = _renderTargets[i];
				panel._captured = false;
			}
		}

		#endregion
	}
}
