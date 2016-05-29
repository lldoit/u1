using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class MaterialPool
	{
		List<NMaterial> _items;
		MaterialManager _manager;
		string[] _variants;

		public MaterialPool(MaterialManager manager, string[] variants)
		{
			_manager = manager;
			_variants = variants;
		}

		public NMaterial GetMaterial(uint frameId, uint context)
		{
			if (_items == null)
				_items = new List<NMaterial>();

			int cnt = _items.Count;
			NMaterial spare = null;
			for (int i = 0; i < cnt; i++)
			{
				NMaterial mat = _items[i];
				if (mat.frameId == frameId)
				{
					if (mat.context == context)
						return mat;
				}
				else if (spare == null)
					spare = mat;
			}

			if (spare != null)
			{
				spare.frameId = frameId;
				spare.context = context;
				return spare;
			}
			else
			{
				NMaterial mat = _manager.CreateMaterial();
				if (_variants != null)
				{
					foreach (string v in _variants)
						mat.EnableKeyword(v);
				}
				mat.frameId = frameId;
				mat.context = context;
				_items.Add(mat);

				return mat;
			}
		}

		public void Clear()
		{
			if (_items != null)
				_items.Clear();
		}

		public void Dispose()
		{
			if (_items != null)
			{
				if (Application.isPlaying)
				{
					foreach (NMaterial mat in _items)
						Material.Destroy(mat);
				}
				else
				{
					foreach (NMaterial mat in _items)
						Material.DestroyImmediate(mat);
				}
				_items = null;
			}
		}
	}
}
