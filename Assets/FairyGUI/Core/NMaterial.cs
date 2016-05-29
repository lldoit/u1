using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class NMaterial : Material
	{
		public uint context;
		public uint frameId;
		public bool stencilSet;

		public NMaterial(Shader shader)
			: base(shader)
		{
		}
	}
}
