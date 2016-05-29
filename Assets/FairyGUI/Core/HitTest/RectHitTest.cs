using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class RectHitTest : IHitTest
	{
		public Rect rect;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public void Set(float x, float y, float width, float height)
		{
			rect.Set(x, y, width, height);
		}

		public void SetEnabled(bool value)
		{
		}

		public int HitTest(Container container, ref Vector2 localPoint)
		{
			localPoint = container.GetHitTestLocalPoint();
			if (rect.Contains(localPoint))
				return 1;
			else
				return 2;
		}
	}
}
