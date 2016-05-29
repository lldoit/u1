﻿using System;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public enum FitScreen
	{
		None,
		FitSize,
		FitWidthAndSetMiddle,
		FitHeightAndSetCenter
	}

	/// <summary>
	/// 
	/// </summary>
	[ExecuteInEditMode]
	[AddComponentMenu("FairyGUI/UI Panel")]
	public class UIPanel : MonoBehaviour
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
		public FitScreen fitScreen;

		/// <summary>
		/// 
		/// </summary>
		public int sortingOrder;

		[SerializeField]
		string packagePath;
		[SerializeField]
		RenderMode renderMode = RenderMode.ScreenSpaceOverlay;
		[SerializeField]
		Camera renderCamera = null;
		[SerializeField]
		Vector3 position;
		[SerializeField]
		Vector3 scale = new Vector3(1, 1, 1);
		[SerializeField]
		Vector3 rotation = new Vector3(0, 0, 0);
		[SerializeField]
		bool fairyBatching = false;
		[SerializeField]
		bool touchDisabled = false;
		[SerializeField]
		Vector2 cachedUISize;
		[SerializeField]
		HitTestMode hitTestMode = HitTestMode.Default;

		[System.NonSerialized]
		int screenSizeVer;
		[System.NonSerialized]
		Rect uiBounds; //Track bounds even when UI is not created, edit mode

		GComponent _ui;
		[NonSerialized]
		bool _created;

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
				//不在播放状态时我们不在OnEnable创建，因为Prefab也会调用OnEnable，延迟到Update里创建（Prefab不调用Update)
				//每次播放前都会disable/enable一次。。。
				if (container != null)//如果不为null，可能是因为Prefab revert， 而不是因为Assembly reload，
					OnDestroy();

				_renderTargets.Add(this);
				_renderTargetsChanged = true;
				screenSizeVer = 0;
				uiBounds.position = position;
				uiBounds.size = cachedUISize;
				if (uiBounds.size == Vector2.zero)
					uiBounds.size = new Vector2(30, 30);
			}
		}

		void OnDisable()
		{
			if (!Application.isPlaying)
			{
				_renderTargets.Remove(this);
			}
		}

		void Start()
		{
			if (!_created && Application.isPlaying)
				CreateUI_PlayMode();
		}

		void Update()
		{
			if (screenSizeVer != StageCamera.screenSizeVer)
				HandleScreenSizeChanged();
		}

		void OnDestroy()
		{
			if (container != null)
			{
				if (!Application.isPlaying)
					_renderTargets.Remove(this);

				if (_ui != null)
				{
					_ui.Dispose();
					_ui = null;
				}

				container.Dispose();
				container = null;
			}
		}

		void CreateContainer()
		{
			if (!Application.isPlaying)
			{
				Transform t = this.transform;
				int cnt = t.childCount;
				while (cnt > 0)
				{
					UnityEngine.Object.DestroyImmediate(t.GetChild(0).gameObject);
					cnt--;
				}
			}

			this.container = new Container(this.gameObject);
			this.container.renderMode = renderMode;
			this.container.renderCamera = renderCamera;
			this.container.touchable = !touchDisabled;
			this.container._panelOrder = sortingOrder;
			this.container.fairyBatching = fairyBatching;
			if (Application.isPlaying)
			{
				SetSortingOrder(this.sortingOrder, true);
				if (this.hitTestMode == HitTestMode.Raycast)
					this.container.hitArea = new BoxColliderHitTest(this.gameObject.AddComponent<BoxCollider>());
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public GComponent ui
		{
			get
			{
				if (!_created && Application.isPlaying)
					CreateUI_PlayMode();

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
				_ui.Dispose();
				_ui = null;
			}

			CreateUI_PlayMode();
		}

		/// <summary>
		/// Change the sorting order of the panel in runtime.
		/// </summary>
		/// <param name="value">sorting order value</param>
		/// <param name="apply">false if you dont want the default sorting behavior. e.g. call Stage.SortWorldSpacePanelsByZOrder later.</param>
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

		void CreateUI_PlayMode()
		{
			_created = true;

			if (string.IsNullOrEmpty(packageName) || string.IsNullOrEmpty(componentName))
				return;

			_ui = (GComponent)UIPackage.CreateObject(packageName, componentName);
			if (_ui != null)
			{
				_ui.position = position;
				if (scale.x != 0 && scale.y != 0)
					_ui.scale = scale;
				_ui.rotationX = rotation.x;
				_ui.rotationY = rotation.y;
				_ui.rotation = rotation.z;
				if (this.container.hitArea != null)
				{
					UpdateHitArea();
					_ui.onSizeChanged.Add(UpdateHitArea);
					_ui.onPositionChanged.Add(UpdateHitArea);
				}
				this.container.AddChild(_ui.displayObject);

				HandleScreenSizeChanged();
			}
			else
				Debug.LogError("Create " + componentName + "@" + packageName + " failed!");
		}

		void UpdateHitArea()
		{
			((BoxColliderHitTest)this.container.hitArea).SetArea(_ui.x, _ui.y, _ui.width, _ui.height);
		}

		void CreateUI_EditMode()
		{
			if (!packageListReady || UIPackage.GetByName(packageName) == null)
				return;

#if UNITY_5
			UIObjectFactory.packageItemExtensions.Clear();
			UIObjectFactory.loaderConstructor = null;
			DisplayOptions.SetEditModeHideFlags();
			DisplayOptions.defaultRoot = new Transform[] { this.transform };
			_ui = (GComponent)UIPackage.CreateObject(packageName, componentName);
			DisplayOptions.defaultRoot = null;

			if (_ui != null)
			{
				_ui.position = position;
				if (scale.x != 0 && scale.y != 0)
					_ui.scale = scale;
				_ui.rotationX = rotation.x;
				_ui.rotationY = rotation.y;
				_ui.rotation = rotation.z;
				this.container.AddChild(_ui.displayObject);

				cachedUISize = _ui.size;
				uiBounds.size = cachedUISize;
				HandleScreenSizeChanged();
			}
#else
			PackageItem pi = UIPackage.GetByName(packageName).GetItemByName(componentName);
			if (pi != null)
			{
				cachedUISize = new Vector2(pi.width, pi.height);
				uiBounds.size = cachedUISize;
				HandleScreenSizeChanged();
			}
#endif
		}

		void HandleScreenSizeChanged()
		{
			screenSizeVer = StageCamera.screenSizeVer;

			if (this.container != null)
			{
				if (this.container.renderMode != RenderMode.WorldSpace)
					this.container.scale = new Vector2(StageCamera.UnitsPerPixel * UIContentScaler.scaleFactor, StageCamera.UnitsPerPixel * UIContentScaler.scaleFactor);
			}

			float width = Screen.width / UIContentScaler.scaleFactor;
			float height = Screen.height / UIContentScaler.scaleFactor;
			if (this._ui != null)
			{
				switch (fitScreen)
				{
					case FitScreen.FitSize:
						this._ui.SetXY(0, 0);
						this._ui.SetSize(width, height);
						break;

					case FitScreen.FitWidthAndSetMiddle:
						this._ui.SetXY(0, (height - this._ui.height) / 2);
						this._ui.SetSize(width, this._ui.sourceHeight);
						break;

					case FitScreen.FitHeightAndSetCenter:
						this._ui.SetXY((width - this._ui.width) / 2, 0);
						this._ui.SetSize(this._ui.sourceWidth, height);
						break;
				}
			}
			else
			{
				switch (fitScreen)
				{
					case FitScreen.FitSize:
						uiBounds.position = new Vector2(0, 0);
						uiBounds.size = new Vector2(width, height);
						break;

					case FitScreen.FitWidthAndSetMiddle:
						uiBounds.position = new Vector2(0, (height - cachedUISize.y) / 2);
						uiBounds.size = new Vector2(width, cachedUISize.y);
						break;

					case FitScreen.FitHeightAndSetCenter:
						uiBounds.position = new Vector2((width - cachedUISize.x) / 2, 0);
						uiBounds.size = new Vector2(cachedUISize.x, height);
						break;
				}
			}
		}

		#region edit mode functions

		void OnUpdateSource(object[] data)
		{
			if (Application.isPlaying)
				return;

			this.packageName = (string)data[0];
			this.packagePath = (string)data[1];
			this.componentName = (string)data[2];

			if ((bool)data[3])
			{
				if (container == null)
					return;

				if (_ui != null)
				{
					_ui.Dispose();
					_ui = null;
				}
			}
		}

		void ApplyModifiedProperties()
		{
			if (container != null)
			{
				container.renderMode = renderMode;
				container.renderCamera = renderCamera;
				if (container._panelOrder != sortingOrder)
				{
					container._panelOrder = sortingOrder;
					_renderTargetsChanged = true;
				}
				container.fairyBatching = fairyBatching;
			}

			if (_ui != null)
			{
				if (fitScreen == FitScreen.None)
					_ui.position = position;
				if (scale.x != 0 && scale.y != 0)
					_ui.scale = scale;
				_ui.rotationX = rotation.x;
				_ui.rotationY = rotation.y;
				_ui.rotation = rotation.z;
			}
			if (fitScreen == FitScreen.None)
				uiBounds.position = position;
			screenSizeVer = 0;//force HandleScreenSizeChanged be called
		}

		void ApplyFitSceenChanged()
		{
			if (this.fitScreen == FitScreen.None)
			{
				if (this._ui != null)
				{
					this._ui.position = position;
					this._ui.SetSize(this._ui.sourceWidth, this._ui.sourceHeight);
				}
				uiBounds.position = position;
				uiBounds.size = cachedUISize;
			}
			else
				screenSizeVer = 0; //force HandleScreenSizeChanged be called
		}

		public void MoveUI(Vector3 delta)
		{
			if (fitScreen != FitScreen.None)
				return;

			this.position += delta;
			if (_ui != null)
				_ui.position = position;
			uiBounds.position = position;
		}

		public Vector3 GetUIWorldPosition()
		{
			if (_ui != null)
				return _ui.displayObject.cachedTransform.position;
			else
				return this.container.cachedTransform.TransformPoint(uiBounds.position);
		}

		void OnDrawGizmos()
		{
			if (Application.isPlaying || this.container == null)
				return;

			Vector3 pos, size;
			if (this._ui != null)
			{
				Gizmos.matrix = this._ui.displayObject.cachedTransform.localToWorldMatrix;
				pos = new Vector3(this._ui.width / 2, -this._ui.height / 2, 0);
				size = new Vector3(this._ui.width, this._ui.height, 0);
			}
			else
			{
				Gizmos.matrix = this.container.cachedTransform.localToWorldMatrix;
				pos = new Vector3(uiBounds.x + uiBounds.width / 2, -uiBounds.y - uiBounds.height / 2, 0);
				size = new Vector3(uiBounds.width, uiBounds.height, 0);
			}

			Gizmos.color = new Color(0, 0, 0, 0);
			Gizmos.DrawCube(pos, size);

			Gizmos.color = Color.white;
			Gizmos.DrawWireCube(pos, size);
		}

		void PreRender()
		{
			if (container == null)
				CreateContainer();

			if (packageName != null && componentName != null && _ui == null)
				CreateUI_EditMode();

			if (screenSizeVer != StageCamera.screenSizeVer)
				HandleScreenSizeChanged();
		}

		static UpdateContext _updateContext;
		static List<UIPanel> _renderTargets = new List<UIPanel>();
		static bool _renderTargetsChanged;
		public static bool packageListReady;

		public static int activePanelCount
		{
			get { return _renderTargets.Count; }
		}

		public static void RenderAllPanels()
		{
			if (Application.isPlaying)
				return;

			if (_updateContext == null)
				_updateContext = new UpdateContext();

			if (_renderTargetsChanged)
			{
				_renderTargets.Sort(CompareDepth);
				_renderTargetsChanged = false;
			}

			int cnt = _renderTargets.Count;
			for (int i = 0; i < cnt; i++)
			{
				UIPanel panel = _renderTargets[i];
				panel.PreRender();
			}

			if (packageListReady)
			{
				_updateContext.Begin();
				for (int i = 0; i < cnt; i++)
					_renderTargets[i].container.Update(_updateContext);
				_updateContext.End();
			}
		}

		static int CompareDepth(UIPanel c1, UIPanel c2)
		{
			return c1.sortingOrder - c2.sortingOrder;
		}

		public static void ReloadAllPanels()
		{
			if (Application.isPlaying)
				return;

			int cnt = _renderTargets.Count;
			for (int i = 0; i < cnt; i++)
			{
				UIPanel panel = _renderTargets[i];
				if (panel._ui != null)
				{
					panel._ui.Dispose();
					panel._ui = null;
				}
			}
		}

		#endregion
	}
}
