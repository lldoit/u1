﻿using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class DisplayObject : EventDispatcher
	{
		public string name;
		public Container parent { get; internal set; }
		public GameObject gameObject { get; protected set; }
		public Transform cachedTransform { get; protected set; }
		public NGraphics graphics { get; protected set; }

		public NGraphics paintingGraphics { get; protected set; }
		public EventCallback0 onPaint;

		public GObject gOwner;
		public uint id;

		public EventListener onClick { get; private set; }
		public EventListener onRightClick { get; private set; }
		public EventListener onTouchBegin { get; private set; }
		public EventListener onTouchEnd { get; private set; }
		public EventListener onRollOver { get; private set; }
		public EventListener onRollOut { get; private set; }
		public EventListener onMouseWheel { get; private set; }
		public EventListener onAddedToStage { get; private set; }
		public EventListener onRemovedFromStage { get; private set; }
		public EventListener onKeyDown { get; private set; }
		public EventListener onClickLink { get; private set; }

		bool _visible;
		bool _touchable;
		Vector2 _pivot;
		Vector3 _pivotOffset;
		Vector2 _skew;
		int _renderingOrder;
		float _alpha;
		bool _grayed;
		BlendMode _blendMode;
		IFilter _filter;
		bool _disposed;

		bool _perspective;
		int _focalLength;
		Vector3 _rotation;

		protected EventCallback0 _captureDelegate; //缓存这个delegate，可以防止Capture状态下每帧104B的GC
		//painting mode
		protected int _paintingMode; //1-滤镜，2-blendMode，3-transformMatrix
		protected Margin _paintingMargin;
		protected int _paintingFlag;

		protected Rect _contentRect;
		protected Vector2 _positionOffset;
		protected bool _requireUpdateMesh;
		protected Matrix4x4? _transformMatrix;

		internal protected bool _optimizeNotTouchable;
		internal Rect _internal_bounds;
		internal protected bool _skipInFairyBatching;

		internal static uint _gInstanceCounter;

		public DisplayObject()
		{
			_alpha = 1;
			_visible = true;
			_touchable = true;
			id = _gInstanceCounter++;
			_blendMode = BlendMode.Normal;
			_focalLength = 2000;
			_captureDelegate = Capture;

			onClick = new EventListener(this, "onClick");
			onRightClick = new EventListener(this, "onRightClick");
			onTouchBegin = new EventListener(this, "onTouchBegin");
			onTouchEnd = new EventListener(this, "onTouchEnd");
			onRollOver = new EventListener(this, "onRollOver");
			onRollOut = new EventListener(this, "onRollOut");
			onMouseWheel = new EventListener(this, "onMouseWheel");
			onAddedToStage = new EventListener(this, "onAddedToStage");
			onRemovedFromStage = new EventListener(this, "onRemovedFromStage");
			onKeyDown = new EventListener(this, "onKeyDown");
			onClickLink = new EventListener(this, "onClickLink");
		}

		virtual protected void CreateGameObject(string gameObjectName)
		{
			gameObject = new GameObject(gameObjectName);
			cachedTransform = gameObject.transform;
			if (DisplayOptions.defaultRoot != null)
				FairyGUI.Utils.ToolSet.SetParent(cachedTransform, DisplayOptions.defaultRoot[0]);
			else
				Object.DontDestroyOnLoad(gameObject);
			gameObject.hideFlags = DisplayOptions.hideFlags | HideFlags.HideInHierarchy;
			gameObject.SetActive(false);
		}

		virtual protected void DestroyGameObject()
		{
			if (gameObject != null)
			{
				if (Application.isPlaying)
					GameObject.Destroy(gameObject);
				else
					GameObject.DestroyImmediate(gameObject);
				gameObject = null;
				cachedTransform = null;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float alpha
		{
			get { return _alpha; }
			set { _alpha = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool grayed
		{
			get { return _grayed; }
			set { _grayed = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool visible
		{
			get { return _visible; }
			set
			{
				if (_visible != value)
				{
					_visible = value;
					SetGO_Visible();

					if (_visible)
						this.InvalidateBatchingState();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float x
		{
			get { return cachedTransform.localPosition.x + _positionOffset.x; }
			set
			{
				Vector3 v = cachedTransform.localPosition;
				v.x = value - _positionOffset.x;
				cachedTransform.localPosition = v;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float y
		{
			get { return -cachedTransform.localPosition.y + _positionOffset.y; }
			set
			{
				Vector3 v = cachedTransform.localPosition;
				v.y = -value + _positionOffset.y;
				cachedTransform.localPosition = v;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float z
		{
			get { return cachedTransform.localPosition.z; }
			set
			{
				Vector3 v = cachedTransform.localPosition;
				v.z = value;
				cachedTransform.localPosition = v;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector2 xy
		{
			get { return new Vector2(this.x, this.y); }
			set { SetPosition(value.x, value.y, cachedTransform.localPosition.z); }
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector3 position
		{
			get { return new Vector3(this.x, this.y, this.z); }
			set { SetPosition(value.x, value.y, value.z); }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="xv"></param>
		/// <param name="yv"></param>
		public void SetXY(float xv, float yv)
		{
			SetPosition(xv, yv, cachedTransform.localPosition.z);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="xv"></param>
		/// <param name="yv"></param>
		/// <param name="zv"></param>
		public void SetPosition(float xv, float yv, float zv)
		{
			Vector3 v = cachedTransform.localPosition;
			v.x = xv - _positionOffset.x;
			v.y = -yv + _positionOffset.y;
			v.z = zv;
			cachedTransform.localPosition = v;
		}

		protected void SetPositionOffset(Vector2 value)
		{
			Vector3 v = cachedTransform.localPosition;
			v.x += (_positionOffset.x - value.x);
			v.y += value.y - _positionOffset.y;
			cachedTransform.localPosition = v;
			_positionOffset = value;
		}

		/// <summary>
		/// 
		/// </summary>
		public float width
		{
			get { return _contentRect.width; }
			set
			{
				if (!Mathf.Approximately(value, _contentRect.width))
				{
					_contentRect.width = value;
					OnSizeChanged();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float height
		{
			get { return _contentRect.height; }
			set
			{
				if (!Mathf.Approximately(value, _contentRect.height))
				{
					_contentRect.height = value;
					OnSizeChanged();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector2 size
		{
			get { return _contentRect.size; }
			set { SetSize(value.x, value.y); }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="wv"></param>
		/// <param name="hv"></param>
		public void SetSize(float wv, float hv)
		{
			if (!Mathf.Approximately(wv, _contentRect.width)
				|| !Mathf.Approximately(hv, _contentRect.height))
			{
				_contentRect.width = wv;
				_contentRect.height = hv;
				OnSizeChanged();
			}
		}

		virtual protected void OnSizeChanged()
		{
			ApplyPivot();
			_paintingFlag = 1;
			if (graphics != null)
				_requireUpdateMesh = true;
		}

		/// <summary>
		/// 
		/// </summary>
		public float scaleX
		{
			get { return cachedTransform.localScale.x; }
			set
			{
				Vector3 v = cachedTransform.localScale;
				v.x = value;
				v.z = value;
				cachedTransform.localScale = v;
				ApplyPivot();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float scaleY
		{
			get { return cachedTransform.localScale.y; }
			set
			{
				Vector3 v = cachedTransform.localScale;
				v.y = value;
				cachedTransform.localScale = v;
				ApplyPivot();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="xv"></param>
		/// <param name="yv"></param>
		public void SetScale(float xv, float yv)
		{
			Vector3 v = cachedTransform.localScale;
			v.x = xv;
			v.y = yv;
			v.z = xv;
			cachedTransform.localScale = v;
			ApplyPivot();
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector2 scale
		{
			get { return cachedTransform.localScale; }
			set
			{
				SetScale(value.x, value.y);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float rotation
		{
			get
			{
				//和Unity默认的旋转方向相反
				if (_perspective)
					return -_rotation.z;
				else
					return -cachedTransform.localEulerAngles.z;
			}
			set
			{
				if (_perspective)
				{
					_rotation.z = -value;
					UpdateTransformMatrix();
				}
				else
				{
					Vector3 v = cachedTransform.localEulerAngles;
					v.z = -value;
					cachedTransform.localEulerAngles = v;
					ApplyPivot();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float rotationX
		{
			get
			{
				if (_perspective)
					return _rotation.x;
				else
					return cachedTransform.localEulerAngles.x;
			}
			set
			{
				if (_perspective)
				{
					_rotation.x = value;
					UpdateTransformMatrix();
				}
				else
				{
					Vector3 v = cachedTransform.localEulerAngles;
					v.x = value;
					cachedTransform.localEulerAngles = v;
					ApplyPivot();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float rotationY
		{
			get
			{
				if (_perspective)
					return _rotation.y;
				else
					return cachedTransform.localEulerAngles.y;
			}
			set
			{
				if (_perspective)
				{
					_rotation.y = value;
					UpdateTransformMatrix();
				}
				else
				{
					Vector3 v = cachedTransform.localEulerAngles;
					v.y = value;
					cachedTransform.localEulerAngles = v;
					ApplyPivot();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector2 skew
		{
			get { return _skew; }
			set
			{
				_skew = value;
				UpdateTransformMatrix();
			}
		}

		/// <summary>
		/// 当对象处于ScreenSpace，也就是使用正交相机渲染时，对象虽然可以绕X轴或者Y轴旋转，但没有透视效果。设置perspective，可以模拟出透视效果。
		/// </summary>
		public bool perspective
		{
			get
			{
				return _perspective;
			}
			set
			{
				if (_perspective != value)
				{
					_perspective = value;
					if (_perspective)
					{
						//屏蔽Unity自身的旋转变换
						_rotation = cachedTransform.localEulerAngles;
						cachedTransform.localEulerAngles = Vector3.zero;
					}
					else
						cachedTransform.localEulerAngles = _rotation;

					ApplyPivot();
					UpdateTransformMatrix();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int focalLength
		{
			get { return _focalLength; }
			set
			{
				if (value <= 0)
					value = 1;

				_focalLength = value;
				if (_transformMatrix != null)
					UpdateTransformMatrix();
			}
		}

		void UpdateTransformMatrix()
		{
			Matrix4x4 matrix = Matrix4x4.identity;
			if (_skew.x != 0 || _skew.y != 0)
				ToolSet.SkewMatrix(ref matrix, _skew.x, _skew.y);
			if (_perspective)
				matrix *= Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(_rotation), Vector3.one);
			Vector3 camPos = Vector3.zero;
			if (matrix.isIdentity)
				_transformMatrix = null;
			else
			{
				_transformMatrix = matrix;
				camPos = new Vector3(_pivot.x * _contentRect.width, -_pivot.y * _contentRect.height, _focalLength);
			}

			//组件的transformMatrix是通过paintingMode实现的，因为全部通过矩阵变换的话，和unity自身的变换混杂在一起，无力理清。
			if (_transformMatrix != null)
			{
				if (this is Container)
					this.EnterPaintingMode(3, null);
			}
			else
			{
				if (this is Container)
					this.LeavePaintingMode(3);
			}

			if (this._paintingMode > 0)
			{
				this.paintingGraphics.cameraPosition = camPos;
				this.paintingGraphics.vertexMatrix = _transformMatrix;
				this._paintingFlag = 1;
			}
			else if (this.graphics != null)
			{
				this.graphics.cameraPosition = camPos;
				this.graphics.vertexMatrix = _transformMatrix;
				_requireUpdateMesh = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector2 pivot
		{
			get { return _pivot; }
			set
			{
				Vector3 deltaPivot = new Vector2((value.x - _pivot.x) * _contentRect.width, (_pivot.y - value.y) * _contentRect.height);
				Vector3 oldOffset = _pivotOffset;

				_pivot = value;
				UpdatePivotOffset();
				Vector3 v = cachedTransform.localPosition;
				v += oldOffset - _pivotOffset + deltaPivot;
				cachedTransform.localPosition = v;

				if (_transformMatrix != null)
					UpdateTransformMatrix();
			}
		}

		void UpdatePivotOffset()
		{
			float px = _pivot.x * _contentRect.width;
			float py = _pivot.y * _contentRect.height;

			//注意这里不用处理skew，因为在顶点变换里有对pivot的处理
			Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, cachedTransform.localRotation, cachedTransform.localScale);
			_pivotOffset = matrix.MultiplyPoint(new Vector3(px, -py, 0));
		}

		void ApplyPivot()
		{
			if (_pivot.x != 0 || _pivot.y != 0)
			{
				Vector3 oldOffset = _pivotOffset;

				UpdatePivotOffset();
				Vector3 v = cachedTransform.localPosition;
				v += oldOffset - _pivotOffset;
				cachedTransform.localPosition = v;
			}
		}

		/// <summary>
		/// This is the pivot position
		/// </summary>
		public Vector3 location
		{
			get
			{
				Vector3 pos = this.position;
				pos.x += _pivotOffset.x;
				pos.y -= _pivotOffset.y;
				pos.z += _pivotOffset.z;
				return pos;
			}

			set
			{
				this.SetPosition(value.x - _pivotOffset.x, value.y + _pivotOffset.y, value.z - _pivotOffset.z);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		virtual public Material material
		{
			get
			{
				if (graphics != null)
					return graphics.material;
				else
					return null;
			}
			set
			{
				if (graphics != null)
					graphics.material = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		virtual public string shader
		{
			get
			{
				if (graphics != null)
					return graphics.shader;
				else
					return null;
			}
			set
			{
				if (graphics != null)
					graphics.shader = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		virtual public int renderingOrder
		{
			get
			{
				return _renderingOrder;
			}
			set
			{
				_renderingOrder = value;
				if (graphics != null)
					graphics.sortingOrder = value;
				if (_paintingMode > 0)
					paintingGraphics.sortingOrder = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		virtual public int layer
		{
			get
			{
				if (_paintingMode > 0)
					return paintingGraphics.gameObject.layer;
				else
					return gameObject.layer;
			}
			set
			{
				if (_paintingMode > 0)
					paintingGraphics.gameObject.layer = value;
				else
					gameObject.layer = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool isDisposed
		{
			get { return gameObject == null; }
		}

		protected internal void SetParent(Container value)
		{
			if (parent != value)
			{
				parent = value;
				SetGO_Visible();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Container topmost
		{
			get
			{
				DisplayObject currentObject = this;
				while (currentObject.parent != null)
					currentObject = currentObject.parent;
				return currentObject as Container;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Stage stage
		{
			get
			{
				return topmost as Stage;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Container worldSpaceContainer
		{
			get
			{
				Container wsc = null;
				DisplayObject currentObject = this;
				while (currentObject.parent != null)
				{
					if ((currentObject is Container) && ((Container)currentObject).renderMode == RenderMode.WorldSpace)
					{
						wsc = (Container)currentObject;
						break;
					}
					currentObject = currentObject.parent;
				}

				return wsc;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		virtual public bool touchable
		{
			get { return _touchable; }
			set { _touchable = value; }
		}

		/// <summary>
		/// 进入绘画模式，整个对象将画到一张RenderTexture上，然后这种贴图将代替原有的显示内容。
		/// 可以在onPaint回调里对这张纹理进行进一步操作，实现特殊效果。
		/// 可能有多个地方要求进入绘画模式，这里用requestorId加以区别，取值是1、2、4、8、16以此类推。1024内内部保留。用户自定义的id从1024开始。
		/// </summary>
		/// <param name="requestId">请求者id</param>
		/// <param name="margin">纹理四周的留空。如果特殊处理后的内容大于原内容，那么这里的设置可以使纹理扩大。</param>
		public void EnterPaintingMode(int requestorId, Margin? margin)
		{
			bool first = _paintingMode == 0;
			_paintingMode |= requestorId;
			if (first)
			{
				if (paintingGraphics == null)
				{
					if (graphics == null)
						paintingGraphics = new NGraphics(this.gameObject);
					else
					{
						GameObject go = new GameObject(this.gameObject.name + " (Painter)");
						go.layer = this.gameObject.layer;
						FairyGUI.Utils.ToolSet.SetParent(go.transform, cachedTransform);
						go.hideFlags = DisplayOptions.hideFlags;
						paintingGraphics = new NGraphics(go);
					}
				}
				else
					paintingGraphics.enabled = true;
				paintingGraphics.vertexMatrix = null;

				if (this is Container)
				{
					((Container)this).SetChildrenLayer(CaptureCamera.hiddenLayer);
					((Container)this).UpdateBatchingFlags();
				}
				else
					this.InvalidateBatchingState();

				if (graphics != null)
					this.gameObject.layer = CaptureCamera.hiddenLayer;

				_paintingMargin = new Margin();
			}
			if (margin != null)
				_paintingMargin = (Margin)margin;
			_paintingFlag = 1;
		}

		/// <summary>
		/// 离开绘画模式
		/// </summary>
		/// <param name="requestId"></param>
		public void LeavePaintingMode(int requestorId)
		{
			if (_paintingMode == 0 || _disposed)
				return;

			_paintingMode ^= requestorId;
			if (_paintingMode == 0)
			{
				paintingGraphics.ClearMesh();
				paintingGraphics.enabled = false;

				if (this is Container)
				{
					((Container)this).SetChildrenLayer(this.layer);
					((Container)this).UpdateBatchingFlags();
				}
				else
					this.InvalidateBatchingState();

				if (graphics != null)
					this.gameObject.layer = paintingGraphics.gameObject.layer;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool paintingMode
		{
			get { return _paintingMode > 0; }
		}

		/// <summary>
		/// 
		/// </summary>
		public IFilter filter
		{
			get
			{
				return _filter;
			}

			set
			{
				if (!Application.isPlaying) //编辑期间不支持！！
					return;

				if (value == _filter)
					return;

				if (_filter != null)
					_filter.Dispose();

				if (value != null && value.target != null)
					value.target.filter = null;

				_filter = value;
				if (_filter != null)
					_filter.target = this;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public BlendMode blendMode
		{
			get { return _blendMode; }
			set
			{
				_blendMode = value;
				InvalidateBatchingState();

				if (this is Container)
				{
					if (_blendMode != BlendMode.Normal)
					{
						if (!Application.isPlaying) //编辑期间不支持！！
							return;

						EnterPaintingMode(2, null);
						paintingGraphics.blendMode = _blendMode;
					}
					else
						LeavePaintingMode(2);
				}
				else
					graphics.blendMode = _blendMode;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="targetSpace"></param>
		/// <returns></returns>
		virtual public Rect GetBounds(DisplayObject targetSpace)
		{
			if (targetSpace == this || _contentRect.width == 0 || _contentRect.height == 0) // optimization
			{
				return _contentRect;
			}
			else if (targetSpace == parent && this.rotation == 0)
			{
				float sx = this.scaleX;
				float sy = this.scaleY;
				return new Rect(this.x, this.y, _contentRect.width * sx, _contentRect.height * sy);
			}
			else
				return TransformRect(_contentRect, targetSpace);
		}

		virtual protected internal DisplayObject HitTest(bool forTouch)
		{
			if (!_visible || (forTouch && (!_touchable || _optimizeNotTouchable)))
				return null;

			Rect rect = GetBounds(this);
			if (rect.width == 0 || rect.height == 0)
				return null;

			Vector2 localPoint = WorldToLocal(HitTestContext.worldPoint, HitTestContext.direction);
			if (rect.Contains(localPoint))
				return this;
			else
				return null;
		}

		/// <summary>
		/// 将舞台坐标转换为本地坐标
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public Vector2 GlobalToLocal(Vector2 point)
		{
			Container wsc = this.worldSpaceContainer;

			if (wsc != null)//I am in a world space
			{
				Camera cam = wsc.GetRenderCamera();
				Vector3 worldPoint;
				Vector3 direction;
				Vector3 screenPoint = new Vector3();
				screenPoint.x = point.x;
				screenPoint.y = Screen.height - point.y;

				if (wsc.hitArea is MeshColliderHitTest)
				{
					if (((MeshColliderHitTest)wsc.hitArea).ScreenToLocal(cam, screenPoint, ref point))
					{
						worldPoint = Stage.inst.cachedTransform.TransformPoint(point.x, -point.y, 0);
						direction = Vector3.back;
					}
					else //当射线没有击中模型时，无法确定本地坐标
						return new Vector2(float.NaN, float.NaN);
				}
				else
				{
					screenPoint.z = cam.WorldToScreenPoint(this.cachedTransform.position).z;
					worldPoint = cam.ScreenToWorldPoint(screenPoint);
					Ray ray = cam.ScreenPointToRay(screenPoint);
					direction = Vector3.zero - ray.direction;
				}

				return this.WorldToLocal(worldPoint, direction);
			}
			else //I am in stage space
			{
				Vector3 worldPoint = Stage.inst.cachedTransform.TransformPoint(point.x, -point.y, 0);
				return this.WorldToLocal(worldPoint, Vector3.back);
			}
		}

		/// <summary>
		/// 将本地坐标转换为舞台坐标
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public Vector2 LocalToGlobal(Vector2 point)
		{
			Container wsc = this.worldSpaceContainer;

			Vector3 worldPoint = this.cachedTransform.TransformPoint(point.x, -point.y, 0);
			if (wsc != null)
			{
				if (wsc.hitArea is MeshColliderHitTest)
					Debug.LogError("Not supported for UIPainter, use TransfromPoint instead.");

				Vector3 screePoint = wsc.GetRenderCamera().WorldToScreenPoint(worldPoint);
				return new Vector2(screePoint.x, Stage.inst.stageHeight - screePoint.y);
			}
			else
			{
				point = Stage.inst.cachedTransform.InverseTransformPoint(worldPoint);
				point.y = -point.y;
				return point;
			}
		}

		/// <summary>
		/// 转换世界坐标点到等效的本地xy平面的点。等效的意思是他们在屏幕方向看到的位置一样。
		/// 返回的点是在对象的本地坐标空间，且z=0
		/// </summary>
		/// <param name="worldPoint"></param>
		/// <param name="direction"></param>
		/// <returns></returns>
		public Vector3 WorldToLocal(Vector3 worldPoint, Vector3 direction)
		{
			Vector3 localPoint = this.cachedTransform.InverseTransformPoint(worldPoint);
			if (localPoint.z != 0) //如果对象绕x轴或y轴旋转过，或者对象是在透视相机，那么z值可能不为0，
			{
				//将世界坐标的摄影机方向在本地空间上投射，求出与xy平面的交点
				direction = this.cachedTransform.InverseTransformDirection(direction);
				float distOnLine = Vector3.Dot(Vector3.zero - localPoint, Vector3.forward) / Vector3.Dot(direction, Vector3.forward);
				if (float.IsInfinity(distOnLine))
					return new Vector2(0, 0);

				localPoint = localPoint + direction * distOnLine;
			}
			else if (_transformMatrix != null)
			{
				Matrix4x4 mm = (Matrix4x4)_transformMatrix;
				Vector3 camPos = new Vector3(_pivot.x * _contentRect.width, -_pivot.y * _contentRect.height, _focalLength);
				Vector3 center = new Vector3(camPos.x, camPos.y, 0);
				center -= mm.MultiplyPoint(center);
				mm = mm.inverse;
				//相机位置需要变换！
				camPos = mm.MultiplyPoint(camPos);
				//消除轴心影响
				localPoint -= center;
				localPoint = mm.MultiplyPoint(localPoint);
				//获得与平面交点
				Vector3 vec = localPoint - camPos;
				float lambda = -camPos.z / vec.z;
				localPoint.x = camPos.x + lambda * vec.x;
				localPoint.y = camPos.y + lambda * vec.y;
				localPoint.z = 0;

				//在这写可能不大合适，但要转回世界坐标，才能保证孩子的点击检测正确进行
				HitTestContext.worldPoint = this.cachedTransform.TransformPoint(localPoint);
			}
			localPoint.y = -localPoint.y;
			localPoint.x -= this._positionOffset.x;
			localPoint.y -= this._positionOffset.y;

			return localPoint;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="point"></param>
		/// <param name="targetSpace">null if to world space</param>
		/// <returns></returns>
		public Vector2 TransformPoint(Vector2 point, DisplayObject targetSpace)
		{
			if (targetSpace == this)
				return point;

			point.y = -point.y;
			Vector3 v = this.cachedTransform.TransformPoint(point);
			if (targetSpace != null)
			{
				v = targetSpace.cachedTransform.InverseTransformPoint(v);
				v.y = -v.y;
				v.x -= targetSpace._positionOffset.x;
				v.y -= targetSpace._positionOffset.y;
			}
			return v;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rect"></param>
		/// <param name="targetSpace">null if to world space</param>
		/// <returns></returns>
		public Rect TransformRect(Rect rect, DisplayObject targetSpace)
		{
			if (targetSpace == this)
				return rect;

			if (targetSpace == parent && rotation == 0f) // optimization
			{
				float scaleX = this.scaleX;
				float scaleY = this.scaleY;
				rect = new Rect(this.x + rect.x, this.y + rect.y, rect.width, rect.height);
				rect.x *= scaleX;
				rect.y *= scaleY;
				rect.width *= scaleX;
				rect.height *= scaleY;

				return rect;
			}
			else
			{
				float xMin = float.MaxValue, xMax = float.MinValue;
				float yMin = float.MaxValue, yMax = float.MinValue;
				Rect result = Rect.MinMaxRect(xMin, yMin, xMax, yMax);

				TransformRectPoint(rect.xMin, rect.yMin, targetSpace, ref result);
				TransformRectPoint(rect.xMax, rect.yMin, targetSpace, ref result);
				TransformRectPoint(rect.xMin, rect.yMax, targetSpace, ref result);
				TransformRectPoint(rect.xMax, rect.yMax, targetSpace, ref result);

				return result;
			}
		}

		protected void TransformRectPoint(float px, float py, DisplayObject targetSpace, ref Rect rect)
		{
			px += _positionOffset.x;
			py += _positionOffset.y;
			Vector2 v = this.cachedTransform.TransformPoint(px, -py, 0);
			if (targetSpace != null)
			{
				v = targetSpace.cachedTransform.InverseTransformPoint(v);
				v.y = -v.y;
				v.x -= targetSpace._positionOffset.x;
				v.y -= targetSpace._positionOffset.y;
			}
			if (rect.xMin > v.x) rect.xMin = v.x;
			if (rect.xMax < v.x) rect.xMax = v.x;
			if (rect.yMin > v.y) rect.yMin = v.y;
			if (rect.yMax < v.y) rect.yMax = v.y;
		}

		/// <summary>
		/// 
		/// </summary>
		public void RemoveFromParent()
		{
			if (parent != null)
				parent.RemoveChild(this);
		}

		/// <summary>
		/// 
		/// </summary>
		public void InvalidateBatchingState()
		{
			if (parent != null)
				parent.InvalidateBatchingState(true);
		}

		virtual public void Update(UpdateContext context)
		{
			if (graphics != null)
			{
				graphics.alpha = context.alpha * _alpha;
				graphics.grayed = context.grayed | _grayed;
				graphics.UpdateMaterial(context);
			}

			if (_paintingMode != 0)
			{
				NTexture paintingTexture = paintingGraphics.texture;
				if (paintingTexture != null && paintingTexture.disposed) //Texture可能已被Stage.MonitorTexture销毁
				{
					paintingTexture = null;
					_paintingFlag = 1;
				}
				if (_paintingFlag == 1)
				{
					_paintingFlag = 0;

					//从优化考虑，决定使用绘画模式的容器都需要明确指定大小，而不是自动计算包围。这在UI使用上并没有问题，因为组件总是有固定大小的
					int textureWidth = Mathf.RoundToInt(_contentRect.width + _paintingMargin.left + _paintingMargin.right);
					int textureHeight = Mathf.RoundToInt(_contentRect.height + _paintingMargin.top + _paintingMargin.bottom);
					if (paintingTexture == null || paintingTexture.width != textureWidth || paintingTexture.height != textureHeight)
					{
						if (paintingTexture != null)
							paintingTexture.Dispose();
						if (textureWidth > 0 && textureHeight > 0)
						{
							paintingTexture = new NTexture(CaptureCamera.CreateRenderTexture(textureWidth, textureHeight, false));
							Stage.inst.MonitorTexture(paintingTexture);
						}
						else
							paintingTexture = null;
						paintingGraphics.texture = paintingTexture;
					}

					if (paintingGraphics.material == null)
					{
						paintingGraphics.material = new Material(ShaderConfig.GetShader(ShaderConfig.imageShader));
						paintingGraphics.material.hideFlags = DisplayOptions.hideFlags;
					}

					if (paintingTexture != null)
					{
						paintingGraphics.SetOneQuadMesh(
							new Rect(-_paintingMargin.left, -_paintingMargin.top, paintingTexture.width, paintingTexture.height),
							new Rect(0, 0, 1, 1), Color.white);
					}
					else
						paintingGraphics.ClearMesh();
				}

				if (paintingTexture != null)
				{
					paintingTexture.lastActive = Time.time;

					if (!(this is Container)) //如果是容器，这句移到Container.Update的最后执行，因为容器中可能也有需要Capture的内容，要等他们完成后再进行容器的Capture。
						UpdateContext.OnEnd += _captureDelegate;
				}

				paintingGraphics.UpdateMaterial(context);
			}

			if (_filter != null)
				_filter.Update();

			context.counter++;
		}

		void Capture()
		{
			Vector2 offset = new Vector2(_paintingMargin.left, _paintingMargin.top);
			CaptureCamera.Capture(this, (RenderTexture)paintingGraphics.texture.nativeTexture, offset);

			_paintingFlag = 2; //2表示已完成一次Capture
			if (onPaint != null)
				onPaint();
		}

		virtual protected void SetGO_Visible()
		{
			if (parent != null && _visible)
			{
#if (UNITY_4_6 || UNITY_4_7 || UNITY_5)
				cachedTransform.SetParent(parent.cachedTransform, false);
#else
				Vector3 v1 = cachedTransform.localPosition;
				Vector3 v2 = cachedTransform.localScale;
				Vector3 v3 = cachedTransform.localEulerAngles;

				cachedTransform.parent = parent.cachedTransform;

				cachedTransform.localPosition = v1;
				cachedTransform.localScale = v2;
				cachedTransform.localEulerAngles = v3;
#endif
				gameObject.hideFlags = DisplayOptions.hideFlags;
				gameObject.SetActive(true);

				int layerValue = parent.gameObject.layer;
				if (parent._paintingMode != 0)
					layerValue = CaptureCamera.hiddenLayer;

				if ((this is Container) && this.gameObject.layer != layerValue && this._paintingMode == 0)
					((Container)this).SetChildrenLayer(layerValue);

				this.layer = layerValue;
			}
			else if (!_disposed)
			{
				if (Application.isPlaying)
				{
#if (UNITY_4_6 || UNITY_4_7 || UNITY_5)
					cachedTransform.SetParent(null, false);
#else
					cachedTransform.parent = null;
#endif
				}
				gameObject.hideFlags |= HideFlags.HideInHierarchy;
				gameObject.SetActive(false);
			}
		}

		virtual public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;
			RemoveFromParent();
			RemoveEventListeners();
			if (graphics != null)
				graphics.Dispose();
			if (_filter != null)
				_filter.Dispose();
			if (paintingGraphics != null)
			{
				if (paintingGraphics.texture != null)
					paintingGraphics.texture.Dispose();

				paintingGraphics.Dispose();
				if (paintingGraphics.gameObject != this.gameObject)
				{
					if (Application.isPlaying)
						GameObject.Destroy(paintingGraphics.gameObject);
					else
						GameObject.DestroyImmediate(paintingGraphics.gameObject);
				}
			}
			DestroyGameObject();
		}
	}
}
