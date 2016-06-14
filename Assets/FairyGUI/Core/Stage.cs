﻿using System.Collections.Generic;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class Stage : Container
	{
		/// <summary>
		/// 
		/// </summary>
		public static bool touchScreen { get; private set; }

		internal static bool shiftDown { get; private set; }

		internal static bool textRebuildFlag;

		/// <summary>
		/// 
		/// </summary>
		public int stageHeight { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public int stageWidth { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public float soundVolume { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onStageResized { get; private set; }


		/// <summary>
		/// 
		/// </summary>
		public EventListener onCopy { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onPaste { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onTouchMove { get; private set; }

		internal InputCaret inputCaret { get; private set; }
		internal Highlighter highlighter { get; private set; }

		DisplayObject _touchTarget;
		DisplayObject _focused;
		UpdateContext _updateContext;
		List<DisplayObject> _rollOutChain;
		List<DisplayObject> _rollOverChain;
		TouchInfo[] _touches;
		int _touchCount;
		Vector2 _touchPosition;
		int _frameGotHitTarget;
		int _frameGotTouchPosition;
		bool _customInput;
		Vector2 _customInputPos;
		bool _customInputButtonDown;

		AudioSource _audio;

		List<NTexture> _toCollectTextures = new List<NTexture>();

		static Stage _inst;
		/// <summary>
		/// 
		/// </summary>
		public static Stage inst
		{
			get
			{
				if (_inst == null)
					Instantiate();

				return _inst;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static void Instantiate()
		{
			if (_inst == null)
			{
				_inst = new Stage();
				GRoot._inst = new GRoot();
				_inst.AddChild(GRoot._inst.displayObject);

				StageCamera.CheckMainCamera();
			}
		}

		public Stage()
			: base()
		{
			_inst = this;
			soundVolume = 1;

			_updateContext = new UpdateContext();
			stageWidth = Screen.width;
			stageHeight = Screen.height;
			_frameGotHitTarget = -1;

			touchScreen = Input.touchSupported;

			_touches = new TouchInfo[5];
			for (int i = 0; i < _touches.Length; i++)
				_touches[i] = new TouchInfo();

			if (!touchScreen)
				_touches[0].touchId = 0;

			_rollOutChain = new List<DisplayObject>();
			_rollOverChain = new List<DisplayObject>();

			onStageResized = new EventListener(this, "onStageResized");
			onTouchMove = new EventListener(this, "onTouchMove");
			onCopy = new EventListener(this, "onCopy");
			onPaste = new EventListener(this, "onPaste");

			StageEngine engine = GameObject.FindObjectOfType<StageEngine>();
			if (engine != null)
				this.gameObject = engine.gameObject;
			else
			{
				int layer = LayerMask.NameToLayer(StageCamera.LayerName);

				this.gameObject = new GameObject("Stage");
				this.gameObject.hideFlags = HideFlags.None;
				this.gameObject.layer = layer;
				this.gameObject.AddComponent<StageEngine>();
				this.gameObject.AddComponent<UIContentScaler>();
			}
			this.cachedTransform = gameObject.transform;
			this.cachedTransform.localScale = new Vector3(StageCamera.UnitsPerPixel, StageCamera.UnitsPerPixel, StageCamera.UnitsPerPixel);
			this.gameObject.SetActive(true);
			UnityEngine.Object.DontDestroyOnLoad(this.gameObject);

			EnableSound();

			inputCaret = new InputCaret();
			highlighter = new Highlighter();

			Timers.inst.Add(5, 0, RunTextureCollector);

#if UNITY_WEBPLAYER || UNITY_WEBGL || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR
			CopyPastePatch.Apply();
#endif
		}

		/// <summary>
		/// 
		/// </summary>
		public DisplayObject touchTarget
		{
			get
			{
				if (_frameGotHitTarget != Time.frameCount)
					GetHitTarget();

				if (_touchTarget == this)
					return null;
				else
					return _touchTarget;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static bool isTouchOnUI
		{
			get
			{
				return _inst != null && _inst.touchTarget != null;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public DisplayObject focus
		{
			get
			{
				if (_focused != null && _focused.isDisposed)
					_focused = null;
				return _focused;
			}
			set
			{
				if (_focused == value)
					return;

				if (_focused != null)
				{
					if ((_focused is TextField))
						((TextField)_focused).onFocusOut.Call();

					_focused.onRemovedFromStage.RemoveCapture(OnFocusRemoved);
				}

				_focused = value;
				if (_focused == this)
					_focused = null;
				if (_focused != null)
				{
					if ((_focused is TextField))
						((TextField)_focused).onFocusIn.Call();

					_focused.onRemovedFromStage.AddCapture(OnFocusRemoved);
				}
			}
		}

		void OnFocusRemoved(EventContext context)
		{
			if (_focused == null)
				return;

			if (context.sender == _focused)
				this.focus = null;
			else
			{
				DisplayObject currentObject = _focused.parent;
				while (currentObject != null)
				{
					if (currentObject == context.sender)
					{
						this.focus = null;
						break;
					}
					currentObject = currentObject.parent;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector2 touchPosition
		{
			get
			{
				UpdateTouchPosition();
				return _touchPosition;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="touchId"></param>
		/// <returns></returns>
		public Vector2 GetTouchPosition(int touchId)
		{
			UpdateTouchPosition();

			if (touchId < 0)
				return _touchPosition;

			for (int j = 0; j < 5; j++)
			{
				TouchInfo touch = _touches[j];
				if (touch.touchId == touchId)
					return new Vector2(touch.x, touch.y);
			}

			return _touchPosition;
		}

		/// <summary>
		/// 
		/// </summary>
		public int touchCount
		{
			get { return _touchCount; }
		}

		public int[] GetAllTouch(int[] result)
		{
			if (result == null)
				result = new int[_touchCount];
			int i = 0;
			for (int j = 0; j < 5; j++)
			{
				TouchInfo touch = _touches[j];
				if (touch.touchId != -1)
				{
					result[i++] = touch.touchId;
					if (i >= result.Length)
						break;
				}
			}
			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		public void ResetInputState()
		{
			for (int j = 0; j < 5; j++)
				_touches[j].Reset();

			if (!touchScreen)
				_touches[0].touchId = 0;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="touchId"></param>
		public void CancelClick(int touchId)
		{
			for (int j = 0; j < 5; j++)
			{
				TouchInfo touch = _touches[j];
				if (touch.touchId == touchId)
					touch.clickCancelled = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void EnableSound()
		{
			if (_audio == null)
			{
				_audio = gameObject.AddComponent<AudioSource>();
				_audio.bypassEffects = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void DisableSound()
		{
			if (_audio != null)
			{
				Object.DestroyObject(_audio);
				_audio = null;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="clip"></param>
		/// <param name="volumeScale"></param>
		public void PlayOneShotSound(AudioClip clip, float volumeScale)
		{
			if (_audio != null && this.soundVolume > 0)
				_audio.PlayOneShot(clip, volumeScale * this.soundVolume);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="clip"></param>
		public void PlayOneShotSound(AudioClip clip)
		{
			if (_audio != null && this.soundVolume > 0)
				_audio.PlayOneShot(clip, this.soundVolume);
		}

		public void SetCustomInput(Vector2 screenPos, bool buttonDown)
		{
			_customInput = true;
			_customInputButtonDown = buttonDown;
			_customInputPos = screenPos;
			_frameGotHitTarget = 0;
		}

		public void SetCustomInput(ref RaycastHit hit, bool buttonDown)
		{
			Vector2 screenPos = Camera.main.WorldToScreenPoint(hit.point);
			HitTestContext.CacheRaycastHit(Camera.main, ref hit);
			SetCustomInput(screenPos, buttonDown);
		}

		internal int InternalUpdate()
		{
			HandleEvents();

			_updateContext.Begin();
			Update(_updateContext);
			_updateContext.End();

			if (textRebuildFlag)
			{
				//字体贴图更改了，重新渲染一遍，防止本帧文字显示错误
				_updateContext.Begin();
				Update(_updateContext);
				_updateContext.End();

				textRebuildFlag = false;
			}

			return _updateContext.counter;
		}

		void GetHitTarget()
		{
			if (_frameGotHitTarget == Time.frameCount)
				return;

			_frameGotHitTarget = Time.frameCount;

			if (_customInput)
			{
				Vector2 pos = _customInputPos;
				pos.y = stageHeight - pos.y;

				TouchInfo touch = _touches[0];
				if (touch.x != pos.x || touch.y != pos.y
					|| _customInputButtonDown)
				{
					_touchTarget = HitTest(pos);
					touch.target = _touchTarget;
				}
			}
			else if (touchScreen)
			{
				for (int i = 0; i < Input.touchCount; ++i)
				{
					Touch uTouch = Input.GetTouch(i);
					if (uTouch.phase == TouchPhase.Stationary)
						continue;

					Vector2 pos = uTouch.position;
					pos.y = stageHeight - pos.y;

					TouchInfo touch = null;
					for (int j = 0; j < 5; j++)
					{
						if (_touches[j].touchId == uTouch.fingerId)
						{
							touch = _touches[j];
							break;
						}

						if (_touches[j].touchId == -1)
						{
							touch = _touches[j];
							//下面的赋值避免了touchMove在touchDown前触发
							touch.x = uTouch.position.x;
							touch.y = stageHeight - uTouch.position.y;
						}
					}
					if (touch == null)
						return;

					touch.touchId = uTouch.fingerId;
					_touchTarget = HitTest(pos);
					touch.target = _touchTarget;
				}
			}
			else
			{
				Vector2 pos = Input.mousePosition;
				pos.y = stageHeight - pos.y;

				TouchInfo touch = _touches[0];
				if (pos.x < 0 || pos.y < 0)
				{
					pos.x = touch.x;
					pos.y = touch.y;
				}

				if (touch.x != pos.x || touch.y != pos.y
					|| Input.GetMouseButtonDown(0)
					|| Input.GetMouseButtonUp(0)
					|| Input.GetMouseButtonDown(1))
				{
					_touchTarget = HitTest(pos);
					touch.target = _touchTarget;
				}
			}

			HitTestContext.ClearRaycastHitCache();
		}

		internal void HandleScreenSizeChanged()
		{
			stageWidth = Screen.width;
			stageHeight = Screen.height;

			this.cachedTransform.localScale = new Vector3(StageCamera.UnitsPerPixel, StageCamera.UnitsPerPixel, StageCamera.UnitsPerPixel);

			UIContentScaler scaler = this.gameObject.GetComponent<UIContentScaler>();
			scaler.ApplyChange();
			GRoot.inst.ApplyContentScaleFactor();

			onStageResized.Call();
		}

		internal void HandleGUIEvents(Event evt)
		{
			if (evt.rawType == EventType.KeyDown && evt.keyCode != KeyCode.None)
			{
				TouchInfo touch = _touches[0];
				touch.keyCode = evt.keyCode;
				touch.modifiers = evt.modifiers;

				touch.UpdateEvent();
				DisplayObject f = this.focus;
				if (f != null)
					f.onKeyDown.BubbleCall(touch.evt);
				else
					this.onKeyDown.Call(touch.evt);
			}
			else if (evt.rawType == EventType.KeyUp)
			{
				TouchInfo touch = _touches[0];
				touch.modifiers = evt.modifiers;
			}
			else if (evt.type == EventType.scrollWheel)
			{
				if (_touchTarget != null)
				{
					TouchInfo touch = _touches[0];
					touch.mouseWheelDelta = (int)evt.delta.y;
					touch.UpdateEvent();
					_touchTarget.onMouseWheel.BubbleCall(touch.evt);
				}
			}
		}

		void HandleEvents()
		{
			GetHitTarget();

			if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
				shiftDown = false;
			else if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
				shiftDown = true;

			UpdateTouchPosition();

			if (_customInput)
			{
				HandleCustomInput();
				_customInput = false;
			}
			else if (touchScreen)
				HandleTouchEvents();
			else
				HandleMouseEvents();
		}

		void UpdateTouchPosition()
		{
			if (_frameGotTouchPosition != Time.frameCount)
			{
				_frameGotTouchPosition = Time.frameCount;
				if (_customInput)
				{
					_touchPosition = _customInputPos;
					_touchPosition.y = stageHeight - _touchPosition.y;
				}
				else if (touchScreen)
				{
					for (int i = 0; i < Input.touchCount; ++i)
					{
						Touch uTouch = Input.GetTouch(i);
						_touchPosition = uTouch.position;
						_touchPosition.y = stageHeight - _touchPosition.y;
					}
				}
				else
				{
					Vector2 pos = Input.mousePosition;
					if (pos.x >= 0 && pos.y >= 0) //编辑器环境下坐标有时是负
					{
						pos.y = stageHeight - pos.y;
						_touchPosition = pos;
					}
				}
			}
		}

		void HandleCustomInput()
		{
			Vector2 pos = _customInputPos;
			pos.y = stageHeight - pos.y;
			TouchInfo touch = _touches[0];

			if (touch.x != pos.x || touch.y != pos.y)
			{
				touch.x = pos.x;
				touch.y = pos.y;
				touch.UpdateEvent();
				onTouchMove.Call(touch.evt);

				if (touch.lastRollOver != touch.target)
					HandleRollOver(touch);
			}

			if (_customInputButtonDown)
			{
				if (!touch.began)
				{
					touch.began = true;
					_touchCount++;
					touch.clickCancelled = false;
					touch.downX = touch.x;
					touch.downY = touch.y;
					this.focus = touch.target;

					if (touch.target != null)
					{
						touch.UpdateEvent();
						touch.target.onTouchBegin.BubbleCall(touch.evt);
					}
				}
			}
			else if (touch.began)
			{
				touch.began = false;
				_touchCount--;

				if (touch.target != null)
				{
					touch.UpdateEvent();
					touch.target.onTouchEnd.BubbleCall(touch.evt);

					if (!touch.clickCancelled && Mathf.Abs(touch.x - touch.downX) < 50 && Mathf.Abs(touch.y - touch.downY) < 50)
					{
						if (Time.realtimeSinceStartup - touch.lastClickTime < 0.35f)
						{
							if (touch.clickCount == 2)
								touch.clickCount = 1;
							else
								touch.clickCount++;
						}
						else
							touch.clickCount = 1;
						touch.lastClickTime = Time.realtimeSinceStartup;
						touch.UpdateEvent();
						touch.target.onClick.BubbleCall(touch.evt);
					}
				}
			}
		}

		void HandleMouseEvents()
		{
			TouchInfo touch = _touches[0];
			if (touch.x != _touchPosition.x || touch.y != _touchPosition.y)
			{
				touch.x = _touchPosition.x;
				touch.y = _touchPosition.y;
				touch.UpdateEvent();
				onTouchMove.Call(touch.evt);

				if (touch.lastRollOver != touch.target)
					HandleRollOver(touch);
			}

			if (Input.GetMouseButtonDown(0))
			{
				if (!touch.began)
				{
					touch.began = true;
					_touchCount++;
					touch.clickCancelled = false;
					touch.downX = touch.x;
					touch.downY = touch.y;
					this.focus = touch.target;

					if (touch.target != null)
					{
						touch.UpdateEvent();
						touch.target.onTouchBegin.BubbleCall(touch.evt);
					}
				}
			}
			if (Input.GetMouseButtonUp(0))
			{
				if (touch.began)
				{
					touch.began = false;
					_touchCount--;

					if (touch.target != null)
					{
						touch.UpdateEvent();
						touch.target.onTouchEnd.BubbleCall(touch.evt);

						if (!touch.clickCancelled && Mathf.Abs(touch.x - touch.downX) < 50 && Mathf.Abs(touch.y - touch.downY) < 50)
						{
							if (Time.realtimeSinceStartup - touch.lastClickTime < 0.35f)
							{
								if (touch.clickCount == 2)
									touch.clickCount = 1;
								else
									touch.clickCount++;
							}
							else
								touch.clickCount = 1;
							touch.lastClickTime = Time.realtimeSinceStartup;
							touch.UpdateEvent();
							touch.target.onClick.BubbleCall(touch.evt);
						}
					}
				}
			}
			if (Input.GetMouseButtonUp(1))
			{
				if (touch.target != null)
				{
					touch.UpdateEvent();
					touch.target.onRightClick.BubbleCall(touch.evt);
				}
			}
		}

		void HandleTouchEvents()
		{
			for (int i = 0; i < Input.touchCount; ++i)
			{
				Touch uTouch = Input.GetTouch(i);

				if (uTouch.phase == TouchPhase.Stationary)
					continue;

				Vector2 pos = uTouch.position;
				pos.y = stageHeight - pos.y;

				TouchInfo touch = null;
				for (int j = 0; j < 5; j++)
				{
					if (_touches[j].touchId == uTouch.fingerId)
					{
						touch = _touches[j];
						break;
					}
				}
				if (touch == null)
					continue;

				if (touch.x != pos.x || touch.y != pos.y)
				{
					touch.x = pos.x;
					touch.y = pos.y;
					touch.UpdateEvent();
					onTouchMove.Call(touch.evt);

					//no rollover/rollout on mobile
				}

				if (uTouch.phase == TouchPhase.Began)
				{
					if (!touch.began)
					{
						touch.began = true;
						_touchCount++;
						touch.clickCancelled = false;
						touch.downX = touch.x;
						touch.downY = touch.y;
						this.focus = touch.target;

						if (touch.target != null)
						{
							touch.UpdateEvent();
							touch.target.onTouchBegin.BubbleCall(touch.evt);
						}
					}
				}
				else if (uTouch.phase == TouchPhase.Canceled || uTouch.phase == TouchPhase.Ended)
				{
					if (touch.began)
					{
						touch.began = false;
						_touchCount--;

						if (touch.target != null)
						{
							touch.UpdateEvent();
							touch.target.onTouchEnd.BubbleCall(touch.evt);

							if (!touch.clickCancelled && Mathf.Abs(touch.x - touch.downX) < 50 && Mathf.Abs(touch.y - touch.downY) < 50)
							{
								touch.clickCount = uTouch.tapCount;
								touch.UpdateEvent();
								touch.target.onClick.BubbleCall(touch.evt);
							}
						}
					}

					touch.Reset();
				}
			}
		}

		void HandleRollOver(TouchInfo touch)
		{
			DisplayObject element;
			element = touch.lastRollOver;
			while (element != null)
			{
				_rollOutChain.Add(element);
				element = element.parent;
			}

			touch.lastRollOver = touch.target;

			element = touch.target;
			int i;
			while (element != null)
			{
				i = _rollOutChain.IndexOf(element);
				if (i != -1)
				{
					_rollOutChain.RemoveRange(i, _rollOutChain.Count - i);
					break;
				}
				_rollOverChain.Add(element);

				element = element.parent;
			}

			int cnt = _rollOutChain.Count;
			if (cnt > 0)
			{
				for (i = 0; i < cnt; i++)
				{
					element = _rollOutChain[i];
					if (element.stage != null)
						element.onRollOut.Call();
				}
				_rollOutChain.Clear();
			}

			cnt = _rollOverChain.Count;
			if (cnt > 0)
			{
				for (i = 0; i < cnt; i++)
				{
					element = _rollOverChain[i];
					if (element.stage != null)
						element.onRollOver.Call();
				}
				_rollOverChain.Clear();
			}
		}

		/// <summary>
		/// Adjust display order of all UIPanels rendering in worldspace by their z order.
		/// </summary>
		/// <param name="panelSortingOrder">Only UIPanel.sortingOrder equals to this value will be involve in this sorting</param>
		public void SortWorldSpacePanelsByZOrder(int panelSortingOrder)
		{
			if (sTempList1 == null)
			{
				sTempList1 = new List<DisplayObject>();
				sTempList2 = new List<int>();
			}

			int numChildren = Stage.inst.numChildren;
			for (int i = 0; i < numChildren; i++)
			{
				Container obj = Stage.inst.GetChildAt(i) as Container;
				if (obj == null || obj.renderMode != RenderMode.WorldSpace || obj._panelOrder != panelSortingOrder)
					continue;

				//借用一下tmpBounds
				obj._internal_bounds.x = obj.cachedTransform.position.z;
				obj._internal_bounds.y = i;

				sTempList1.Add(obj);
				sTempList2.Add(i);
			}

			sTempList1.Sort(CompareZ);

			ChangeChildrenOrder(sTempList2, sTempList1);

			sTempList1.Clear();
			sTempList2.Clear();
		}

		static List<DisplayObject> sTempList1;
		static List<int> sTempList2;
		static int CompareZ(DisplayObject c1, DisplayObject c2)
		{
			int ret = ((Container)c2)._internal_bounds.x.CompareTo(((Container)c1)._internal_bounds.x);
			if (ret == 0)
			{
				//如果大家z值一样，使用原来的顺序，防止不停交换顺序（闪烁）
				return c1._internal_bounds.y.CompareTo(c2._internal_bounds.y);
			}
			else
				return ret;
		}

		public void MonitorTexture(NTexture texture)
		{
			if (_toCollectTextures.IndexOf(texture) == -1)
				_toCollectTextures.Add(texture);
		}

		void RunTextureCollector(object param)
		{
			int cnt = _toCollectTextures.Count;
			float curTime = Time.time;
			int i = 0;
			while (i < cnt)
			{
				NTexture texture = _toCollectTextures[i];
				if (texture.disposed)
				{
					_toCollectTextures.RemoveAt(i);
					cnt--;
				}
				else if (curTime - texture.lastActive > 5)
				{
					texture.Dispose();
					_toCollectTextures.RemoveAt(i);
					cnt--;
				}
				else
					i++;
			}
		}
	}

	class TouchInfo
	{
		public float x;
		public float y;
		public int touchId;
		public int clickCount;
		public KeyCode keyCode;
		public EventModifiers modifiers;
		public int mouseWheelDelta;

		public float downX;
		public float downY;
		public bool began;
		public bool clickCancelled;
		public float lastClickTime;
		public DisplayObject target;
		public DisplayObject lastRollOver;

		public InputEvent evt;

		public TouchInfo()
		{
			evt = new InputEvent();
			Reset();
		}

		public void Reset()
		{
			touchId = -1;
			x = 0;
			y = 0;
			clickCount = 0;
			keyCode = KeyCode.None;
			modifiers = 0;
			mouseWheelDelta = 0;
			lastClickTime = 0;
			began = false;
			target = null;
			lastRollOver = null;
			clickCancelled = false;
		}

		public void UpdateEvent()
		{
			evt.touchId = this.touchId;
			evt.x = this.x;
			evt.y = this.y;
			evt.clickCount = this.clickCount;
			evt.keyCode = this.keyCode;
			evt.modifiers = this.modifiers;
			evt.mouseWheelDelta = this.mouseWheelDelta;
		}
	}
}