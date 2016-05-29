using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class NGraphics
	{
		public Vector3[] vertices { get; private set; }
		public Vector2[] uv { get; private set; }
		public Color32[] colors { get; private set; }
		public int[] triangles { get; private set; }
		public int vertCount { get; private set; }

		public MeshFilter meshFilter { get; private set; }
		public MeshRenderer meshRenderer { get; private set; }

		public bool grayed;

		internal uint maskFrameId;

		float _alpha;
		byte[] _alphaBackup;

		NTexture _texture;
		string _shader;
		Material _material;
		MaterialManager _manager;
		Mesh mesh;

		//写死的一些三角形顶点组合，避免每次new
		/** 1---2
		 *  | / |
		 *  0---3
		 */
		public static int[] TRIANGLES = new int[] { 0, 1, 2, 2, 3, 0 };
		public static int[] TRIANGLES_9_GRID = new int[] { 
			4,0,1,1,5,4,
			5,1,2,2,6,5,
			6,2,3,3,7,6,
			8,4,5,5,9,8,
			9,5,6,6,10,9,
			10,6,7,7,11,10,
			12,8,9,9,13,12,
			13,9,10,10,14,13,
			14,10,11,
			11,15,14
        };

		public NGraphics(GameObject gameObject)
		{
			_alpha = 1f;
			_shader = ShaderConfig.imageShader;
			meshFilter = gameObject.AddComponent<MeshFilter>();
			meshRenderer = gameObject.AddComponent<MeshRenderer>();
#if UNITY_5
			meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
#else
            meshRenderer.castShadows = false;
#endif
			meshRenderer.receiveShadows = false;
			mesh = new Mesh();
			mesh.MarkDynamic();

			meshFilter.hideFlags = DisplayOptions.hideFlags;
			meshRenderer.hideFlags = DisplayOptions.hideFlags;
			mesh.hideFlags = DisplayOptions.hideFlags;
		}

		public NTexture texture
		{
			get { return _texture; }
			set
			{
				if (_texture != value)
				{
					_texture = value;
					if (_texture != null)
					{
						_manager = MaterialManager.GetInstance(_texture, _shader);
						if (_material != null)
							_material.mainTexture = _texture.nativeTexture;
					}
					else
					{
						if (_material != null)
							_material.mainTexture = null;
						_manager = null;
					}
				}
			}
		}

		public string shader
		{
			get { return _shader; }
			set
			{
				_shader = value;
				if (_texture != null)
					_manager = MaterialManager.GetInstance(_texture, _shader);
			}
		}

		public void SetShaderAndTexture(string shader, NTexture texture)
		{
			_shader = shader;
			_texture = texture;
			if (_texture != null)
			{
				_manager = MaterialManager.GetInstance(_texture, _shader);
				if (_material != null)
					_material.mainTexture = _texture.nativeTexture;
			}
			else
			{
				if (_material != null)
					_material.mainTexture = null;
				_manager = null;
			}
		}

		public Material material
		{
			get
			{
				if (meshRenderer.sharedMaterial != null)
					return meshRenderer.sharedMaterial;
				else if (_manager != null)
					return _manager.sharedMaterial;
				else
					return null;
			}
			set
			{
				_material = value;
				if (_material != null)
				{
					if (_texture != null)
						_material.mainTexture = _texture.nativeTexture;
				}
				meshRenderer.sharedMaterial = _material;
			}
		}

		public bool enabled
		{
			get { return meshRenderer.enabled; }
			set { meshRenderer.enabled = value; }
		}

		public int sortingOrder
		{
			get { return meshRenderer.sortingOrder; }
			set { meshRenderer.sortingOrder = value; }
		}

		public void Dispose()
		{
			if (mesh != null)
			{
				if (Application.isPlaying)
					Mesh.Destroy(mesh);
				else
					Mesh.DestroyImmediate(mesh);
				mesh = null;
			}
			_manager = null;
			_material = null;
			meshRenderer = null;
			meshFilter = null;
		}

		virtual public void Update(UpdateContext context)
		{
			if (_manager == null)
				return;

			Material mat;
			if ((object)_material != null)
				mat = _material;
			else
				mat = _manager.GetContextMaterial(this, context);
			if ((object)mat != (object)meshRenderer.sharedMaterial && (object)mat.mainTexture != null)
				meshRenderer.sharedMaterial = mat;
		}

		public void Alloc(int vertCount)
		{
			if (vertices == null || vertices.Length != vertCount)
			{
				vertices = new Vector3[vertCount];
				uv = new Vector2[vertCount];
				colors = new Color32[vertCount];
			}
		}

		public void UpdateMesh()
		{
			vertCount = vertices.Length;

			for (int i = 0; i < vertCount; i++)
			{
				Color32 col = colors[i];
				if (col.a != 255)
				{
					if (_alphaBackup == null)
						_alphaBackup = new byte[vertCount];
				}
				col.a = (byte)(col.a * _alpha);
				colors[i] = col;
			}

			if (_alphaBackup != null)
			{
				if (_alphaBackup.Length < vertCount)
					_alphaBackup = new byte[vertCount];

				for (int i = 0; i < vertCount; i++)
					_alphaBackup[i] = colors[i].a;
			}

			mesh.Clear();
			mesh.vertices = vertices;
			mesh.uv = uv;
			mesh.triangles = triangles;
			mesh.colors32 = colors;
			meshFilter.mesh = mesh;
		}

		public void SetOneQuadMesh(Rect drawRect, Rect uvRect, Color color)
		{
			Alloc(4);
			FillVerts(0, drawRect);
			FillUV(0, uvRect);
			FillColors(color);
			this.triangles = TRIANGLES;
			UpdateMesh();
		}

		public void DrawRect(Rect vertRect, int lineSize, Color lineColor, Color fillColor)
		{
			if (lineSize == 0)
			{
				SetOneQuadMesh(new Rect(0, 0, vertRect.width, vertRect.height), new Rect(0, 0, 1, 1), fillColor);
			}
			else
			{
				Alloc(20);

				Rect rect;
				//left,right
				rect = Rect.MinMaxRect(0, 0, lineSize, vertRect.height);
				FillVerts(0, rect);
				rect = Rect.MinMaxRect(vertRect.width - lineSize, 0, vertRect.width, vertRect.height);
				FillVerts(4, rect);

				//top, bottom
				rect = Rect.MinMaxRect(lineSize, 0, vertRect.width - lineSize, lineSize);
				FillVerts(8, rect);
				rect = Rect.MinMaxRect(lineSize, vertRect.height - lineSize, vertRect.width - lineSize, vertRect.height);
				FillVerts(12, rect);

				//middle
				rect = Rect.MinMaxRect(lineSize, lineSize, vertRect.width - lineSize, vertRect.height - lineSize);
				FillVerts(16, rect);

				rect = new Rect(0, 0, 1, 1);
				int i;
				for (i = 0; i < 5; i++)
					FillUV(i * 4, rect);

				Color32 col32 = lineColor;
				for (i = 0; i < 16; i++)
					this.colors[i] = col32;

				col32 = fillColor;
				for (i = 16; i < 20; i++)
					this.colors[i] = col32;

				FillTriangles();
				UpdateMesh();
			}
		}

		public void DrawEllipse(Rect vertRect, Color fillColor)
		{
			float radiusX = vertRect.width / 2;
			float radiusY = vertRect.height / 2;
			int numSides = Mathf.CeilToInt(Mathf.PI * (radiusX + radiusY) / 4);
			if (numSides < 6) numSides = 6;
			Alloc(numSides + 1);

			float angleDelta = 2 * Mathf.PI / numSides;
			float angle = 0;

			vertices[0] = new Vector3(radiusX, -radiusY);
			for (int i = 1; i <= numSides; i++)
			{
				vertices[i] = new Vector3(Mathf.Cos(angle) * radiusX + radiusX,
					Mathf.Sin(angle) * radiusY - radiusY, 0);
				angle += angleDelta;
			}

			int triangleCount = numSides * 3;
			if (this.triangles != null && this.triangles.Length == triangleCount
				&& this.triangles != TRIANGLES && this.triangles != TRIANGLES_9_GRID)
				triangles = this.triangles;
			else
				triangles = new int[triangleCount];

			int k = 0;
			for (int i = 1; i < numSides; i++)
			{
				triangles[k++] = i;
				triangles[k++] = i + 1;
				triangles[k++] = 0;
			}
			triangles[k++] = numSides;
			triangles[k++] = 1;
			triangles[k++] = 0;

			FillColors(fillColor);
			UpdateMesh();
		}

		public void Fill(FillMethod method, float amount, int origin, bool clockwise, Rect vertRect, Rect uvRect)
		{
			amount = Mathf.Clamp01(amount);
			switch (method)
			{
				case FillMethod.Horizontal:
					Alloc(4);
					FillUtils.FillHorizontal((OriginHorizontal)origin, amount, vertRect, uvRect, vertices, uv);
					break;

				case FillMethod.Vertical:
					Alloc(4);
					FillUtils.FillVertical((OriginVertical)origin, amount, vertRect, uvRect, vertices, uv);
					break;

				case FillMethod.Radial90:
					Alloc(4);
					FillUtils.FillRadial90((Origin90)origin, amount, clockwise, vertRect, uvRect, vertices, uv);
					break;

				case FillMethod.Radial180:
					Alloc(8);
					FillUtils.FillRadial180((Origin180)origin, amount, clockwise, vertRect, uvRect, vertices, uv);
					break;

				case FillMethod.Radial360:
					Alloc(12);
					FillUtils.FillRadial360((Origin360)origin, amount, clockwise, vertRect, uvRect, vertices, uv);
					break;
			}
		}

		public void FillVerts(int index, Rect rect)
		{
			vertices[index] = new Vector3(rect.xMin, -rect.yMax, 0f);
			vertices[index + 1] = new Vector3(rect.xMin, -rect.yMin, 0f);
			vertices[index + 2] = new Vector3(rect.xMax, -rect.yMin, 0f);
			vertices[index + 3] = new Vector3(rect.xMax, -rect.yMax, 0f);
		}

		public void FillUV(int index, Rect rect)
		{
			uv[index] = new Vector2(rect.xMin, rect.yMin);
			uv[index + 1] = new Vector2(rect.xMin, rect.yMax);
			uv[index + 2] = new Vector2(rect.xMax, rect.yMax);
			uv[index + 3] = new Vector2(rect.xMax, rect.yMin);
		}

		public void FillColors(Color value)
		{
			int count = this.colors.Length;
			Color32 col32 = value;
			for (int i = 0; i < count; i++)
				this.colors[i] = col32;
		}

		public void FillTriangles()
		{
			int vertCount = this.vertices.Length;
			int triangleCount = (vertCount >> 1) * 3;
			if (this.triangles != null && this.triangles.Length == triangleCount
				&& this.triangles != TRIANGLES && this.triangles != TRIANGLES_9_GRID)
				triangles = this.triangles;
			else
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
		}

		public void FillTriangles(int[] triangles)
		{
			this.triangles = triangles;
		}

		public void Clear()
		{
			vertCount = 0;

			mesh.Clear();
			meshFilter.mesh = mesh;
		}

		public void Tint(Color value)
		{
			if (this.colors == null)
				return;

			Color32 value32 = value;
			int count = this.colors.Length;
			for (int i = 0; i < count; i++)
			{
				Color32 col = value32;
				if (col.a != 255)
				{
					if (_alphaBackup == null)
						_alphaBackup = new byte[vertCount];
				}
				col.a = (byte)(_alpha * 255);
				this.colors[i] = col;
			}

			if (_alphaBackup != null)
			{
				if (_alphaBackup.Length < vertCount)
					_alphaBackup = new byte[vertCount];

				for (int i = 0; i < vertCount; i++)
					_alphaBackup[i] = this.colors[i].a;
			}

			mesh.colors32 = this.colors;
		}

		public float alpha
		{
			get { return _alpha; }
			set
			{
				if (_alpha != value)
				{
					_alpha = value;

					if (vertCount > 0)
					{
						int count = this.colors.Length;
						for (int i = 0; i < count; i++)
						{
							Color32 col = this.colors[i];
							col.a = (byte)(_alpha * (_alphaBackup != null ? _alphaBackup[i] : (byte)255));
							this.colors[i] = col;
						}
						mesh.colors32 = this.colors;
					}
				}
			}
		}

		public static void FillVertsOfQuad(Vector3[] verts, int index, Rect rect)
		{
			verts[index] = new Vector3(rect.xMin, -rect.yMax, 0f);
			verts[index + 1] = new Vector3(rect.xMin, -rect.yMin, 0f);
			verts[index + 2] = new Vector3(rect.xMax, -rect.yMin, 0f);
			verts[index + 3] = new Vector3(rect.xMax, -rect.yMax, 0f);
		}

		public static void FillUVOfQuad(Vector2[] uv, int index, Rect rect)
		{
			uv[index] = new Vector2(rect.xMin, rect.yMin);
			uv[index + 1] = new Vector2(rect.xMin, rect.yMax);
			uv[index + 2] = new Vector2(rect.xMax, rect.yMax);
			uv[index + 3] = new Vector2(rect.xMax, rect.yMin);
		}
	}
}
