﻿using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class DisplayOptions
	{
		public static Transform[] defaultRoot;//use only in edit mode. use array to avoid unity null reference checking
		public static HideFlags hideFlags = HideFlags.None;

		public static void SetEditModeHideFlags()
		{
#if UNITY_5
	#if SHOW_HIERARCHY_EDIT_MODE
			hideFlags = HideFlags.DontSaveInEditor;
	#else
			hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor;
	#endif
#else
			hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
#endif
		}
	}
}
