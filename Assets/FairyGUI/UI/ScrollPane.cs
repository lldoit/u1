﻿using System;
using UnityEngine;
using DG.Tweening;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class ScrollPane : EventDispatcher
	{
		/// <summary>
		/// Dispatched when scrolling.
		/// </summary>
		public EventListener onScroll { get; private set; }

		float _maskWidth;
		float _maskHeight;
		float _contentWidth;
		float _contentHeight;
		ScrollType _scrollType;
		float _scrollSpeed;
		float _mouseWheelSpeed;
		Margin _scrollBarMargin;
		bool _bouncebackEffect;
		bool _touchEffect;
		bool _scrollBarDisplayAuto;
		bool _vScrollNone;
		bool _hScrollNone;
		bool _needRefresh;

		bool _displayOnLeft;
		bool _snapToItem;
		bool _displayInDemand;
		bool _mouseWheelEnabled;
		bool _softnessOnTopOrLeftSide;
		bool _pageMode;
		Vector2 _pageSize;

		float _yPerc;
		float _xPerc;
		bool _vScroll;
		bool _hScroll;

		float _time1, _time2;
		float _y1, _y2;
		float _xOverlap, _yOverlap;
		float _x1, _x2;
		float _xOffset, _yOffset;
		bool _isMouseMoved;
		Vector2 _holdAreaPoint;
		bool _isHoldAreaDone;
		bool _aniFlag;
		bool _scrollBarVisible;
		int _touchId;

		ThrowTween _throwTween;
		Tweener _tweener;
		int _tweening;

		EventCallback0 _refreshDelegate;

		GComponent _owner;
		Container _container;
		Container _maskHolder;
		Container _maskContentHolder;
		GScrollBar _hzScrollBar;
		GScrollBar _vtScrollBar;

		public ScrollPane(GComponent owner,
									ScrollType scrollType,
									Margin scrollBarMargin,
									ScrollBarDisplayType scrollBarDisplay,
									int flags,
									string vtScrollBarRes,
									string hzScrollBarRes)
		{
			onScroll = new EventListener(this, "onScroll");
			_refreshDelegate = Refresh;

			_throwTween = new ThrowTween();
			_owner = owner;
			_container = _owner.rootContainer;

			_maskHolder = new Container();
			_container.AddChild(_maskHolder);

			_maskContentHolder = _owner.container;
			_maskContentHolder.x = 0;
			_maskContentHolder.y = 0;
			_maskHolder.AddChild(_maskContentHolder);

			_scrollBarMargin = scrollBarMargin;
			_scrollType = scrollType;
			_scrollSpeed = UIConfig.defaultScrollSpeed;
			_mouseWheelSpeed = _scrollSpeed * 2;
			_softnessOnTopOrLeftSide = UIConfig.allowSoftnessOnTopOrLeftSide;

			_displayOnLeft = (flags & 1) != 0;
			_snapToItem = (flags & 2) != 0;
			_displayInDemand = (flags & 4) != 0;
			_pageMode = (flags & 8) != 0;
			if ((flags & 16) != 0)
				_touchEffect = true;
			else if ((flags & 32) != 0)
				_touchEffect = false;
			else
				_touchEffect = UIConfig.defaultScrollTouchEffect;
			if ((flags & 64) != 0)
				_bouncebackEffect = true;
			else if ((flags & 128) != 0)
				_bouncebackEffect = false;
			else
				_bouncebackEffect = UIConfig.defaultScrollBounceEffect;

			_xPerc = 0;
			_yPerc = 0;
			_aniFlag = true;
			_scrollBarVisible = true;
			_mouseWheelEnabled = true;
			_holdAreaPoint = new Vector2();
			_pageSize = Vector2.one;

			if (scrollBarDisplay == ScrollBarDisplayType.Default)
			{
				if (Application.isMobilePlatform)
					scrollBarDisplay = ScrollBarDisplayType.Auto;
				else
					scrollBarDisplay = UIConfig.defaultScrollBarDisplay;
			}

			if (scrollBarDisplay != ScrollBarDisplayType.Hidden)
			{
				if (_scrollType == ScrollType.Both || _scrollType == ScrollType.Vertical)
				{
					string res = string.IsNullOrEmpty(vtScrollBarRes) ? UIConfig.verticalScrollBar : vtScrollBarRes;
					if (!string.IsNullOrEmpty(res))
					{
						_vtScrollBar = UIPackage.CreateObjectFromURL(res) as GScrollBar;
						if (_vtScrollBar == null)
							Debug.LogWarning("FairyGUI: cannot create scrollbar from " + res);
						else
						{
							_vtScrollBar.SetScrollPane(this, true);
							_container.AddChild(_vtScrollBar.displayObject);
						}
					}
				}
				if (_scrollType == ScrollType.Both || _scrollType == ScrollType.Horizontal)
				{
					string res = string.IsNullOrEmpty(hzScrollBarRes) ? UIConfig.horizontalScrollBar : hzScrollBarRes;
					if (!string.IsNullOrEmpty(res))
					{
						_hzScrollBar = UIPackage.CreateObjectFromURL(res) as GScrollBar;
						if (_hzScrollBar == null)
							Debug.LogWarning("FairyGUI: cannot create scrollbar from " + res);
						else
						{
							_hzScrollBar.SetScrollPane(this, false);
							_container.AddChild(_hzScrollBar.displayObject);
						}
					}
				}

				_scrollBarDisplayAuto = scrollBarDisplay == ScrollBarDisplayType.Auto;
				if (_scrollBarDisplayAuto)
				{
					if (_vtScrollBar != null)
						_vtScrollBar.displayObject.visible = false;
					if (_hzScrollBar != null)
						_hzScrollBar.displayObject.visible = false;
					_scrollBarVisible = false;

					_container.onRollOver.Add(__rollOver);
					_container.onRollOut.Add(__rollOut);
				}
			}
			else
				_mouseWheelEnabled = false;

			SetSize(owner.width, owner.height);

			_container.onMouseWheel.Add(__mouseWheel);
			_container.onTouchBegin.Add(__touchBegin);
		}

		/// <summary>
		/// 
		/// </summary>
		public GComponent owner
		{
			get { return _owner; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool bouncebackEffect
		{
			get { return _bouncebackEffect; }
			set { _bouncebackEffect = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool touchEffect
		{
			get { return _touchEffect; }
			set { _touchEffect = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool softnessOnTopOrLeftSide
		{
			get { return _softnessOnTopOrLeftSide; }
			set { _softnessOnTopOrLeftSide = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public float scrollSpeed
		{
			get { return _scrollSpeed; }
			set
			{
				_scrollSpeed = value;
				if (_scrollSpeed == 0)
					_scrollSpeed = UIConfig.defaultScrollSpeed;
				_mouseWheelSpeed = _scrollSpeed * 2;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool snapToItem
		{
			get { return _snapToItem; }
			set { _snapToItem = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool mouseWheelEnabled
		{
			get { return _mouseWheelEnabled; }
			set { _mouseWheelEnabled = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public float percX
		{
			get { return _xPerc; }
			set { SetPercX(value, false); }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="ani"></param>
		public void SetPercX(float value, bool ani)
		{
			if (value > 1)
				value = 1;
			else if (value < 0)
				value = 0;
			if (value != _xPerc)
			{
				_xPerc = value;
				PosChanged(ani);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float percY
		{
			get { return _yPerc; }
			set { SetPercY(value, false); }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="ani"></param>
		public void SetPercY(float value, bool ani)
		{
			if (value > 1)
				value = 1;
			else if (value < 0)
				value = 0;
			if (value != _yPerc)
			{
				_yPerc = value;
				PosChanged(ani);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float posX
		{
			get { return _xPerc * Mathf.Max(0, _contentWidth - _maskWidth); }
			set { SetPosX(value, false); }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="ani"></param>
		public void SetPosX(float value, bool ani)
		{
			if (_contentWidth > _maskWidth)
				this.SetPercX(value / (_contentWidth - _maskWidth), ani);
			else
				this.SetPercX(0, ani);
		}

		/// <summary>
		/// 
		/// </summary>
		public float posY
		{
			get { return _yPerc * Mathf.Max(0, _contentHeight - _maskHeight); }
			set { SetPosY(value, false); }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="ani"></param>
		public void SetPosY(float value, bool ani)
		{
			if (_contentHeight > _maskHeight)
				this.SetPercY(value / (_contentHeight - _maskHeight), ani);
			else
				this.SetPercY(0, ani);
		}

		/// <summary>
		/// 
		/// </summary>
		public bool isBottomMost
		{
			get { return _yPerc == 1 || _contentHeight <= _maskHeight; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool isRightMost
		{
			get { return _xPerc == 1 || _contentWidth <= _maskWidth; }
		}

		public int currentPageX
		{
			get { return _pageMode ? Mathf.FloorToInt(this.posX / _pageSize.x) : 0; }
			set
			{
				if (_hScroll)
					this.SetPosX(value * _pageSize.x, false);
			}
		}

		public int currentPageY
		{
			get { return _pageMode ? Mathf.FloorToInt(this.posY / _pageSize.y) : 0; }
			set
			{
				if (_vScroll)
					this.SetPosY(value * _pageSize.y, false);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float contentWidth
		{
			get
			{
				return _contentWidth;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float contentHeight
		{
			get
			{
				return _contentHeight;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float viewWidth
		{
			get { return _maskWidth; }
			set
			{
				value = value + _owner.margin.left + _owner.margin.right;
				if (_vtScrollBar != null)
					value += _vtScrollBar.width;
				_owner.width = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float viewHeight
		{
			get { return _maskHeight; }
			set
			{
				value = value + _owner.margin.top + _owner.margin.bottom;
				if (_hzScrollBar != null)
					value += _hzScrollBar.height;
				_owner.height = value;
			}
		}

		float GetDeltaX(float move)
		{
			return (_pageMode ? _pageSize.x : move) / (_contentWidth - _maskWidth);
		}

		float GetDeltaY(float move)
		{
			return (_pageMode ? _pageSize.y : move) / (_contentHeight - _maskHeight);
		}

		/// <summary>
		/// 
		/// </summary>
		public void ScrollTop()
		{
			ScrollTop(false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ani"></param>
		public void ScrollTop(bool ani)
		{
			this.SetPercY(0, ani);
		}

		/// <summary>
		/// 
		/// </summary>
		public void ScrollBottom()
		{
			ScrollBottom(false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ani"></param>
		public void ScrollBottom(bool ani)
		{
			this.SetPercY(1, ani);
		}

		/// <summary>
		/// 
		/// </summary>
		public void ScrollUp()
		{
			ScrollUp(1, false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="speed"></param>
		/// <param name="ani"></param>
		public void ScrollUp(float speed, bool ani)
		{
			this.SetPercY(_yPerc - GetDeltaY(_scrollSpeed * speed), ani);
		}

		/// <summary>
		/// 
		/// </summary>
		public void ScrollDown()
		{
			ScrollDown(1, false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="speed"></param>
		/// <param name="ani"></param>
		public void ScrollDown(float speed, bool ani)
		{
			this.SetPercY(_yPerc + GetDeltaY(_scrollSpeed * speed), ani);
		}

		/// <summary>
		/// 
		/// </summary>
		public void ScrollLeft()
		{
			ScrollLeft(1, false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="speed"></param>
		/// <param name="ani"></param>
		public void ScrollLeft(float speed, bool ani)
		{
			this.SetPercX(_xPerc - GetDeltaX(_scrollSpeed * speed), ani);
		}

		/// <summary>
		/// 
		/// </summary>
		public void ScrollRight()
		{
			ScrollRight(1, false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="speed"></param>
		/// <param name="ani"></param>
		public void ScrollRight(float speed, bool ani)
		{
			this.SetPercX(_xPerc + GetDeltaX(_scrollSpeed * speed), ani);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj">obj can be any object on stage, not limited to the direct child of this container.</param>
		public void ScrollToView(GObject obj)
		{
			ScrollToView(obj, false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj">obj can be any object on stage, not limited to the direct child of this container.</param>
		/// <param name="ani">If moving to target position with animation</param>
		public void ScrollToView(GObject obj, bool ani)
		{
			ScrollToView(obj, ani, false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj">obj can be any object on stage, not limited to the direct child of this container.</param>
		/// <param name="ani">If moving to target position with animation</param>
		/// <param name="setFirst">If true, scroll to make the target on the top/left; If false, scroll to make the target any position in view.</param>
		public void ScrollToView(GObject obj, bool ani, bool setFirst)
		{
			_owner.EnsureBoundsCorrect();
			if (_needRefresh)
				Refresh();

			Rect rect = new Rect(obj.x, obj.y, obj.width, obj.height);
			if (obj.parent != _owner)
				rect = obj.parent.TransformRect(rect, _owner);
			ScrollToView(rect, ani, setFirst);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rect">Rect in local coordinates</param>
		/// <param name="ani">If moving to target position with animation</param>
		/// <param name="setFirst">If true, scroll to make the target on the top/left; If false, scroll to make the target any position in view.</param>
		public void ScrollToView(Rect rect, bool ani, bool setFirst)
		{
			_owner.EnsureBoundsCorrect();
			if (_needRefresh)
				Refresh();

			if (_vScroll)
			{
				float top = this.posY;
				float bottom = top + _maskHeight;
				if (setFirst || rect.y <= top || rect.height >= _maskHeight)
				{
					if (_pageMode)
						this.SetPosY(Mathf.Floor(rect.y / _pageSize.y) * _pageSize.y, ani);
					else
						SetPosY(rect.y, ani);
				}
				else if (rect.y + rect.height > bottom)
				{
					if (_pageMode)
						this.SetPosY(Mathf.Floor(rect.y / _pageSize.y) * _pageSize.y, ani);
					else if (rect.height <= _maskHeight / 2)
						SetPosY(rect.y + rect.height * 2 - _maskHeight, ani);
					else
						SetPosY(rect.y + rect.height - _maskHeight, ani);
				}
			}
			if (_hScroll)
			{
				float left = this.posX;
				float right = left + _maskWidth;
				if (setFirst || rect.x <= left || rect.width >= _maskWidth)
				{
					if (_pageMode)
						this.SetPosX(Mathf.Floor(rect.x / _pageSize.x) * _pageSize.x, ani);
					SetPosX(rect.x, ani);
				}
				else if (rect.x + rect.width > right)
				{
					if (_pageMode)
						this.SetPosX(Mathf.Floor(rect.x / _pageSize.x) * _pageSize.x, ani);
					else if (rect.width <= _maskWidth / 2)
						SetPosX(rect.x + rect.width * 2 - _maskWidth, ani);
					else
						SetPosX(rect.x + rect.width - _maskWidth, ani);
				}
			}

			if (!ani && _needRefresh)
				Refresh();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj">obj must be the direct child of this container</param>
		/// <returns></returns>
		public bool IsChildInView(GObject obj)
		{
			if (_vScroll)
			{
				float dist = obj.y + _maskContentHolder.y;
				if (dist < -obj.height - 20 || dist > _maskHeight + 20)
					return false;
			}
			if (_hScroll)
			{
				float dist = obj.x + _maskContentHolder.x;
				if (dist < -obj.width - 20 || dist > _maskWidth + 20)
					return false;
			}

			return true;
		}

		internal void OnOwnerSizeChanged()
		{
			SetSize(_owner.width, _owner.height);
			PosChanged(false);
		}

		void SetSize(float aWidth, float aHeight)
		{
			if (_displayOnLeft && _vtScrollBar != null)
				_maskHolder.x = Mathf.FloorToInt(_owner.margin.left + _vtScrollBar.width);
			else
				_maskHolder.x = Mathf.FloorToInt(_owner.margin.left);
			_maskHolder.y = Mathf.FloorToInt(_owner.margin.top);

			if (_hzScrollBar != null)
			{
				_hzScrollBar.y = aHeight - _hzScrollBar.height;
				if (_vtScrollBar != null)
				{
					_hzScrollBar.width = aWidth - _vtScrollBar.width - _scrollBarMargin.left - _scrollBarMargin.right;
					if (_displayOnLeft)
						_hzScrollBar.x = _scrollBarMargin.left + _vtScrollBar.width;
					else
						_hzScrollBar.x = _scrollBarMargin.left;
				}
				else
				{
					_hzScrollBar.width = aWidth - _scrollBarMargin.left - _scrollBarMargin.right;
					_hzScrollBar.x = _scrollBarMargin.left;
				}
			}
			if (_vtScrollBar != null)
			{
				if (!_displayOnLeft)
					_vtScrollBar.x = aWidth - _vtScrollBar.width;
				if (_hzScrollBar != null)
					_vtScrollBar.height = aHeight - _hzScrollBar.height - _scrollBarMargin.top - _scrollBarMargin.bottom;
				else
					_vtScrollBar.height = aHeight - _scrollBarMargin.top - _scrollBarMargin.bottom;
				_vtScrollBar.y = _scrollBarMargin.top;
			}

			_maskWidth = aWidth;
			_maskHeight = aHeight;
			if (_hzScrollBar != null && !_hScrollNone)
				_maskHeight -= _hzScrollBar.height;
			if (_vtScrollBar != null && !_vScrollNone)
				_maskWidth -= _vtScrollBar.width;
			_maskWidth -= (_owner.margin.left + _owner.margin.right);
			_maskHeight -= (_owner.margin.top + _owner.margin.bottom);

			_maskWidth = Mathf.Max(1, _maskWidth);
			_maskHeight = Mathf.Max(1, _maskHeight);
			_pageSize = new Vector2(_maskWidth, _maskHeight);

			HandleSizeChanged();
		}

		internal void SetContentSize(float aWidth, float aHeight)
		{
			if (Mathf.Approximately(_contentWidth, aWidth) && Mathf.Approximately(_contentHeight, aHeight))
				return;

			_contentWidth = aWidth;
			_contentHeight = aHeight;
			HandleSizeChanged();
			_aniFlag = false;
			Refresh();
		}

		void HandleSizeChanged()
		{
			if (_displayInDemand)
			{
				if (_vtScrollBar != null)
				{
					if (_contentHeight <= _maskHeight)
					{
						if (!_vScrollNone)
						{
							_vScrollNone = true;
							_maskWidth += _vtScrollBar.width;
						}
					}
					else
					{
						if (_vScrollNone)
						{
							_vScrollNone = false;
							_maskWidth -= _vtScrollBar.width;
						}
					}
				}
				if (_hzScrollBar != null)
				{
					if (_contentWidth <= _maskWidth)
					{
						if (!_hScrollNone)
						{
							_hScrollNone = true;
							_maskHeight += _vtScrollBar.height;
						}
					}
					else
					{
						if (_hScrollNone)
						{
							_hScrollNone = false;
							_maskHeight -= _vtScrollBar.height;
						}
					}
				}
			}

			if (_vtScrollBar != null)
			{
				if (_maskHeight < _vtScrollBar.minSize)
					_vtScrollBar.displayObject.visible = false;
				else
				{
					_vtScrollBar.displayObject.visible = _scrollBarVisible && !_vScrollNone;
					if (_contentHeight == 0)
						_vtScrollBar.displayPerc = 0;
					else
						_vtScrollBar.displayPerc = Math.Min(1, _maskHeight / _contentHeight);
				}
			}
			if (_hzScrollBar != null)
			{
				if (_maskWidth < _hzScrollBar.minSize)
					_hzScrollBar.displayObject.visible = false;
				else
				{
					_hzScrollBar.displayObject.visible = _scrollBarVisible && !_hScrollNone;
					if (_contentWidth == 0)
						_hzScrollBar.displayPerc = 0;
					else
						_hzScrollBar.displayPerc = Math.Min(1, _maskWidth / _contentWidth);
				}
			}

			_maskHolder.clipRect = new Rect(0, 0, _maskWidth, _maskHeight);

			_xOverlap = Mathf.Ceil(Math.Max(0, _contentWidth - _maskWidth));
			_yOverlap = Mathf.Ceil(Math.Max(0, _contentHeight - _maskHeight));

			switch (_scrollType)
			{
				case ScrollType.Both:

					if (_contentWidth > _maskWidth && _contentHeight <= _maskHeight)
					{
						_hScroll = true;
						_vScroll = false;
					}
					else if (_contentWidth <= _maskWidth && _contentHeight > _maskHeight)
					{
						_hScroll = false;
						_vScroll = true;
					}
					else if (_contentWidth > _maskWidth && _contentHeight > _maskHeight)
					{
						_hScroll = true;
						_vScroll = true;
					}
					else
					{
						_hScroll = false;
						_vScroll = false;
					}
					break;

				case ScrollType.Vertical:

					if (_contentHeight > _maskHeight)
					{
						_hScroll = false;
						_vScroll = true;
					}
					else
					{
						_hScroll = false;
						_vScroll = false;
					}
					break;

				case ScrollType.Horizontal:

					if (_contentWidth > _maskWidth)
					{
						_hScroll = true;
						_vScroll = false;
					}
					else
					{
						_hScroll = false;
						_vScroll = false;
					}
					break;
			}
		}

		internal void UpdateClipSoft()
		{
			Vector2 softness = _owner.clipSoftness;
			if (softness.x != 0 || softness.y != 0)
			{
				_maskHolder.clipSoftness = new Vector4(
					//左边缘和上边缘感觉不需要效果，所以注释掉
					(_xPerc < 0.01 || !_softnessOnTopOrLeftSide) ? 0 : softness.x,
					(_yPerc < 0.01 || !_softnessOnTopOrLeftSide) ? 0 : softness.y,
					(!_hScroll || _xPerc > 0.99) ? 0 : softness.x,
					(!_vScroll || _yPerc > 0.99) ? 0 : softness.y);
			}
			else
				_maskHolder.clipSoftness = null;
		}

		private void PosChanged(bool ani)
		{
			if (_aniFlag)
				_aniFlag = ani;

			_needRefresh = true;

			UpdateContext.OnBegin -= _refreshDelegate;
			UpdateContext.OnBegin += _refreshDelegate;

			//如果在甩手指滚动过程中用代码重新设置滚动位置，要停止滚动
			if (_tweening == 2) //kill throw tween only
			{
				_tweener.Kill();
				_tweener = null;
				_tweening = 0;
			}
		}

		private void Refresh()
		{
			_needRefresh = false;
			UpdateContext.OnBegin -= _refreshDelegate;

			float contentXLoc = 0f;
			float contentYLoc = 0f;

			if (_hScroll)
				contentXLoc = _xPerc * (_contentWidth - _maskWidth);
			if (_vScroll)
				contentYLoc = _yPerc * (_contentHeight - _maskHeight);

			if (_pageMode)
			{
				int page;
				float delta;
				if (_vScroll && _yPerc != 1 && _yPerc != 0)
				{
					page = Mathf.FloorToInt(contentYLoc / _pageSize.y);
					delta = contentYLoc - page * _pageSize.y;
					if (delta > _pageSize.y / 2)
						page++;
					contentYLoc = page * _pageSize.y;
					if (contentYLoc > _contentHeight - _maskHeight)
					{
						contentYLoc = _contentHeight - _maskHeight;
						_yPerc = 1;
					}
					else
						_yPerc = contentYLoc / (_contentHeight - _maskHeight);
				}

				if (_hScroll && _xPerc != 1 && _xPerc != 0)
				{
					page = Mathf.FloorToInt(contentXLoc / _pageSize.x);
					delta = contentXLoc - page * _pageSize.x;
					if (delta > _pageSize.x / 2)
						page++;
					contentXLoc = page * _pageSize.x;
					if (contentXLoc > _contentWidth - _maskWidth)
					{
						contentXLoc = _contentWidth - _maskWidth;
						_xPerc = 1;
					}
					else
						_xPerc = contentXLoc / (_contentWidth - _maskWidth);
				}
			}
			else if (_snapToItem)
			{
				float tmpX = _xPerc == 1 ? 0 : contentXLoc;
				float tmpY = _yPerc == 1 ? 0 : contentYLoc;
				_owner.GetSnappingPosition(ref tmpX, ref tmpY);
				if (_xPerc != 1 && !Mathf.Approximately(tmpX, contentXLoc))
				{
					_xPerc = tmpX / (_contentWidth - _maskWidth);
					if (_xPerc > 1)
						_xPerc = 1;
					contentXLoc = _xPerc * (_contentWidth - _maskWidth);
				}
				if (_yPerc != 1 && !Mathf.Approximately(tmpY, contentYLoc))
				{
					_yPerc = tmpY / (_contentHeight - _maskHeight);
					if (_yPerc > 1)
						_yPerc = 1;
					contentYLoc = _yPerc * (_contentHeight - _maskHeight);
				}
			}

			Refresh2(contentXLoc, contentYLoc);

			onScroll.Call();
			if (_needRefresh) //user change scroll pos in on scroll
			{
				_needRefresh = false;
				UpdateContext.OnBegin -= _refreshDelegate;

				if (_hScroll)
					contentXLoc = _xPerc * (_contentWidth - _maskWidth);
				if (_vScroll)
					contentYLoc = _yPerc * (_contentHeight - _maskHeight);
				Refresh2(contentXLoc, contentYLoc);
			}

			_aniFlag = true;
		}

		void Refresh2(float contentXLoc, float contentYLoc)
		{
			contentXLoc = (int)contentXLoc;
			contentYLoc = (int)contentYLoc;

			if (_aniFlag && !_isMouseMoved)
			{
				float toX = _maskContentHolder.x;
				float toY = _maskContentHolder.y;

				if (_vScroll)
				{
					toY = -contentYLoc;
				}
				else
				{
					if (_maskContentHolder.y != 0)
						_maskContentHolder.y = 0;
				}
				if (_hScroll)
				{
					toX = -contentXLoc;
				}
				else
				{
					if (_maskContentHolder.x != 0)
						_maskContentHolder.x = 0;
				}

				if (toX != _maskContentHolder.x || toY != _maskContentHolder.y)
				{
					if (_tweener != null)
					{
						_tweening = 0;
						_tweener.Complete();
						_tweener = null;
					}
					_tweener = DOTween.To(() => _maskContentHolder.xy, v => _maskContentHolder.xy = v, new Vector2(toX, toY), 0.5f)
						.SetEase(Ease.OutCubic)
						.SetUpdate(true)
						.OnUpdate(__tweenUpdate)
						.OnComplete(__tweenComplete);
					_tweening = 1;
				}
			}
			else
			{
				if (_tweener != null)
				{
					_tweening = 0;
					_tweener.Complete();
					_tweener = null;
				}

				//如果在拖动的过程中Refresh，这里要进行处理，保证拖动继续正常进行
				if (_isMouseMoved)
				{
					_xOffset += _maskContentHolder.x - (-contentXLoc);
					_yOffset += _maskContentHolder.y - (-contentYLoc);
				}

				_maskContentHolder.SetXY(-contentXLoc, -contentYLoc);

				//如果在拖动的过程中Refresh，这里要进行处理，保证手指离开是滚动正常进行
				if (_isMouseMoved)
				{
					_y1 = _y2 = _maskContentHolder.y;
					_x1 = _x2 = _maskContentHolder.x;
				}

				if (_vtScrollBar != null)
					_vtScrollBar.scrollPerc = _yPerc;
				if (_hzScrollBar != null)
					_hzScrollBar.scrollPerc = _xPerc;

				UpdateClipSoft();
			}
		}

		private float CalcYPerc()
		{
			if (!_vScroll)
				return 0;

			float diff = _contentHeight - _maskHeight;
			float my = _maskContentHolder.y;
			float currY;
			if (my > 0)
				currY = 0;
			else if (-my > diff)
				currY = diff;
			else
				currY = -my;
			return currY / diff;
		}

		private float CalcXPerc()
		{
			if (!_hScroll)
				return 0;

			float diff = _contentWidth - _maskWidth;
			float mx = _maskContentHolder.x;
			float currX;
			if (mx > 0)
				currX = 0;
			else if (-mx > diff)
				currX = diff;
			else
				currX = -mx;

			return currX / diff;
		}

		private void OnScrolling()
		{
			if (_vtScrollBar != null)
			{
				_vtScrollBar.scrollPerc = CalcYPerc();
				if (_scrollBarDisplayAuto)
					ShowScrollBar(true);
			}
			if (_hzScrollBar != null)
			{
				_hzScrollBar.scrollPerc = CalcXPerc();
				if (_scrollBarDisplayAuto)
					ShowScrollBar(true);
			}

			UpdateClipSoft();
		}

		private void OnScrollEnd()
		{
			if (_vtScrollBar != null)
			{
				if (_scrollBarDisplayAuto)
					ShowScrollBar(false);
			}
			if (_hzScrollBar != null)
			{
				if (_scrollBarDisplayAuto)
					ShowScrollBar(false);
			}

			UpdateClipSoft();
		}

		private void __touchBegin(EventContext context)
		{
			if (!_touchEffect)
				return;

			InputEvent evt = context.inputEvent;
			_touchId = evt.touchId;
			Vector2 pt = _owner.GlobalToLocal(new Vector2(evt.x, evt.y));
			if (_tweener != null)
			{
				_tweening = 0;
				_tweener.Complete();
				_tweener = null;
				Stage.inst.CancelClick(_touchId);
			}

			_y1 = _y2 = _maskContentHolder.y;
			_yOffset = pt.y - _maskContentHolder.y;

			_x1 = _x2 = _maskContentHolder.x;
			_xOffset = pt.x - _maskContentHolder.x;

			_time1 = _time2 = Time.time;
			_holdAreaPoint.x = pt.x;
			_holdAreaPoint.y = pt.y;
			_isHoldAreaDone = false;
			_isMouseMoved = false;

			Stage.inst.onTouchMove.Add(__touchMove);
			Stage.inst.onTouchEnd.Add(__touchEnd);
		}

		private void __touchMove(EventContext context)
		{
			InputEvent evt = context.inputEvent;
			if (_touchId != evt.touchId)
				return;

			Vector2 pt = _owner.GlobalToLocal(new Vector2(evt.x, evt.y));
			if (float.IsNaN(pt.x))
				return;

			int sensitivity;
			if (Stage.touchScreen)
				sensitivity = UIConfig.touchScrollSensitivity;
			else
				sensitivity = 5;

			float diff;
			bool sv = false, sh = false, st = false;

			if (_scrollType == ScrollType.Vertical)
			{
				if (!_isHoldAreaDone)
				{
					diff = Mathf.Abs(_holdAreaPoint.y - pt.y);
					if (diff < sensitivity)
						return;
				}

				sv = true;
			}
			else if (_scrollType == ScrollType.Horizontal)
			{
				if (!_isHoldAreaDone)
				{
					diff = Mathf.Abs(_holdAreaPoint.x - pt.x);
					if (diff < sensitivity)
						return;
				}

				sh = true;
			}
			else
			{
				if (!_isHoldAreaDone)
				{
					diff = Mathf.Abs(_holdAreaPoint.y - pt.y);
					if (diff < sensitivity)
					{
						diff = Mathf.Abs(_holdAreaPoint.x - pt.x);
						if (diff < sensitivity)
							return;
					}
				}

				sv = sh = true;
			}

			float t = Time.time;
			if (t - _time2 > 0.05f)
			{
				_time2 = _time1;
				_time1 = t;
				st = true;
			}

			if (sv)
			{
				float y = pt.y - _yOffset;
				if (y > 0)
				{
					if (!_bouncebackEffect)
						_maskContentHolder.y = 0;
					else
						_maskContentHolder.y = (int)(y * 0.5);
				}
				else if (y < -_yOverlap)
				{
					if (!_bouncebackEffect)
						_maskContentHolder.y = -(int)_yOverlap;
					else
						_maskContentHolder.y = (int)((y - _yOverlap) * 0.5);
				}
				else
				{
					_maskContentHolder.y = y;
				}

				if (st)
				{
					_y2 = _y1;
					_y1 = _maskContentHolder.y;
				}

				_yPerc = CalcYPerc();
			}

			if (sh)
			{
				float x = pt.x - _xOffset;
				if (x > 0)
				{
					if (!_bouncebackEffect)
						_maskContentHolder.x = 0;
					else
						_maskContentHolder.x = (int)(x * 0.5);
				}
				else if (x < 0 - _xOverlap)
				{
					if (!_bouncebackEffect)
						_maskContentHolder.x = -(int)_xOverlap;
					else
						_maskContentHolder.x = (int)((x - _xOverlap) * 0.5);
				}
				else
				{

					_maskContentHolder.x = x;
				}

				if (st)
				{
					_x2 = _x1;
					_x1 = _maskContentHolder.x;
				}

				_xPerc = CalcXPerc();
			}

			_isHoldAreaDone = true;
			_isMouseMoved = true;
			OnScrolling();

			onScroll.Call();
		}

		private void __touchEnd(EventContext context)
		{
			if (!_touchEffect)
			{
				_isMouseMoved = false;
				return;
			}

			InputEvent evt = context.inputEvent;
			if (_touchId != evt.touchId)
				return;

			Stage.inst.onTouchMove.Remove(__touchMove);
			Stage.inst.onTouchEnd.Remove(__touchEnd);

			if (!_isMouseMoved)
				return;

			_isMouseMoved = false;
			float time = Time.time - _time2;
			if (time == 0)
				time = 0.001f;
			float yVelocity = (_maskContentHolder.y - _y2) / time;
			float xVelocity = (_maskContentHolder.x - _x2) / time;
			float duration = 0.3f;

			_throwTween.start.x = _maskContentHolder.x;
			_throwTween.start.y = _maskContentHolder.y;

			Vector2 change1, change2;
			float endX = 0, endY = 0;

			if (_scrollType == ScrollType.Both || _scrollType == ScrollType.Horizontal)
			{
				change1.x = ThrowTween.CalculateChange(xVelocity, duration);
				change2.x = 0;
				endX = _maskContentHolder.x + change1.x;

				if (_pageMode)
				{
					int page = Mathf.FloorToInt(-endX / _pageSize.x);
					float delta = -endX - page * _pageSize.x;
					//页面吸附策略
					if (change1.x > _pageSize.x)
					{
						//如果翻页数量超过1，则需要超过页面的一半，才能到下一页
						if (delta >= _pageSize.x / 2)
							page++;
					}
					else if (endX < _maskContentHolder.x)
					{
						if (delta >= _pageSize.x / 2)
							page++;
					}
					endX = -page * _pageSize.x;
					if (endX < _maskWidth - _contentWidth)
						endX = _maskWidth - _contentWidth;

					change1.x = endX - _maskContentHolder.x;
				}
			}
			else
				change1.x = change2.x = 0;

			if (_scrollType == ScrollType.Both || _scrollType == ScrollType.Vertical)
			{
				change1.y = ThrowTween.CalculateChange(yVelocity, duration);
				change2.y = 0;
				endY = _maskContentHolder.y + change1.y;

				if (_pageMode)
				{
					int page = Mathf.FloorToInt(-endY / _pageSize.y);
					float delta = -endY - page * _pageSize.y;
					//页面吸附策略
					if (change1.y > _pageSize.y)
					{
						//如果翻页数量超过1，则需要超过页面的一半，才能到下一页
						if (delta >= _pageSize.y / 2)
							page++;
					}
					else if (endY < _maskContentHolder.y)
					{
						if (delta >= _pageSize.y / 2)
							page++;
					}
					endY = -page * _pageSize.y;
					if (endY < _maskHeight - _contentHeight)
						endY = _maskHeight - _contentHeight;

					change1.y = endY - _maskContentHolder.y;
				}
			}
			else
				change1.y = change2.y = 0;

			if (_snapToItem && !_pageMode)
			{
				endX = -endX;
				endY = -endY;
				_owner.GetSnappingPosition(ref endX, ref endY);
				endX = -endX;
				endY = -endY;
				change1.x = endX - _maskContentHolder.x;
				change1.y = endY - _maskContentHolder.y;
			}

			if (_bouncebackEffect)
			{
				if (endX > 0)
					change2.x = 0 - _maskContentHolder.x - change1.x;
				else if (endX < -_xOverlap)
					change2.x = -_xOverlap - _maskContentHolder.x - change1.x;

				if (endY > 0)
					change2.y = 0 - _maskContentHolder.y - change1.y;
				else if (endY < -_yOverlap)
					change2.y = -_yOverlap - _maskContentHolder.y - change1.y;
			}
			else
			{
				if (endX > 0)
					change1.x = 0 - _maskContentHolder.x;
				else if (endX < -_xOverlap)
					change1.x = -_xOverlap - _maskContentHolder.x;

				if (endY > 0)
					change1.y = 0 - _maskContentHolder.y;
				else if (endY < -_yOverlap)
					change1.y = -_yOverlap - _maskContentHolder.y;
			}

			_throwTween.value = 0;
			_throwTween.change1 = change1;
			_throwTween.change2 = change2;

			if (_tweener != null)
			{
				_tweening = 0;
				_tweener.Complete();
			}

			_tweening = 2;
			_tweener = DOTween.To(() => _throwTween.value, v => _throwTween.value = v, 1, duration)
				.SetEase(Ease.OutCubic)
				.SetUpdate(true)
				.OnUpdate(__tweenUpdate2)
				.OnComplete(__tweenComplete2);
		}

		private void __mouseWheel(EventContext context)
		{
			if (!_mouseWheelEnabled)
				return;

			InputEvent evt = context.inputEvent;
			int delta = evt.mouseWheelDelta;
			if (_hScroll && !_vScroll)
			{
				if (delta < 0)
					this.SetPercX(_xPerc - GetDeltaX(_mouseWheelSpeed), false);
				else
					this.SetPercX(_xPerc + GetDeltaX(_mouseWheelSpeed), false);
			}
			else
			{
				if (delta < 0)
					this.SetPercY(_yPerc - GetDeltaY(_mouseWheelSpeed), false);
				else
					this.SetPercY(_yPerc + GetDeltaY(_mouseWheelSpeed), false);
			}
		}

		private void __rollOver(object e)
		{
			ShowScrollBar(true);
		}

		private void __rollOut(object e)
		{
			ShowScrollBar(false);
		}

		private void ShowScrollBar(bool val)
		{
			if (val)
			{
				__showScrollBar(true);
				Timers.inst.Remove(__showScrollBar);
			}
			else
				Timers.inst.Add(0.5f, 1, __showScrollBar, val);
		}

		private void __showScrollBar(object obj)
		{
			_scrollBarVisible = (bool)obj && _maskWidth > 0 && _maskHeight > 0;
			if (_vtScrollBar != null)
				_vtScrollBar.displayObject.visible = _scrollBarVisible && !_vScrollNone;
			if (_hzScrollBar != null)
				_hzScrollBar.displayObject.visible = _scrollBarVisible && !_hScrollNone;
		}

		private void __tweenUpdate()
		{
			OnScrolling();
		}

		private void __tweenComplete()
		{
			_tweener = null;
			_tweening = 0;
			OnScrollEnd();
		}

		private void __tweenUpdate2()
		{
			_throwTween.Update(_maskContentHolder);

			if (_scrollType == ScrollType.Vertical)
				_yPerc = CalcYPerc();
			else if (_scrollType == ScrollType.Horizontal)
				_xPerc = CalcXPerc();
			else
			{
				_yPerc = CalcYPerc();
				_xPerc = CalcXPerc();
			}

			OnScrolling();
			onScroll.Call();
		}

		private void __tweenComplete2()
		{
			if (_tweening == 0)
				return;

			_tweener = null;
			_tweening = 0;

			if (_scrollType == ScrollType.Vertical)
				_yPerc = CalcYPerc();
			else if (_scrollType == ScrollType.Horizontal)
				_xPerc = CalcXPerc();
			else
			{
				_yPerc = CalcYPerc();
				_xPerc = CalcXPerc();
			}

			_isMouseMoved = false;
			OnScrollEnd();
			onScroll.Call();
		}

		class ThrowTween
		{
			public float value;
			public Vector2 start;
			public Vector2 change1, change2;

			const float checkpoint = 0.05f;

			public void Update(DisplayObject obj)
			{
				obj.SetXY((int)(start.x + change1.x * value + change2.x * value * value), (int)(start.y + change1.y * value + change2.y * value * value));
			}

			static public float CalculateChange(float velocity, float duration)
			{
				return (duration * checkpoint * velocity) / easeOutCubic(checkpoint, 0, 1, 1);
			}

			static float easeOutCubic(float t, float b, float c, float d)
			{
				return c * ((t = t / d - 1) * t * t + 1) + b;
			}
		}
	}
}
