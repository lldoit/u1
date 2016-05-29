using UnityEngine;
using FairyGUI.Utils;
using System;

namespace FairyGUI
{
	public enum FlipType
	{
		None,
		Horizontal,
		Vertical,
		Both
	}

	public class Image : DisplayObject
	{
		protected NTexture _texture;
		protected Color _color;
		protected FlipType _flip;
		protected Rect? _scale9Grid;
		protected bool _scaleByTile;
		protected bool _needRebuild;
		protected FillMethod _fillMethod;
		protected int _fillOrigin;
		protected float _fillAmount;
		protected bool _fillClockwise;
		protected Vector2 _textureScale;

		public Image()
		{
			Create(null);
		}

		public Image(NTexture texture)
			: base()
		{
			Create(texture);
		}

		void Create(NTexture texture)
		{
			_optimizeNotTouchable = true;
			_fillClockwise = true;
			_textureScale = Vector2.one;

			CreateGameObject("Image");
			graphics = new NGraphics(gameObject);
			graphics.shader = ShaderConfig.imageShader;

			_color = Color.white;
			if (texture != null)
				UpdateTexture(texture);
		}

		public NTexture texture
		{
			get { return _texture; }
			set
			{
				UpdateTexture(value);
			}
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
				if (_flip != value)
				{
					_flip = value;
					_needRebuild = true;
				}
			}
		}

		public FillMethod fillMethod
		{
			get { return _fillMethod; }
			set
			{
				if (_fillMethod != value)
				{
					_fillMethod = value;
					_needRebuild = true;
				}
			}
		}

		public int fillOrigin
		{
			get { return _fillOrigin; }
			set
			{
				if (_fillOrigin != value)
				{
					_fillOrigin = value;
					_needRebuild = true;
				}
			}
		}

		public bool fillClockwise
		{
			get { return _fillClockwise; }
			set
			{
				if (_fillClockwise != value)
				{
					_fillClockwise = value;
					_needRebuild = true;
				}
			}
		}

		public float fillAmount
		{
			get { return _fillAmount; }
			set
			{
				if (_fillAmount != value)
				{
					_fillAmount = value;
					_needRebuild = true;
				}
			}
		}

		public Vector2 textureScale
		{
			get { return _textureScale; }
			set
			{
				if (Mathf.Approximately(_textureScale.x, value.x)
					&& Mathf.Approximately(_textureScale.y, value.y))
					return;

				_textureScale = value;
				if (_texture != null)
				{
					_contentRect.width = _texture.width * _textureScale.x;
					_contentRect.height = _texture.height * _textureScale.y;
					_needRebuild = true;
				}
			}
		}

		public Rect? scale9Grid
		{
			get { return _scale9Grid; }
			set
			{
				if (_scale9Grid != value)
				{
					_scale9Grid = value;
					_needRebuild = true;
				}
			}
		}

		public bool scaleByTile
		{
			get { return _scaleByTile; }
			set
			{
				if (_scaleByTile != value)
				{
					_scaleByTile = value;
					_needRebuild = true;
				}
			}
		}

		public override void Update(UpdateContext context)
		{
			if (_needRebuild)
				Rebuild();

			graphics.Update(context);
		}

		virtual protected void UpdateTexture(NTexture value)
		{
			if (value == _texture)
				return;

			if (value != null && (_texture == null || _texture.width != value.width || _texture.height != value.height
				|| !_texture.uvRect.Equals(value.uvRect)))
				_needRebuild = true;

			_texture = value;
			if (_texture != null)
			{
				_contentRect.width = _texture.width * _textureScale.x;
				_contentRect.height = _texture.height * _textureScale.y;
				if (scaleX != 0 && scaleY != 0)
					graphics.enabled = true;
			}
			else
			{
				_contentRect.width = 0;
				_contentRect.height = 0;
				graphics.enabled = false;
			}

			graphics.texture = _texture;
			InvalidateBatchingState();
		}

