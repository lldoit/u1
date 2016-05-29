﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI.Utils
{
	/// <summary>
	/// 
	/// </summary>
	public class HtmlButton : IHtmlObject
	{
		public GComponent button { get; private set; }

		public const string CLICK_EVENT = "OnHtmlButtonClick";

		public static string resource;

		RichTextField _owner;
		HtmlElement _element;

		public HtmlButton()
		{
			if (resource != null)
			{
				button = UIPackage.CreateObjectFromURL(resource).asCom;
				button.onClick.Add((EventContext context) =>
				{
					_owner.DispatchEvent(CLICK_EVENT, context.data, this);
				});
			}
			else
				Debug.LogWarning("FairyGUI: Set HtmlButton.resource first");
		}

		public HtmlElement element
		{
			get { return _element; }
		}

		public float width
		{
			get { return button != null ? button.width : 0; }
		}

		public float height
		{
			get { return button != null ? button.height : 0; }
		}

		public void Create(RichTextField owner, HtmlElement element)
		{
			_owner = owner;
			_element = element;

			if (button == null)
				return;

			int width = element.GetInt("width", 0);
			int height = element.GetInt("height", 0);

			if (width == 0)
				width = button.sourceWidth;
			if (height == 0)
				height = button.sourceHeight;

			button.SetSize(width, height);
			button.text = element.GetString("value");
		}

		public void SetPosition(float x, float y)
		{
			if (button != null)
				button.SetXY(x, y);
		}

		public void Add()
		{
			if (button != null)
				_owner.AddChild(button.displayObject);
		}

		public void Remove()
		{
			if (button != null && button.displayObject.parent != null)
				_owner.RemoveChild(button.displayObject);

			_owner = null;
			_element = null;
		}

		public void Dispose()
		{
			if (button != null)
				button.Dispose();
		}
	}
}
