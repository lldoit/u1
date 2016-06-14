﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 长按手势。当按下一定时间后(duration)，派发onAction，如果once为false，则间隔duration时间持续派发onAction，直到手指释放。
	/// </summary>
	public class LongPressGesture : EventDispatcher
	{
		/// <summary>
		/// 当手指按下时派发该事件。
		/// </summary>
		public EventListener onBegin { get; private set; }
		/// <summary>
		/// 手指离开屏幕时派发该事件。
		/// </summary>
		public EventListener onEnd { get; private set; }
		/// <summary>
		/// 当手指按下后一段时间后派发该事件。并且在手指离开前按一定周期派发该事件。
		/// </summary>
		public EventListener onAction { get; private set; }

		/// <summary>
		/// 第一次派发事件的触发时间。单位秒
		/// </summary>
		public float trigger;

		/// <summary>
		/// 派发onAction事件的时间间隔。单位秒。
		/// </summary>
		public float interval;

		/// <summary>
		/// 如果为真，则onAction再一次按下释放过程只派发一次。如果为假，则每隔duration时间派发一次。
		/// </summary>
		public bool once;

		GObject _host;
		Vector2 _startPoint;
		bool _started;

		public static float TRIGGER = 1.5f;
		public static float INTERVAL = 1f;

		public LongPressGesture(GObject host)
		{
			_host = host;
			trigger = TRIGGER;
			interval = INTERVAL;
			Enable(true);

			onBegin = new EventListener(this, "onLongPressBegin");
			onEnd = new EventListener(this, "onLongPressEnd");
			onAction = new EventListener(this, "onLongPressAction");
		}

		public void Dispose()
		{
			Enable(false);
			_host = null;
		}

		public void Enable(bool value)
		{
			if (value)
				_host.onTouchBegin.Add(__touchBegin);
			else
			{
				_host.onTouchBegin.Remove(__touchBegin);
				Stage.inst.onTouchEnd.Remove(__touchEnd);
				Timers.inst.Remove(__timer);
			}
		}

		public void Cancel()
		{
			Stage.inst.onTouchEnd.Remove(__touchEnd);
			Timers.inst.Remove(__timer);
		}

		void __touchBegin(EventContext context)
		{
			if (Stage.inst.touchCount > 1)
			{
				Timers.inst.Remove(__timer);
				return;
			}

			InputEvent evt = context.inputEvent;
			_startPoint = _host.GlobalToLocal(new Vector2(evt.x, evt.y));
			_started = false;

			Timers.inst.Add(trigger, 1, __timer);
			Stage.inst.onTouchEnd.Add(__touchEnd);
		}

		void __timer(object param)
		{
			Vector2 pt = Stage.inst.touchPosition;
			pt = _host.GlobalToLocal(pt) - _startPoint;
			if (Mathf.Abs(pt.x) > UIConfig.touchDragSensitivity || Mathf.Abs(pt.y) > UIConfig.touchDragSensitivity)
			{
				Stage.inst.onTouchEnd.Remove(__touchEnd);
				Timers.inst.Remove(__timer);
				return;
			}

			onAction.Call();
			if (!_started && !once)
			{
				_started = true;
				Timers.inst.Add(interval, 0, __timer);
			}
		}

		void __touchEnd(EventContext context)
		{
			Stage.inst.onTouchEnd.Remove(__touchEnd);
			Timers.inst.Remove(__timer);
		}
	}
}
