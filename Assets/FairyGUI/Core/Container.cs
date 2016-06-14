﻿using System;
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
		/// <summary>
		/// 
		/// </summary>
		public RenderMode renderMode;

		/// <summary>
		/// 
		/// </summary>
		public Camera renderCamera;

		/// <summary>
		/// 
		/// </summary>
		public bool opaque;


		/// <summary>
		/// 
		/// </summary>
		public Vector4? clipSoftness;

		/// <summary>
		/// 
		/// </summary>
		public IHitTest hitArea;

		///
		public bool touchChildren;

		public EventCallback0 onUpdate;

		List<DisplayObject> _children;
		DisplayObject _mask;
		Rect? _clipRect;

		bool _fBatchingRequested;
		bool _fBatchingRoot;
		bool _fBatching;
		List<DisplayObject> _descendants;

		internal int _panelOrder;
		bool _ownsGameObject;

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
			touchChildren = true;
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

					InvalidateBatchingState(true);
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
					InvalidateBatchingState(true);
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
			InvalidateBatchingState(true);
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
			InvalidateBatchingState(true);
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
			InvalidateBatchingState(true);
		}

		public Rect? clipRect
		{
			get { return _clipRect; }
			set
			{
				_clipRect = value;
				UpdateBatchingFlags();
			}
		}

		public DisplayObject mask
		{
			get { return _mask; }
			set
			{
				_mask = value;
				UpdateBatchingFlags();
			}
		}

		override public bool touchable
		{
			get { return base.touchable; }
			set
			{
				base.touchable = value;
				if (hitArea != null)
					hitArea.SetEnabled(value);
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
			if (!visible || (forTouch && (!touchable || _optimizeNotTouchable)))
				return null;

			Vector2 localPoint = new Vector2();
			Vector3 savedWorldPoint = HitTestContext.worldPoint;
			Vector3 savedDirection = HitTestContext.direction;

			if (hitArea != null)
			{
				if (!hitArea.HitTest(this, ref localPoint))
				{
					HitTestContext.worldPoint = savedWorldPoint;
					HitTestContext.direction = savedDirection;
					return null;
				}
			}
			else
			{
				localPoint = GetHitTestLocalPoint();
				if (_clipRect != null && !((Rect)_clipRect).Contains(localPoint))
				{
					HitTestContext.worldPoint = savedWorldPoint;
					HitTestContext.direction = savedDirection;
					return null;
				}
			}

			if (_mask != null && _mask.HitTest(false) == null)
				return null;

			DisplayObject target = null;
			if (touchChildren)
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

			if (target == null && opaque && _contentRect.Contains(localPoint))
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
				_fBatching = value;
				UpdateBatchingFlags();
			}
		}

		internal void UpdateBatchingFlags()
		{
			bool oldValue = _fBatchingRoot;
			_fBatchingRoot = _fBatching || _clipRect != null || _mask != null || _paintingMode > 0;
			if (oldValue != _fBatchingRoot)
			{
				if (_fBatchingRoot)
					_fBatchingRequested = true;
				else if (_descendants != null)
					_descendants.Clear();

				InvalidateBatchingState();
			}
		}

		public void InvalidateBatchingState(bool childrenChanged)
		{
			if (childrenChanged && _fBatchingRoot)
				_fBatchingRequested = true;
			else
			{
				Container p = this.parent;
				while (p != null)
				{
					if (p._fBatchingRoot)
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
				if ((child is Container) && !child.paintingMode)
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
			if (onUpdate != null)
				onUpdate();

			base.Update(context);

			if (_mask != null)
				context.EnterClipping(this.id, null, null);
			else if (_clipRect != null)
				context.EnterClipping(this.id, this.TransformRect((Rect)_clipRect, null), clipSoftness);

			float savedAlpha = context.alpha;
			context.alpha *= this.alpha;
			bool savedGrayed = context.grayed;
			context.grayed = context.grayed || this.grayed;

			if (_fBatching)
				context.batchingDepth++;

			if (context.batchingDepth > 0)
			{
				if (_mask != null)
					_mask.graphics.maskFrameId = UpdateContext.frameId;

				int cnt = _children.Count;
				for (int i = 0; i < cnt; i++)
				{
					DisplayObject child = _children[i];
					if (child.visible)
						child.Update(context);
				}
			}
			else
			{
				if (_mask != null)
				{
					_mask.graphics.maskFrameId = UpdateContext.frameId;
					_mask.renderingOrder = context.renderingOrder++;
				}

				int cnt = _children.Count;
				for (int i = 0; i < cnt; i++)
				{
					DisplayObject child = _children[i];
					if (child.visible)
					{
						if (child != _mask)
							child.renderingOrder = context.renderingOrder++;

						child.Update(context);
					}
				}
			}

			if (_fBatching)
			{
				if (context.batchingDepth == 1)
					SetRenderingOrder(context);
				context.batchingDepth--;
			}

			context.alpha = savedAlpha;
			context.grayed = savedGrayed;

			if (_clipRect != null || _mask != null)
				context.LeaveClipping();

			if (_paintingMode > 0 && paintingGraphics.texture != null)
				UpdateContext.OnEnd += _captureDelegate;
		}

		private void SetRenderingOrder(UpdateContext context)
		{
			if (_fBatchingRequested)
				DoFairyBatching();

			if (_mask != null)
				_mask.renderingOrder = context.renderingOrder++;

			int cnt = _descendants.Count;
			for (int i = 0; i < cnt; i++)
			{
				DisplayObject child = _descendants[i];
				if (child != _mask)
					child.renderingOrder = context.renderingOrder++;

				if ((child is Container) && ((Container)child)._fBatchingRoot)
					((Container)child).SetRenderingOrder(context);
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
			//Debug.Log("DoFairyBatching " + cnt);

			int i, j;
			for (i = 0; i < cnt; i++)
			{
				DisplayObject current = _descendants[i];
				Rect bound = current._internal_bounds;
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

					if (ToolSet.Intersects(ref bound, ref test._internal_bounds))
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
					if (container._fBatchingRoot)
					{
						initiator._descendants.Add(container);
						container._internal_bounds = container.GetBounds(initiator);
						if (container._fBatchingRequested)
							container.DoFairyBatching();
					}
					else
						container.CollectChildren(initiator);
				}
				else if (child != initiator._mask)
				{
					child._internal_bounds = child.GetBounds(initiator);
					initiator._descendants.Add(child);
				}
			}
		}

		public override void Dispose()
		{
			base.Dispose(); //Destroy GameObject tree first, avoid destroying each seperately;

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
