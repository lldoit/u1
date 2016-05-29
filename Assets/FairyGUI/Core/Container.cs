using System;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class Container : DisplayObject
	{
		public RenderMode renderMode;
		public Camera renderCamera;

		List<DisplayObject> _children;

		Rect? _clipRect;
		DisplayObject _mask;
		IHitTest _hitArea;
		Vector4? _clipSoftness;
		bool _touchChildren;

		bool _fBatchingRequested;
		bool _fBatchingInherited;
		bool _fBatching;
		List<DisplayObject> _descendants;

		bool _skipRendering;
		float _alphaInFrame;

		internal EventCallback0 _onUpdate;
		internal int _panelOrder;
		internal bool _ownsGameObject;

		public Container()
			: base()
		{
			_ownsGameObject = true;

			CreateGameObject("Container");
			Init();
		}

		public Container(string gameObjectName)
			: base()
		{
			_ownsGameObject = true;

			CreateGameObject(gameObjectName);
			Init();
		}

		public Container(GameObject attachTarget)
			: base()
		{
			_ownsGameObject = false;

			this.gameObject = attachTarget;
			cachedTransform = gameObject.transform;

			Init();
		}

		void Init()
		{
			_children = new List<DisplayObject>();
			_touchChildren = true;
		}

		public int numChildren
		{
			get { return _children.Count; }
		}

		public DisplayObject AddChild(DisplayObject child)
		{
			AddChildAt(child, _children.Count);
			return child;
		}

		public DisplayObject AddChildAt(DisplayObject child, int index)
		{
			int count = _children.Count;
			if (index >= 0 && index <= count)
			{
				if (child.parent == this)
				{
					SetChildIndex(child, index);
				}
				else
				{
					child.RemoveFromParent();
					if (index == count)
						_children.Add(child);
					else
						_children.Insert(index, child);
					child.SetParent(this);

					if (stage != null)
					{
						if (child is Container)
							child.onAddedToStage.BroadcastCall();
						else
							child.onAddedToStage.Call();
					}

					InvalidateBatchingState();
				}
				return child;
			}
			else
			{
				throw new Exception("Invalid child index");
			}
		}

		public bool Contains(DisplayObject child)
		{
			return _children.Contains(child);
		}

		public DisplayObject GetChildAt(int index)
		{
			return _children[index];
		}

		public DisplayObject GetChild(string name)
		{
			int cnt = _children.Count;
			for (int i = 0; i < cnt; ++i)
			{
				if (_children[i].name == name)
					return _children[i];
			}

			return null;
		}

		public int GetChildIndex(DisplayObject child)
		{
			return _children.IndexOf(child);
		}

		public DisplayObject RemoveChild(DisplayObject child)
		{
			return RemoveChild(child, false);
		}

		public DisplayObject RemoveChild(DisplayObject child, bool dispose)
		{
			if (child.parent != this)
				throw new Exception("obj is not a child");

			int i = _children.IndexOf(child);
			if (i >= 0)
				return RemoveChildAt(i, dispose);
			else
				return null;
		}

		public DisplayObject RemoveChildAt(int index)
		{
			return RemoveChildAt(index, false);
		}

		public DisplayObject RemoveChildAt(int index, bool dispose)
		{
			if (index >= 0 && index < _children.Count)
			{
				DisplayObject child = _children[index];

				if (stage != null)
				{
					if (child is Container)
						child.onRemovedFromStage.BroadcastCall();
					else
						child.onRemovedFromStage.Call();
				}
				_children.Remove(child);
				if (!dispose)
				{
					child.SetParent(null);
					InvalidateBatchingState();
				}
				else
					child.Dispose();

				return child;
			}
			else
				throw new Exception("Invalid child index");
		}

		public void RemoveChildren()
		{
			RemoveChildren(0, int.MaxValue, false);
		}

		public void RemoveChildren(int beginIndex, int endIndex, bool dispose)
		{
			if (endIndex < 0 || endIndex >= numChildren)
				endIndex = numChildren - 1;

			for (int i = beginIndex; i <= endIndex; ++i)
				RemoveChildAt(beginIndex, dispose);
		}

		public void SetChildIndex(DisplayObject child, int index)
		{
			int oldIndex = _children.IndexOf(child);
			if (oldIndex == index) return;
			if (oldIndex == -1) throw new ArgumentException("Not a child of this container");
			_children.RemoveAt(oldIndex);
			if (index >= _children.Count)
				_children.Add(child);
			else
				_children.Insert(index, child);
			InvalidateBatchingState();
		}

		public void SwapChildren(DisplayObject child1, DisplayObject child2)
		{
			int index1 = _children.IndexOf(child1);
			int index2 = _children.IndexOf(child2);
			if (index1 == -1 || index2 == -1)
				throw new Exception("Not a child of this container");
			SwapChildrenAt(index1, index2);
		}

		public void SwapChildrenAt(int index1, int index2)
		{
			DisplayObject obj1 = _children[index1];
			DisplayObject obj2 = _children[index2];
			_children[index1] = obj2;
			_children[index2] = obj1;
			InvalidateBatchingState();
		}

		public void ChangeChildrenOrder(List<int> indice, List<DisplayObject> objs)
		{
			int cnt = indice.Count;
			for (int i = 0; i < cnt; i++)
			{
				DisplayObject obj = objs[i];
				if (obj.parent != this)
					throw new Exception("Not a child of this container");

				_children[indice[i]] = obj;
			}
			InvalidateBatchingState();
		}

		public Rect? clipRect
		{
			get { return _clipRect; }
			set
			{
				_clipRect = value;
				if (_clipRect != null)
					_contentRect = (Rect)_clipRect;
				else
					_contentRect.Set(0, 0, 0, 0);
			}
		}

		//left-top-right-bottom
		public Vector4? clipSoftness
		{
			get { return _clipSoftness; }
			set { _clipSoftness = value; }
		}

		public DisplayObject mask
		{
			get { return _mask; }
			set
			{
				_mask = value;
				InvalidateBatchingState();
			}
		}

		public IHitTest hitArea
		{
			get { return _hitArea; }
			set { _hitArea = value; }
		}

		public bool touchChildren
		{
			get { return _touchChildren; }
			set { _touchChildren = value; }
		}

		override public bool touchable
		{
			get { return base.touchable; }
			set
			{
				base.touchable = value;
				if (_hitArea != null)
					_hitArea.SetEnabled(value);
			}
		}

		public override Rect GetBounds(DisplayObject targetSpace)
		{
			if (_clipRect != null)
				return TransformRect((Rect)_clipRect, targetSpace);

			int count = _children.Count;

			Rect rect;
			if (count == 0)
			{
				Vector2 v = TransformPoint(Vector2.zero, targetSpace);
				rect = Rect.MinMaxRect(v.x, v.y, 0, 0);
			}
			else if (count == 1)
			{
				rect = _children[0].GetBounds(targetSpace);
			}
			else
			{
				float minX = float.MaxValue, maxX = float.MinValue;
				float minY = float.MaxValue, maxY = float.MinValue;

				for (int i = 0; i < count; ++i)
				{
					rect = _children[i].GetBounds(targetSpace);
					minX = minX < rect.xMin ? minX : rect.xMin;
					maxX = maxX > rect.xMax ? maxX : rect.xMax;
					minY = minY < rect.yMin ? minY : rect.yMin;
					maxY = maxY > rect.yMax ? maxY : rect.yMax;
				}

				rect = Rect.MinMaxRect(minX, minY, maxX, maxY);
			}

			return rect;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public Camera GetRenderCamera()
		{
			if (renderMode == RenderMode.ScreenSpaceOverlay)
				return StageCamera.main;
			else
			{
				Camera cam = this.renderCamera;
				if (cam == null)
					cam = Camera.main;
				if (cam == null)
					cam = StageCamera.main;
				return cam;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stagePoint"></param>
		/// <returns></returns>
		public DisplayObject HitTest(Vector2 stagePoint)
		{
			if (StageCamera.main == null)
				return null;

			HitTestContext.screenPoint = new Vector2(stagePoint.x, Screen.height - stagePoint.y);
			HitTestContext.worldPoint = StageCamera.main.ScreenToWorldPoint(HitTestContext.screenPoint);
			HitTestContext.direction = Vector3.back;

			DisplayObject ret = HitTest(true);
			if (ret != null)
				return ret;
			else if (this is Stage)
				return this;
			else
				return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public Vector2 GetHitTestLocalPoint()
		{
			if (this.renderMode == RenderMode.WorldSpace)
			{
				Camera camera = GetRenderCamera();

				Vector3 screenPoint = camera.WorldToScreenPoint(this.cachedTransform.position); //only for query z value
				screenPoint.x = HitTestContext.screenPoint.x;
				screenPoint.y = HitTestContext.screenPoint.y;

				//获得本地z轴在世界坐标的方向
				HitTestContext.worldPoint = camera.ScreenToWorldPoint(screenPoint);
				Ray ray = camera.ScreenPointToRay(screenPoint);
				HitTestContext.direction = Vector3.zero - ray.direction;
			}

			return WorldToLocal(HitTestContext.worldPoint, HitTestContext.direction);
		}

		override protected internal DisplayObject HitTest(bool forTouch)
		{
			if (_skipRendering)
				return null;

			if (!visible || !touchable || _optimizeNotTouchable)
				return null;

			Vector2 localPoint = new Vector2();
			int hitTestResult;
			Vector3 savedWorldPoint = HitTestContext.worldPoint;
			Vector3 savedDirection = HitTestContext.direction;

			if (_hitArea != null)
			{
				hitTestResult = _hitArea.HitTest(this, ref localPoint);
				if (hitTestResult == 0)
				{
					HitTestContext.worldPoint = savedWorldPoint;
					HitTestContext.direction = savedDirection;
					return null;
				}
			}
			else
			{
				localPoint = GetHitTestLocalPoint();
				hitTestResult = 2;
			}

			if (_clipRect != null && !((Rect)_clipRect).Contains(localPoint))
			{
				HitTestContext.worldPoint = savedWorldPoint;
				HitTestContext.direction = savedDirection;
				return null;
			}

			if (_mask != null && _mask.HitTest(false) == null)
				return null;

			DisplayObject target = null;
			if (_touchChildren)
			{
				int count = _children.Count;
				for (int i = count - 1; i >= 0; --i) // front to back!
				{
					DisplayObject child = _children[i];
					if (child == _mask)
						continue;

					target = child.HitTest(forTouch);
					if (target != null)
						break;
				}
			}

			if (target == null && hitTestResult == 1)
				target = this;

			HitTestContext.worldPoint = savedWorldPoint;
			HitTestContext.direction = savedDirection;

			return target;
		}

		public bool IsAncestorOf(DisplayObject obj)
		{
			if (obj == null)
				return false;

			Container p = obj.parent;
			while (p != null)
			{
				if (p == this)
					return true;

				p = p.parent;
			}
			return false;
		}

		public bool fairyBatching
		{
			get { return _fBatching; }
			set
			{
				if (_fBatching != value)
				{
					_fBatching = value;
					_fBatchingRequested = _fBatching;
					if (!_fBatching)
					{
						if (_descendants != null)
							_descendants.Clear();
					}
				}
			}
		}

		public bool fairyBatchingInherited
		{
			get { return _fBatchingInherited; }
		}

		override public void InvalidateBatchingState()
		{
			if (_fBatching || _clipRect != null || _mask != null)
				_fBatchingRequested = true;
			else if (_fBatchingInherited)
			{
				Container p = this.parent;
				while (p != null)
				{
					if (p._fBatching || p._clipRect != null || p._mask != null)
					{
						p._fBatchingRequested = true;
						break;
					}

					p = p.parent;
				}
			}
		}

		public void SetChildrenLayer(int value)
		{
			int cnt = _children.Count;
			for (int i = 0; i < cnt; i++)
			{
				DisplayObject child = _children[i];
				child.layer = value;
				if (child is Container)
					((Container)child).SetChildrenLayer(value);
			}
		}

		override protected void SetGO_Visible()
		{
			if (_ownsGameObject)
				base.SetGO_Visible();
			else if (gameObject != null)
			{
				//we dont change transform parent of this object
				if (parent != null && visible)
					gameObject.SetActive(true);
				else
					gameObject.SetActive(false);
			}
		}

		override protected void DestroyGameObject()
		{
			if (_ownsGameObject)
				base.DestroyGameObject();
		}

		override public void Update(UpdateContext context)
		{
			_skipRendering = gOwner != null && gOwner.parent != null && !gOwner.parent.IsChildInView(gOwner);
			if (_skipRendering)
				return;

			if (_onUpdate != null)
				_onUpdate();

			if (_mask != null)
				context.EnterClipping(this._internalIndex, null, null);
			else if (_clipRect != null)
				context.EnterClipping(this._internalIndex, this.TransformRect((Rect)_clipRect, null), _clipSoftness);

			_alphaInFrame = parent != null ? (parent._alphaInFrame * this.alpha) : this.alpha;

			if (_fBatching && !_fBatchingInherited)
			{
				if (_fBatchingRequested)
				{
					DoFairyBatching();
				}

				context.batchingDepth++;
				SetRenderingOrder(context);
			}

			if ((_fBatching || _fBatchingInherited) && context.batchingDepth > 0)
			{
				int cnt = _children.Count;
				for (int i = 0; i < cnt; i++)
				{
					DisplayObject child = _children[i];
					if (child.visible)
					{
						context.counter++;
						child.Update(context);
						if (child.graphics != null)
							child.graphics.alpha = _alphaInFrame * child.alpha;
					}
				}
			}
			else
			{
				if (_mask != null)
				{
					_mask.graphics.maskFrameId = context.frameId;
					_mask.renderingOrder = context.renderingOrder;
					context.renderingOrder++;
				}

				int cnt = _children.Count;
				for (int i = 0; i < cnt; i++)
				{
					DisplayObject child = _children[i];
					if (child.visible)
					{
						context.counter++;
						if (!(child is Container) && child != _mask)
						{
							child.renderingOrder = context.renderingOrder;
							context.renderingOrder += 3;
						}

						child.Update(context);
						if (child.graphics != null)
							child.graphics.alpha = _alphaInFrame * child.alpha;
					}
				}
			}

			if (_fBatching && !_fBatchingInherited)
				context.batchingDepth--;

			if (_clipRect != null || _mask != null)
				context.LeaveClipping();
		}

		private void SetRenderingOrder(UpdateContext context)
		{
			if (_mask != null)
			{
				_mask.graphics.maskFrameId = context.frameId;
				_mask.renderingOrder = context.renderingOrder;
				context.renderingOrder++;
			}

			int cnt = _descendants.Count;
			for (int i = 0; i < cnt; i++)
			{
				DisplayObject child = _descendants[i];
				if (!(child is Container))
				{
					if (child != _mask)
					{
						child.renderingOrder = context.renderingOrder;
						context.renderingOrder += 3;
					}
				}
				else if (((Container)child)._clipRect != null || ((Container)child)._mask != null)
				{
					if (((Container)child)._fBatchingRequested)
						((Container)child).DoFairyBatching();
					((Container)child).SetRenderingOrder(context);
				}
			}
		}

		private void DoFairyBatching()
		{
			_fBatchingRequested = false;

			if (_descendants == null)
				_descendants = new List<DisplayObject>();
			else
				_descendants.Clear();
			CollectChildren(this);

			int cnt = _descendants.Count;
			int i, j;
			for (i = 0; i < cnt; i++)
			{
				DisplayObject current = _descendants[i];
				Rect bound = current._tmpBounds;
				if ((object)current.material == null || current._skipInFairyBatching)
					continue;

				for (j = i - 1; j >= 0; j--)
				{
					DisplayObject test = _descendants[j];
					if (test._skipInFairyBatching)
						break;

					if ((object)current.material == (object)test.material)
					{
						if (i != j + 1)
						{
							_descendants.RemoveAt(i);
							_descendants.Insert(j + 1, current);
						}
						break;
					}

					if (ToolSet.Intersects(ref bound, ref test._tmpBounds))
						break;
				}
			}
		}

		private void CollectChildren(Container initiator)
		{
			int count = _children.Count;
			for (int i = 0; i < count; i++)
			{
				DisplayObject child = _children[i];
				if (child is Container)
				{
					Container container = (Container)child;
					container._fBatchingInherited = true;
					container._fBatchingRequested = false;

					if (container._clipRect == null && container._mask == null)
					{
						container.CollectChildren(initiator);
					}
					else
					{
						initiator._descendants.Add(container);
						container._tmpBounds = container.GetBounds(initiator);
						container.DoFairyBatching();
					}
				}
				else if (child != initiator._mask)
				{
					child._tmpBounds = child.GetBounds(initiator);
					initiator._descendants.Add(child);
				}
			}
		}

		public override void Dispose()
		{
			base.Dispose(); //Destroy GameObject tree first, avoid destroying each seperately

			int numChildren = _children.Count;
			for (int i = numChildren - 1; i >= 0; --i)
			{
				DisplayObject obj = _children[i];
				obj.parent = null; //Avoid RemoveParent call
				obj.Dispose();
			}
		}
	}
}
