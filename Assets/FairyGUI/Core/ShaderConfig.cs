using UnityEngine;

namespace FairyGUI
{
	public class ShaderConfig
	{
		public delegate Shader GetFunction(string name);

		public static GetFunction Get = Shader.Find;

		public static string imageShader = "FairyGUI/Image";
		public static string textShader = "FairyGUI/Text";
		public static string textBrighterShader = "FairyGUI/Text Brighter";
		public static string bmFontShader = "FairyGUI/BMFont";
	}
}