		virtual protected void Rebuild()
		{
			_needRebuild = false;
			if (_texture == null || scaleX == 0 || scaleY == 0)
			{
				graphics.enabled = false;
				return;
			}

			graphics.enabled = true;

			Rect uvRect = _texture.uvRect;
			if (_flip != FlipType.None)
				ToolSet.FlipRect(ref uvRect, _flip);

			if (_fillMethod != FillMethod.None)
			{
				graphics.Fill(_fillMethod, _fillAmount, _fillOrigin, _fillClockwise, _contentRect, uvRect);
				graphics.FillColors(_color);
				graphics.FillTriangles();
				graphics.UpdateMesh();
			}
			else if (_textureScale == Vector2.one)
			{
				graphics.SetOneQuadMesh(_contentRect, uvRect, _color);
			}
			else if (_scaleByTile)
			{
				int hc = Mathf.CeilToInt(_textureScale.x);
				int vc = Mathf.CeilToInt(_textureScale.y);
				float tailWidth = _contentRect.width - (hc - 1) * _texture.width;
				float tailHeight = _contentRect.height - (vc - 1) * _texture.height;

				graphics.Alloc(hc * vc * 4);

				int k = 0;
				for (int i = 0; i < hc; i++)
				{
					for (int j = 0; j < vc; j++)
					{
						graphics.FillVerts(k, new Rect(i * _texture.width, j * _texture.height,
								i == (hc - 1) ? tailWidth : _texture.width, j == (vc - 1) ? tailHeight : _texture.height));
						Rect uvTmp = uvRect;
						if (i == hc - 1)
							uvTmp.xMax = Mathf.Lerp(uvRect.xMin, uvRect.xMax, tailWidth / _texture.width);
						if (j == vc - 1)
							uvTmp.yMin = Mathf.Lerp(uvRect.yMin, uvRect.yMax, 1 - tailHeight / _texture.height);
						graphics.FillUV(k, uvTmp);
						k += 4;
					}
				}

				graphics.FillColors(_color);
				graphics.FillTriangles();
				graphics.UpdateMesh();
			}
			else if (_scale9Grid != null)
			{
				float[] rows;
				float[] cols;
				float[] dRows;
				float[] dCols;
				Rect gridRect = (Rect)_scale9Grid;

				if (_flip != FlipType.None)
					ToolSet.FlipInnerRect(_texture.width, _texture.height, ref gridRect, _flip);

				rows = new float[] { 0, gridRect.yMin, gridRect.yMax, _texture.height };
				cols = new float[] { 0, gridRect.xMin, gridRect.xMax, _texture.width };

				if (_contentRect.height >= (_texture.height - gridRect.height))
					dRows = new float[] { 0, gridRect.yMin, _contentRect.height - (_texture.height - gridRect.yMax), _contentRect.height };
				else
				{
					float tmp = gridRect.yMin / (_texture.height - gridRect.yMax);
					tmp = _contentRect.height * tmp / (1 + tmp);
					dRows = new float[] { 0, tmp, tmp, _contentRect.height };
				}

				if (_contentRect.width >= (_texture.width - gridRect.width))
					dCols = new float[] { 0, gridRect.xMin, _contentRect.width - (_texture.width - gridRect.xMax), _contentRect.width };
				else
				{
					float tmp = gridRect.xMin / (_texture.width - gridRect.xMax);
					tmp = _contentRect.width * tmp / (1 + tmp);
					dCols = new float[] { 0, tmp, tmp, _contentRect.width };
				}

				graphics.Alloc(16);

				int k = 0;
				for (int cy = 0; cy < 4; cy++)
				{
					for (int cx = 0; cx < 4; cx++)
					{
						Vector2 subTextCoords;
						subTextCoords.x = uvRect.x + cols[cx] / _texture.width * uvRect.width;
						subTextCoords.y = uvRect.y + (1 - rows[cy] / _texture.height) * uvRect.height;
						graphics.uv[k] = subTextCoords;

						Vector3 drawCoords;
						drawCoords.x = dCols[cx];
						drawCoords.y = -dRows[cy];
						drawCoords.z = 0;
						graphics.vertices[k] = drawCoords;

						k++;
					}
				}

				graphics.FillColors(_color);
				graphics.FillTriangles(NGraphics.TRIANGLES_9_GRID);
				graphics.UpdateMesh();
			}
			else
			{
				graphics.SetOneQuadMesh(_contentRect, uvRect, _color);
			}
		}

