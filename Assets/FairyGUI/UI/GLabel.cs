﻿using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// GLabel class.
	/// </summary>
	public class GLabel : GComponent
	{
		protected GObject _titleObject;
		protected GObject _iconObject;

		public GLabel()
		{
		}

		/// <summary>
		/// Icon of the label.
		/// </summary>
		public string icon
		{
			get
			{
				if (_iconObject is GLoader)
					return ((GLoader)_iconObject).url;
				else if (_iconObject is GLabel)
					return ((GLabel)_iconObject).icon;
				else if (_iconObject is GButton)
					return ((GButton)_iconObject).icon;
				else
					return null;
			}

			set
			{
				if (_iconObject is GLoader)
					((GLoader)_iconObject).url = value;
				else if (_iconObject is GLabel)
					((GLabel)_iconObject).icon = value;
				else if (_iconObject is GButton)
					((GButton)_iconObject).icon = value;
			}
		}

		/// <summary>
		/// Title of the label.
		/// </summary>
		public string title
		{
			get
			{
				if (_titleObject != null)
					return _titleObject.text;
				else
					return null;
			}
			set
			{
				if (_titleObject != null)
					_titleObject.text = value;
			}
		}

		/// <summary>
		/// Same of the title.
		/// </summary>
		override public string text
		{
			get { return this.title; }
			set { this.title = value; }
		}

		/// <summary>
		/// If title is input text.
		/// </summary>
		public bool editable
		{
			get
			{
				if (_titleObject is GTextInput)
					return _titleObject.asTextInput.editable;
				else
					return false;
			}

			set
			{
				if (_titleObject is GTextInput)
					_titleObject.asTextInput.editable = value;
			}
		}

		/// <summary>
		/// Title color of the label
		/// </summary>
		public Color titleColor
		{
			get
			{
				if (_titleObject is GTextField)
					return ((GTextField)_titleObject).color;
				else if (_titleObject is GLabel)
					return ((GLabel)_titleObject).titleColor;
				else if (_titleObject is GButton)
					return ((GButton)_titleObject).titleColor;
				else
					return Color.black;
			}
			set
			{
				if (_titleObject is GTextField)
					((GTextField)_titleObject).color = value;
				else if (_titleObject is GLabel)
					((GLabel)_titleObject).titleColor = value;
				else if (_titleObject is GButton)
					((GButton)_titleObject).titleColor = value;
			}
		}

		override public void ConstructFromXML(XML cxml)
		{
			base.ConstructFromXML(cxml);

			_titleObject = GetChild("title");
			_iconObject = GetChild("icon");
		}

		override public void Setup_AfterAdd(XML cxml)
		{
			base.Setup_AfterAdd(cxml);

			XML xml = cxml.GetNode("Label");
			if (xml == null)
				return;

			string str;
			str = xml.GetAttribute("title");
			if (str != null)
				this.title = str;
			str = xml.GetAttribute("icon");
			if (str != null)
				this.icon = str;
			str = xml.GetAttribute("titleColor");
			if (str != null)
				this.titleColor = ToolSet.ConvertFromHtmlColor(str);

			if (_titleObject is GTextInput)
			{
				str = xml.GetAttribute("promptText");
				if (str != null)
					((GTextInput)_titleObject).promptText = str;
			}
		}
	}
}
