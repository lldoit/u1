using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class Shape : DisplayObject
	{
		int _type;
		int _lineSize;
		Color _lineColor;
		Color _fillColor;

		public Shape()
		{
			CreateGameObject("Shape");
		}

		public bool empty
		{
			get { return _type == 0; }
		}

		public void DrawRect(float aWidth, float aHeight, int lineSize, Color lineColor, Color fillColor)
		{
			_type = 1;
			_optimizeNotTouchable = false;
			_contentRect = new Rect(0, 0, aWidth, aHeight);
			_lineSize = lineSize;
			_lineColor = lineColor;
			_fillColor = fillColor;

			DrawShape();
		}

		public void DrawEllipse(float aWidth, float aHeight, Color fillColor)
		{
			_type = 2;
			_optimizeNotTouchable = false;
			_contentRect = new Rect(0, 0, aWidth, aHeight);
			_fillColor = fillColor;

			DrawShape();
		}

		void DrawShape()
		{
			if (graphics == null)
			{
				graphics = new NGraphics(gameObject);
				graphics.texture = NTexture.Empty;
				InvalidateBatchingState();
			}

			if (_type == 1)
				graphics.DrawRect(_contentRect, _lineSize, _lineColor, _fillColor);
			else
				graphics.DrawEllipse(_contentRect, _fillColor);
		}

		public void ResizeShape(float aWidth, float aHeight)
		{
			_contentRect = new Rect(0, 0, aWidth, aHeight);
			DrawShape();
		}

		public void Clear()
		{
			_type = 0;
			_optimizeNotTouchable = true;
			if (graphics != null)
				graphics.Clear();
		}

		public override void Update(UpdateContext context)
		{
			if (graphics != null)
				graphics.Update(context);
		}
	}
}
