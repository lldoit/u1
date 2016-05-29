using UnityEngine;

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

		public GObject gOwner;

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

		float _alpha;
		bool _visible;
		bool _touchable;
		float _pivotX;
		float _pivotY;
		int _renderingOrder;

		protected Rect _contentRect;
		protected internal bool _optimizeNotTouchable;
		protected internal bool _skipInFairyBatching;

		internal Rect _tmpBounds;
		internal uint _internalIndex;
		internal bool _disposed;

		internal static uint _gInstanceCounter;

		public DisplayObject()
		{
			_alpha = 1;
			_visible = true;
			_touchable = true;
			_internalIndex = _gInstanceCounter++;

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

		public float alpha
		{
			get { return _alpha; }
			set { _alpha = value; }
		}

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

		public float x
		{
			get { return cachedTransform.localPosition.x + _pivotX; }
			set
			{
				Vector3 v = cachedTransform.localPosition;
				v.x = value - pivotX;
				cachedTransform.localPosition = v;
			}
		}
		public float y
		{
			get { return -cachedTransform.localPosition.y + _pivotY; }
			set
			{
				Vector3 v = cachedTransform.localPosition;
				v.y = -value + pivotY;
				cachedTransform.localPosition = v;
			}
		}

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

		public Vector2 xy
		{
			get { return new Vector2(this.x, this.y); }
			set { SetPosition(value.x, value.y, cachedTransform.localPosition.z); }
		}

		public Vector3 position
		{
			get { return new Vector3(this.x, this.y, this.z); }
			set { SetPosition(value.x, value.y, value.z); }
		}

		public void SetXY(float xv, float yv)
		{
			SetPosition(xv, yv, cachedTransform.localPosition.z);
		}

		public void SetPosition(float xv, float yv, float zv)
		{
			Vector3 v = cachedTransform.localPosition;
			v.x = xv - pivotX;
			v.y = -yv + pivotY;
			v.z = zv;
			cachedTransform.localPosition = v;
		}

		protected float pivotX
		{
			get { return _pivotX; }
			set
			{
				Vector3 v = cachedTransform.localPosition;
				v.x += (_pivotX - value);
				cachedTransform.localPosition = v;
				_pivotX = value;
			}
		}

		protected float pivotY
		{
			get { return _pivotY; }
			set
			{
				Vector3 v = cachedTransform.localPosition;
				v.y += value - _pivotY;
				cachedTransform.localPosition = v;
				_pivotY = value;
			}
		}

		public float width
		{
			get { return this.size.x; }
			set { SetSize(value, float.NaN); }
		}

		public float height
		{
			get { return this.size.y; }
			set { SetSize(float.NaN, value); }
		}

		public Vector2 size
		{
			get { return GetBounds(parent != null ? parent : this).size; }
			set { SetSize(value.x, value.y); }
		}

		virtual public void SetSize(float wv, float hv)
		{
			SetScale(1, 1);
			Rect bounds = GetBounds(parent != null ? parent : this);
			float nx = this.scaleX;
			float ny = this.scaleY;
			if (!float.IsNaN(wv) && bounds.width != 0)
				nx = wv / bounds.width;
			if (!float.IsNaN(hv) && bounds.height != 0)
				ny = hv / bounds.height;
			SetScale(nx, ny);
		}

		public float scaleX
		{
			get { return cachedTransform.localScale.x; }
			set
			{
				Vector3 v = cachedTransform.localScale;
				v.x = value;
				v.z = value;
				cachedTransform.localScale = v;
			}
		}

		public float scaleY
		{
			get { return cachedTransform.localScale.y; }
			set
			{
				Vector3 v = cachedTransform.localScale;
				v.y = value;
				cachedTransform.localScale = v;
			}
		}

		public void SetScale(float xv, float yv)
		{
			Vector3 v = cachedTransform.localScale;
			v.x = xv;
			v.y = yv;
			v.z = xv;
			cachedTransform.localScale = v;
		}

		public Vector2 scale
		{
			get { return cachedTransform.localScale; }
			set
			{
				SetScale(value.x, value.y);
			}
		}

		public float rotation
		{
			get { return -cachedTransform.localEulerAngles.z; }
			set
			{
				Vector3 v = cachedTransform.localEulerAngles;
				v.z = -value;
				cachedTransform.localEulerAngles = v;
			}
		}

		public float rotationX
		{
			get { return cachedTransform.localEulerAngles.x; }
			set
			{
				Vector3 v = cachedTransform.localEulerAngles;
				v.x = value;
				cachedTransform.localEulerAngles = v;
			}
		}

		public float rotationY
		{
			get { return cachedTransform.localEulerAngles.y; }
			set
			{
				Vector3 v = cachedTransform.localEulerAngles;
				v.y = value;
				cachedTransform.localEulerAngles = v;
			}
		}

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
			}
		}

		virtual public int layer
		{
			get
			{
				return gameObject.layer;
			}
			set
			{
				gameObject.layer = value;
			}
		}

		public bool isDisposed
		{
			get { return gameObject == null; }
		}

		public void SetGrayed(bool value)
		{
			if (graphics == null)
				return;

			graphics.grayed = value;
		}

		internal void SetParent(Container value)
		{
			if (parent != value)
			{
				parent = value;
				SetGO_Visible();
			}
		}

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

		public Stage stage
		{
			get
			{
				return topmost as Stage;
			}
		}

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

		virtual public bool touchable
		{
			get { return _touchable; }
			set { _touchable = value; }
		}

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
				Rect rect = new Rect(this.x - _contentRect.x * sx,
					this.y - _contentRect.y * sy,
					_contentRect.width * sx,
					_contentRect.height * sy);
				return rect;
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
					else
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
			localPoint.y = -localPoint.y;
			localPoint.x -= this._pivotX;
			localPoint.y -= this._pivotY;

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
				v.x -= targetSpace._pivotX;
				v.y -= targetSpace._pivotY;
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
			px += _pivotX;
			py += _pivotY;
			Vector2 v = this.cachedTransform.TransformPoint(px, -py, 0);
			if (targetSpace != null)
			{
				v = targetSpace.cachedTransform.InverseTransformPoint(v);
				v.y = -v.y;
				v.x -= targetSpace._pivotX;
				v.y -= targetSpace._pivotY;
			}
			if (rect.xMin > v.x) rect.xMin = v.x;
			if (rect.xMax < v.x) rect.xMax = v.x;
			if (rect.yMin > v.y) rect.yMin = v.y;
			if (rect.yMax < v.y) rect.yMax = v.y;
		}

		public void RemoveFromParent()
		{
			if (parent != null)
				parent.RemoveChild(this);
		}

		virtual public void InvalidateBatchingState()
		{
			if (parent != null)
				parent.InvalidateBatchingState();
		}

		virtual public void Update(UpdateContext context)
		{
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

				if ((this is Container) && this.layer != parent.layer)
					((Container)this).SetChildrenLayer(parent.layer);
				this.layer = parent.layer;
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
			DestroyGameObject();
		}
	}
}
