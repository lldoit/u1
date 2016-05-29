using System;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class StageEngine : MonoBehaviour
	{
		public int ObjectTotal;
		public int ObjectOnStage;

		void LateUpdate()
		{
			ObjectOnStage = Stage.inst.InternalUpdate();
			ObjectTotal = (int)DisplayObject._gInstanceCounter;
		}

		void OnGUI()
		{
			Stage.inst.HandleGUIEvents(Event.current);
		}

		void OnLevelWasLoaded()
		{
			StageCamera.CheckMainCamera();
		}
	}
}