		public void PrintTo(Mesh mesh, Rect localRect)
		{
			if (_needRebuild)
				Rebuild();

			Rect uvRect = _texture.uvRect;
			if (_flip != FlipType.None)
				ToolSet.FlipRect(ref uvRect, _flip);

			Vector3[] verts;
			Vector2[] uv;
			Color32[] colors;
			int[] triangles;
			int vertCount = 0;

			if (_scaleByTile)
			{
				verts = new Vector3[graphics.vertices.Length];
				uv = new Vector2[graphics.uv.Length];

				int hc = Mathf.CeilToInt(_textureScale.x);
				int vc = Mathf.CeilToInt(_textureScale.y);
				float tailWidth = _contentRect.width - (hc - 1) * _texture.width;
				float tailHeight = _contentRect.height - (vc - 1) * _texture.height;

				Vector2 offset = Vector2.zero;
				for (int i = 0; i < hc; i++)
				{
					for (int j = 0; j < vc; j++)
					{
						Rect rect = new Rect(i * _texture.width, j * _texture.height,
								i == (hc - 1) ? tailWidth : _texture.width, j == (vc - 1) ? tailHeight : _texture.height);
						Rect uvTmp = uvRect;
						if (i == hc - 1)
							uvTmp.xMax = Mathf.Lerp(uvRect.xMin, uvRect.xMax, tailWidth / _texture.width);
						if (j == vc - 1)
							uvTmp.yMin = Mathf.Lerp(uvRect.yMin, uvRect.yMax, 1 - tailHeight / _texture.height);

						Rect bound = ToolSet.Intersection(ref rect, ref localRect);
						if (bound.xMax - bound.xMin >= 0 && bound.yMax - bound.yMin > 0)
						{
							float u0 = (bound.xMin - rect.x) / rect.width;
							float u1 = (bound.xMax - rect.x) / rect.width;
							float v0 = (rect.y + rect.height - bound.yMax) / rect.height;
							float v1 = (rect.y + rect.height - bound.yMin) / rect.height;
							u0 = Mathf.Lerp(uvTmp.xMin, uvTmp.xMax, u0);
							u1 = Mathf.Lerp(uvTmp.xMin, uvTmp.xMax, u1);
							v0 = Mathf.Lerp(uvTmp.yMin, uvTmp.yMax, v0);
							v1 = Mathf.Lerp(uvTmp.yMin, uvTmp.yMax, v1);
							NGraphics.FillUVOfQuad(uv, vertCount, Rect.MinMaxRect(u0, v0, u1, v1));

							if (i == 0 && j == 0)
								offset = new Vector2(bound.x, bound.y);
							bound.x -= offset.x;
							bound.y -= offset.y;

							NGraphics.FillVertsOfQuad(verts, vertCount, bound);

							vertCount += 4;
						}
					}
				}
			}
			else if (_scale9Grid == null || _textureScale == Vector2.zero)
			{
				verts = new Vector3[graphics.vertices.Length];
				uv = new Vector2[graphics.uv.Length];

				Rect bound = ToolSet.Intersection(ref _contentRect, ref localRect);

				float u0 = bound.xMin / _contentRect.width;
				float u1 = bound.xMax / _contentRect.width;
				float v0 = (_contentRect.height - bound.yMax) / _contentRect.height;
				float v1 = (_contentRect.height - bound.yMin) / _contentRect.height;
				u0 = Mathf.Lerp(uvRect.xMin, uvRect.xMax, u0);
				u1 = Mathf.Lerp(uvRect.xMin, uvRect.xMax, u1);
				v0 = Mathf.Lerp(uvRect.yMin, uvRect.yMax, v0);
				v1 = Mathf.Lerp(uvRect.yMin, uvRect.yMax, v1);
				NGraphics.FillUVOfQuad(uv, 0, Rect.MinMaxRect(u0, v0, u1, v1));

				bound.x = 0;
				bound.y = 0;
				NGraphics.FillVertsOfQuad(verts, 0, bound);
				vertCount += 4;
			}
			else
			{
				verts = new Vector3[36];
				uv = new Vector2[36];

				float[] rows;
				float[] cols;
				float[] dRows;
				float[] dCols;
				Rect gridRect = (Rect)_scale9Grid;

				rows = new float[] { 0, gridRect.yMin, gridRect.yMax, _texture.height };
				cols = new float[] { 0, gridRect.xMin, gridRect.xMax, _texture.width };

				if (_contentRect.height >= (_texture.height - gridRect.height))
					dRows = new float[] { 0, gridRect.yMin, _contentRect.height - (_texture.height - gridRect.yMax), _contentRect.height };
				else
				{
					float tmp = gridRect.yMin / (_texture.height - gridRect.yMax);
					tmp = _contentRect.height * tmp / (1 + tmp);
					dRows = new float[] { 0, tmp, tmp, _contentRect.height };
				}

				if (_contentRect.width >= (_texture.width - gridRect.width))
					dCols = new float[] { 0, gridRect.xMin, _contentRect.width - (_texture.width - gridRect.xMax), _contentRect.width };
				else
				{
					float tmp = gridRect.xMin / (_texture.width - gridRect.xMax);
					tmp = _contentRect.width * tmp / (1 + tmp);
					dCols = new float[] { 0, tmp, tmp, _contentRect.width };
				}

				Vector2 offset = new Vector2();
				for (int cy = 0; cy < 3; cy++)
				{
					for (int cx = 0; cx < 3; cx++)
					{
						Rect rect = Rect.MinMaxRect(dCols[cx], dRows[cy], dCols[cx + 1], dRows[cy + 1]);
						Rect bound = ToolSet.Intersection(ref rect, ref localRect);
						if (bound.xMax - bound.xMin >= 0 && bound.yMax - bound.yMin > 0)
						{
							Rect texBound = Rect.MinMaxRect(uvRect.x + cols[cx] / _texture.width * uvRect.width,
								uvRect.y + (1 - rows[cy + 1] / _texture.height) * uvRect.height,
								uvRect.x + cols[cx + 1] / _texture.width * uvRect.width,
								uvRect.y + (1 - rows[cy] / _texture.height) * uvRect.height);

							float u0 = (bound.xMin - rect.x) / rect.width;
							float u1 = (bound.xMax - rect.x) / rect.width;
							float v0 = (rect.y + rect.height - bound.yMax) / rect.height;
							float v1 = (rect.y + rect.height - bound.yMin) / rect.height;
							u0 = Mathf.Lerp(texBound.xMin, texBound.xMax, u0);
							u1 = Mathf.Lerp(texBound.xMin, texBound.xMax, u1);
							v0 = Mathf.Lerp(texBound.yMin, texBound.yMax, v0);
							v1 = Mathf.Lerp(texBound.yMin, texBound.yMax, v1);
							NGraphics.FillUVOfQuad(uv, vertCount, Rect.MinMaxRect(u0, v0, u1, v1));

							if (vertCount == 0)
								offset = new Vector2(bound.x, bound.y);
							bound.x -= offset.x;
							bound.y -= offset.y;
							NGraphics.FillVertsOfQuad(verts, vertCount, bound);

							vertCount += 4;
						}
					}
				}
			}

			if (vertCount != verts.Length)
			{
				Array.Resize(ref verts, vertCount);
				Array.Resize(ref uv, vertCount);
			}
			int triangleCount = (vertCount >> 1) * 3;
			triangles = new int[triangleCount];
			int k = 0;
			for (int i = 0; i < vertCount; i += 4)
			{
				triangles[k++] = i;
				triangles[k++] = i + 1;
				triangles[k++] = i + 2;

				triangles[k++] = i + 2;
				triangles[k++] = i + 3;
				triangles[k++] = i;
			}

			colors = new Color32[vertCount];
			for (int i = 0; i < vertCount; i++)
			{
				Color col = _color;
				col.a = this.alpha;
				colors[i] = col;
			}

			mesh.Clear();
			mesh.vertices = verts;
			mesh.uv = uv;
			mesh.triangles = triangles;
			mesh.colors32 = colors;
		}
	}
}
