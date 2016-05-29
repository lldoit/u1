using System.Collections.Generic;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class MovieClip : DisplayObject
	{
		public struct Frame
		{
			public Rect rect;
			public float addDelay;
			public Rect uvRect;
		}

		public float interval;
		public bool swing;
		public float repeatDelay;

		public int frameCount { get; private set; }
		public Frame[] frames { get; private set; }
		public PlayState playState { get; private set; }

		public EventListener onPlayEnd { get; private set; }

		Color _color;
		FlipType _flip;
		int _currentFrame;
		bool _playing;
		int _start;
		int _end;
		int _times;
		int _endAt;
		int _status; //0-none, 1-next loop, 2-ending, 3-ended

		public MovieClip()
		{
			_optimizeNotTouchable = true;

			playState = new PlayState();
			interval = 0.1f;
			_playing = true;
			_color = Color.white;

			CreateGameObject("MovieClip");
			graphics = new NGraphics(gameObject);
			graphics.shader = ShaderConfig.imageShader;

			onPlayEnd = new EventListener(this, "onPlayEnd");

			SetPlaySettings();
		}

		public Color color
		{
			get { return _color; }
			set
			{
				if (!_color.Equals(value))
				{
					_color = value;
					graphics.Tint(_color);
				}
			}
		}

		public FlipType flip
		{
			get { return _flip; }
			set
			{
				_flip = value;
				if (frameCount > 0)
					DrawFrame();
			}
		}

		public void SetData(NTexture texture, Frame[] frames)
		{
			this.frames = frames;
			this.frameCount = frames.Length;

			if (_end == -1 || _end > frameCount - 1)
				_end = frameCount - 1;
			if (_endAt == -1 || _endAt > frameCount - 1)
				_endAt = frameCount - 1;
			playState.Rewind();

			graphics.texture = texture;
			InvalidateBatchingState();
			DrawFrame();
		}

		public void Clear()
		{
			this.frameCount = 0;
			graphics.texture = null;
			graphics.Clear();
		}

		public Rect boundsRect
		{
			get { return _contentRect; }
			set
			{
				_contentRect = value;
			}
		}

		public bool playing
		{
			get { return _playing; }
			set { _playing = value; }
		}

		public int currentFrame
		{
			get { return _currentFrame; }
			set
			{
				if (_currentFrame != value)
				{
					_currentFrame = value;
					playState.currrentFrame = value;
					if (frameCount > 0)
						DrawFrame();
				}
			}
		}

		public void SetPlaySettings()
		{
			SetPlaySettings(0, -1, 0, -1);
		}

		//从start帧开始，播放到end帧（-1表示结尾），重复times次（0表示无限循环），循环结束后，停止在endAt帧（-1表示参数end）
		public void SetPlaySettings(int start, int end, int times, int endAt)
		{
			_start = start;
			_end = end;
			if (_end == -1 || _end > frameCount - 1)
				_end = frameCount - 1;
			_times = times;
			_endAt = endAt;
			if (_endAt == -1)
				_endAt = _end;
			this.currentFrame = start;
			_status = 0;
		}

		public override void Update(UpdateContext context)
		{
			if (_playing && frameCount != 0 && _status != 3)
			{
				playState.Update(this, context);
				if (_currentFrame != playState.currrentFrame)
				{
					if (_status == 1)
					{
						_currentFrame = _start;
						playState.currrentFrame = _currentFrame;
						_status = 0;
					}
					else if (_status == 2)
					{
						_currentFrame = _endAt;
						playState.currrentFrame = _currentFrame;
						_status = 3;

						UpdateContext.OnEnd += () => { onPlayEnd.Call(); };
					}
					else
					{
						_currentFrame = playState.currrentFrame;
						if (_currentFrame == _end)
						{
							if (_times > 0)
							{
								_times--;
								if (_times == 0)
									_status = 2;
								else
									_status = 1;
							}
						}
					}
					DrawFrame();
				}
			}
			graphics.Update(context);
		}

		void DrawFrame()
		{
			if (_currentFrame >= frames.Length)
				graphics.Clear();
			else
			{
				Frame frame = frames[_currentFrame];

				Rect uvRect = frame.uvRect;
				if (_flip != FlipType.None)
					ToolSet.FlipRect(ref uvRect, _flip);

				graphics.SetOneQuadMesh(frame.rect, uvRect, _color);
			}
		}
	}
}
