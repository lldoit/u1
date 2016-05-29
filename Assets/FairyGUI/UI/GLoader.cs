using System.Collections.Generic;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// GLoader class
	/// </summary>
	public class GLoader : GObject, IAnimationGear, IColorGear
	{
		/// <summary>
		/// Display an error sign if the loader fails to load the content.
		/// </summary>
		public bool showErrorSign;

		/// <summary>
		/// 
		/// </summary>
		public GearAnimation gearAnimation { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public GearColor gearColor { get; private set; }

		string _url;
		AlignType _align;
		VertAlignType _verticalAlign;
		bool _autoSize;
		FillType _fill;
		bool _playing;
		int _frame;
		bool _updatingLayout;
		PackageItem _contentItem;
		float _contentWidth;
		float _contentHeight;
		float _contentSourceWidth;
		float _contentSourceHeight;

		Container _container;
		Image _image;
		MovieClip _movieClip;
		DisplayObject _activeObject;
		GObject _errorSign;

		static GObjectPool errorSignPool = new GObjectPool();

		public GLoader()
		{
			_playing = true;
			_url = string.Empty;
			_align = AlignType.Left;
			_verticalAlign = VertAlignType.Top;
			showErrorSign = true;

			gearAnimation = new GearAnimation(this);
			gearColor = new GearColor(this);
		}

		override protected void CreateDisplayObject()
		{
			_container = new Container("GLoader");
			_container.gOwner = this;
			_container.hitArea = new RectHitTest();
			displayObject = _container;

			_image = new Image();
			_container.AddChild(_image);
		}

		override public void Dispose()
		{
			if (_image.texture != null)
			{
				if (_contentItem == null)
					FreeExternal(image.texture);
			}
			_image.Dispose();
			if (_movieClip != null)
				_movieClip.Dispose();
			base.Dispose();
		}

		/// <summary>
		/// 
		/// </summary>
		public string url
		{
			get { return _url; }
			set
			{
				if (_url == value)
					return;

				_url = value;
				LoadContent();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public AlignType align
		{
			get { return _align; }
			set
			{
				if (_align != value)
				{
					_align = value;
					UpdateLayout();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public VertAlignType verticalAlign
		{
			get { return _verticalAlign; }
			set
			{
				if (_verticalAlign != value)
				{
					_verticalAlign = value;
					UpdateLayout();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public FillType fill
		{
			get { return _fill; }
			set
			{
				if (_fill != value)
				{
					_fill = value;
					UpdateLayout();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool autoSize
		{
			get { return _autoSize; }
			set
			{
				if (_autoSize != value)
				{
					_autoSize = value;
					UpdateLayout();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool playing
		{
			get { return _playing; }
			set
			{
				if (_playing != value)
				{
					_playing = value;
					if (_movieClip != null)
					{
						_movieClip.playing = value;
						if (gearAnimation.controller != null)
							gearAnimation.UpdateState();
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int frame
		{
			get { return _frame; }
			set
			{
				_frame = value;
				if (_movieClip != null)
				{
					_movieClip.currentFrame = value;
					if (gearAnimation.controller != null)
						gearAnimation.UpdateState();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Material material
		{
			get { return _image.material; }
			set
			{
				_image.material = value;
				if (_movieClip != null)
					_movieClip.material = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string shader
		{
			get { return _image.shader; }
			set
			{
				_image.shader = value;
				if (_movieClip != null)
					_movieClip.shader = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Color color
		{
			get { return _image.color; }
			set
			{
				_image.color = value;
				if (_movieClip != null)
					_movieClip.color = value;

				if (gearColor.controller != null)
					gearColor.UpdateState();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public FillMethod fillMethod
		{
			get { return _image.fillMethod; }
			set { _image.fillMethod = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public int fillOrigin
		{
			get { return _image.fillOrigin; }
			set { _image.fillOrigin = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool fillClockwise
		{
			get { return _image.fillClockwise; }
			set { _image.fillClockwise = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public float fillAmount
		{
			get { return _image.fillAmount; }
			set { _image.fillAmount = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Image image
		{
			get { return _image; }
		}

		/// <summary>
		/// 
		/// </summary>
		public MovieClip movieClip
		{
			get
			{
				if (_movieClip == null)
				{
					_movieClip = new MovieClip();
					if (grayed)
						_movieClip.SetGrayed(true);
					_container.AddChild(_movieClip);
				}
				return _movieClip;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public NTexture texture
		{
			get
			{
				return _image.texture;
			}

			set
			{
				ClearContent();

				_image.texture = value;
				_contentSourceWidth = value.width;
				_contentSourceHeight = value.height;
				_activeObject = _image;
				UpdateLayout();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		protected void LoadContent()
		{
			ClearContent();

			if (string.IsNullOrEmpty(_url))
				return;

			if (_url.StartsWith(UIPackage.URL_PREFIX))
				LoadFromPackage(_url);
			else
				LoadExternal();
		}

		protected void LoadFromPackage(string itemURL)
		{
			_contentItem = UIPackage.GetItemByURL(itemURL);

			if (_contentItem != null)
			{
				_contentItem.Load();
				if (_contentItem.type == PackageItemType.Image)
				{
					_image.texture = _contentItem.texture;
					_image.scale9Grid = _contentItem.scale9Grid;
					_image.scaleByTile = _contentItem.scaleByTile;
					_activeObject = _image;

					_contentSourceWidth = _contentItem.width;
					_contentSourceHeight = _contentItem.height;
					UpdateLayout();
				}
				else if (_contentItem.type == PackageItemType.MovieClip)
				{
					if (_movieClip == null)
					{
						_movieClip = new MovieClip();
						if (grayed)
							_movieClip.SetGrayed(true);
						_container.AddChild(_movieClip);
					}

					_contentSourceWidth = _contentItem.width;
					_contentSourceHeight = _contentItem.height;

					_movieClip.interval = _contentItem.interval;
					_movieClip.SetData(_contentItem.texture, _contentItem.frames);
					_movieClip.boundsRect = new Rect(0, 0, _contentSourceWidth, _contentSourceHeight);
					_movieClip.playing = _playing;
					_movieClip.currentFrame = _frame;
					_activeObject = _movieClip;

					UpdateLayout();
				}
				else
					SetErrorState();
			}
			else
				SetErrorState();
		}

		virtual protected void LoadExternal()
		{
			Texture2D tex = (Texture2D)Resources.Load(this.url, typeof(Texture2D));
			if (tex != null)
				onExternalLoadSuccess(new NTexture(tex));
			else
				onExternalLoadFailed();
		}

		virtual protected void FreeExternal(NTexture texture)
		{
		}

		protected void onExternalLoadSuccess(NTexture texture)
		{
			_image.texture = texture;
			_contentSourceWidth = texture.width;
			_contentSourceHeight = texture.height;
			_activeObject = _image;
			UpdateLayout();
		}

		protected void onExternalLoadFailed()
		{
			SetErrorState();
		}

		private void SetErrorState()
		{
			if (!showErrorSign)
				return;

			if (_errorSign == null)
			{
				if (UIConfig.loaderErrorSign != null)
					_errorSign = errorSignPool.GetObject(UIConfig.loaderErrorSign);
			}

			if (_errorSign != null)
			{
				_errorSign.width = this.width;
				_errorSign.height = this.height;
				_errorSign.grayed = grayed;
				_container.AddChild(_errorSign.displayObject);
			}
		}

		private void ClearErrorState()
		{
			if (_errorSign != null)
			{
				_container.RemoveChild(_errorSign.displayObject);
				errorSignPool.ReturnObject(_errorSign);
				_errorSign = null;
			}
		}

		private void UpdateLayout()
		{
			if (_activeObject == null)
			{
				if (_autoSize)
				{
					_updatingLayout = true;
					this.SetSize(50, 30);
					_updatingLayout = false;
				}
				return;
			}

			_contentWidth = _contentSourceWidth;
			_contentHeight = _contentSourceHeight;

			if (_autoSize)
			{
				_updatingLayout = true;
				if (_contentWidth == 0)
					_contentWidth = 50;
				if (_contentHeight == 0)
					_contentHeight = 30;
				this.SetSize(_contentWidth, _contentHeight);
				if (_activeObject is Image)
					((Image)_activeObject).textureScale = Vector2.one;
				else
					_activeObject.SetScale(1, 1);
				_updatingLayout = false;
			}
			else
			{
				float sx = 1, sy = 1;
				if (_fill == FillType.Scale || _fill == FillType.ScaleFree)
				{
					sx = this.width / _contentSourceWidth;
					sy = this.height / _contentSourceHeight;

					if (sx != 1 || sy != 1)
					{
						if (_fill == FillType.Scale)
						{
							if (sx > sy)
								sx = sy;
							else
								sy = sx;
						}
						_contentWidth = Mathf.FloorToInt(_contentSourceWidth * sx);
						_contentHeight = Mathf.FloorToInt(_contentSourceHeight * sy);
					}
				}

				if (_activeObject is Image)
					((Image)_activeObject).textureScale = new Vector2(sx, sy);
				else
					_activeObject.SetScale(sx, sy);

				float nx;
				float ny;
				if (_align == AlignType.Center)
					nx = Mathf.FloorToInt((this.width - _contentWidth) / 2);
				else if (_align == AlignType.Right)
					nx = Mathf.FloorToInt(this.width - _contentWidth);
				else
					nx = 0;
				if (_verticalAlign == VertAlignType.Middle)
					ny = Mathf.FloorToInt((this.height - _contentHeight) / 2);
				else if (_verticalAlign == VertAlignType.Bottom)
					ny = Mathf.FloorToInt(this.height - _contentHeight);
				else
					ny = 0;
				_activeObject.SetXY(nx, ny);
			}
		}

		private void ClearContent()
		{
			ClearErrorState();

			if (_activeObject != null)
			{
				if (_image == _activeObject)
				{
					if (_image.texture != null)
					{
						if (_contentItem == null)
							FreeExternal(image.texture);
						_image.texture = null;
					}
				}
				else if (_movieClip == _activeObject)
					_movieClip.Clear();
				_activeObject = null;
			}

			_contentItem = null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="c"></param>
		override public void HandleControllerChanged(Controller c)
		{
			base.HandleControllerChanged(c);
			if (gearAnimation.controller == c)
				gearAnimation.Apply();
			if (gearColor.controller == c)
				gearColor.Apply();
		}

		override protected void HandleSizeChanged()
		{
			if (!_updatingLayout)
				UpdateLayout();
			if (_container.hitArea != null)
				((RectHitTest)_container.hitArea).Set(0, 0, this.width, this.height);

			_container.SetScale(this.scaleX, this.scaleY);
		}

		override protected void HandleGrayedChanged()
		{
			base.HandleGrayedChanged();

			_image.SetGrayed(grayed);
			if (_movieClip != null)
				_movieClip.SetGrayed(grayed);
			if (_errorSign != null)
				_errorSign.grayed = grayed;
		}

		override public void Setup_BeforeAdd(XML xml)
		{
			base.Setup_BeforeAdd(xml);

			string str;
			str = xml.GetAttribute("url");
			if (str != null)
				_url = str;

			str = xml.GetAttribute("align");
			if (str != null)
				_align = FieldTypes.ParseAlign(str);

			str = xml.GetAttribute("vAlign");
			if (str != null)
				_verticalAlign = FieldTypes.ParseVerticalAlign(str);

			str = xml.GetAttribute("fill");
			if (str != null)
				_fill = FieldTypes.ParseFillType(str);

			_autoSize = xml.GetAttributeBool("autoSize", false);

			str = xml.GetAttribute("errorSign");
			if (str != null)
				showErrorSign = str == "true";

			_playing = xml.GetAttributeBool("playing", true);

			str = xml.GetAttribute("color");
			if (str != null)
				this.color = ToolSet.ConvertFromHtmlColor(str);

			str = xml.GetAttribute("fillMethod");
			if (str != null)
				_image.fillMethod = FieldTypes.ParseFillMethod(str);

			if (_image.fillMethod != FillMethod.None)
			{
				_image.fillOrigin = xml.GetAttributeInt("fillOrigin");
				_image.fillClockwise = xml.GetAttributeBool("fillClockwise", true);
				_image.fillAmount = (float)xml.GetAttributeInt("fillAmount", 100) / 100;
			}

			if (_url != null)
				LoadContent();
		}

		override public void Setup_AfterAdd(XML xml)
		{
			base.Setup_AfterAdd(xml);

			XML cxml = xml.GetNode("gearAni");
			if (cxml != null)
				gearAnimation.Setup(cxml);

			cxml = xml.GetNode("gearColor");
			if (cxml != null)
				gearColor.Setup(cxml);
		}
	}
}
