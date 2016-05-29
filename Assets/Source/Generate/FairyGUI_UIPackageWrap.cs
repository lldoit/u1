﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class FairyGUI_UIPackageWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(FairyGUI.UIPackage), typeof(System.Object));
		L.RegFunction("GetById", GetById);
		L.RegFunction("GetByName", GetByName);
		L.RegFunction("AddPackage", AddPackage);
		L.RegFunction("RemovePackage", RemovePackage);
		L.RegFunction("RemoveAllPackages", RemoveAllPackages);
		L.RegFunction("GetPackages", GetPackages);
		L.RegFunction("CreateObject", CreateObject);
		L.RegFunction("CreateObjectFromURL", CreateObjectFromURL);
		L.RegFunction("GetItemAsset", GetItemAsset);
		L.RegFunction("GetItemAssetByURL", GetItemAssetByURL);
		L.RegFunction("GetItemURL", GetItemURL);
		L.RegFunction("GetItemByURL", GetItemByURL);
		L.RegFunction("SetStringsSource", SetStringsSource);
		L.RegFunction("GetPixelHitTestData", GetPixelHitTestData);
		L.RegFunction("GetItems", GetItems);
		L.RegFunction("GetItem", GetItem);
		L.RegFunction("GetItemByName", GetItemByName);
		L.RegFunction("New", _CreateFairyGUI_UIPackage);
		L.RegFunction("__tostring", Lua_ToString);
		L.RegVar("id", get_id, null);
		L.RegVar("name", get_name, null);
		L.RegVar("assetPath", get_assetPath, null);
		L.RegVar("customId", get_customId, set_customId);
		L.RegFunction("LoadResource", FairyGUI_UIPackage_LoadResource);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _CreateFairyGUI_UIPackage(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 0)
			{
				FairyGUI.UIPackage obj = new FairyGUI.UIPackage();
				ToLua.PushObject(L, obj);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to ctor method: FairyGUI.UIPackage.New");
			}
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetById(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			string arg0 = ToLua.CheckString(L, 1);
			FairyGUI.UIPackage o = FairyGUI.UIPackage.GetById(arg0);
			ToLua.PushObject(L, o);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetByName(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			string arg0 = ToLua.CheckString(L, 1);
			FairyGUI.UIPackage o = FairyGUI.UIPackage.GetByName(arg0);
			ToLua.PushObject(L, o);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int AddPackage(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 1 && TypeChecker.CheckTypes(L, 1, typeof(string)))
			{
				string arg0 = ToLua.ToString(L, 1);
				FairyGUI.UIPackage o = FairyGUI.UIPackage.AddPackage(arg0);
				ToLua.PushObject(L, o);
				return 1;
			}
			else if (count == 1 && TypeChecker.CheckTypes(L, 1, typeof(UnityEngine.AssetBundle)))
			{
				UnityEngine.AssetBundle arg0 = (UnityEngine.AssetBundle)ToLua.ToObject(L, 1);
				FairyGUI.UIPackage o = FairyGUI.UIPackage.AddPackage(arg0);
				ToLua.PushObject(L, o);
				return 1;
			}
			else if (count == 2 && TypeChecker.CheckTypes(L, 1, typeof(string), typeof(FairyGUI.UIPackage.LoadResource)))
			{
				string arg0 = ToLua.ToString(L, 1);
				FairyGUI.UIPackage.LoadResource arg1 = null;
				LuaTypes funcType2 = LuaDLL.lua_type(L, 2);

				if (funcType2 != LuaTypes.LUA_TFUNCTION)
				{
					 arg1 = (FairyGUI.UIPackage.LoadResource)ToLua.ToObject(L, 2);
				}
				else
				{
					LuaFunction func = ToLua.ToLuaFunction(L, 2);
					arg1 = DelegateFactory.CreateDelegate(typeof(FairyGUI.UIPackage.LoadResource), func) as FairyGUI.UIPackage.LoadResource;
				}

				FairyGUI.UIPackage o = FairyGUI.UIPackage.AddPackage(arg0, arg1);
				ToLua.PushObject(L, o);
				return 1;
			}
			else if (count == 2 && TypeChecker.CheckTypes(L, 1, typeof(string), typeof(UnityEngine.AssetBundle)))
			{
				string arg0 = ToLua.ToString(L, 1);
				UnityEngine.AssetBundle arg1 = (UnityEngine.AssetBundle)ToLua.ToObject(L, 2);
				FairyGUI.UIPackage o = FairyGUI.UIPackage.AddPackage(arg0, arg1);
				ToLua.PushObject(L, o);
				return 1;
			}
			else if (count == 2 && TypeChecker.CheckTypes(L, 1, typeof(UnityEngine.AssetBundle), typeof(UnityEngine.AssetBundle)))
			{
				UnityEngine.AssetBundle arg0 = (UnityEngine.AssetBundle)ToLua.ToObject(L, 1);
				UnityEngine.AssetBundle arg1 = (UnityEngine.AssetBundle)ToLua.ToObject(L, 2);
				FairyGUI.UIPackage o = FairyGUI.UIPackage.AddPackage(arg0, arg1);
				ToLua.PushObject(L, o);
				return 1;
			}
			else if (count == 3 && TypeChecker.CheckTypes(L, 1, typeof(UnityEngine.AssetBundle), typeof(UnityEngine.AssetBundle), typeof(string)))
			{
				UnityEngine.AssetBundle arg0 = (UnityEngine.AssetBundle)ToLua.ToObject(L, 1);
				UnityEngine.AssetBundle arg1 = (UnityEngine.AssetBundle)ToLua.ToObject(L, 2);
				string arg2 = ToLua.ToString(L, 3);
				FairyGUI.UIPackage o = FairyGUI.UIPackage.AddPackage(arg0, arg1, arg2);
				ToLua.PushObject(L, o);
				return 1;
			}
			else if (count == 3 && TypeChecker.CheckTypes(L, 1, typeof(string), typeof(UnityEngine.AssetBundle), typeof(string)))
			{
				string arg0 = ToLua.ToString(L, 1);
				UnityEngine.AssetBundle arg1 = (UnityEngine.AssetBundle)ToLua.ToObject(L, 2);
				string arg2 = ToLua.ToString(L, 3);
				FairyGUI.UIPackage o = FairyGUI.UIPackage.AddPackage(arg0, arg1, arg2);
				ToLua.PushObject(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: FairyGUI.UIPackage.AddPackage");
			}
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int RemovePackage(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			string arg0 = ToLua.CheckString(L, 1);
			FairyGUI.UIPackage.RemovePackage(arg0);
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int RemoveAllPackages(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 0);
			FairyGUI.UIPackage.RemoveAllPackages();
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetPackages(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 0);
			System.Collections.Generic.List<FairyGUI.UIPackage> o = FairyGUI.UIPackage.GetPackages();
			ToLua.PushObject(L, o);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int CreateObject(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 2 && TypeChecker.CheckTypes(L, 1, typeof(FairyGUI.UIPackage), typeof(string)))
			{
				FairyGUI.UIPackage obj = (FairyGUI.UIPackage)ToLua.ToObject(L, 1);
				string arg0 = ToLua.ToString(L, 2);
				FairyGUI.GObject o = obj.CreateObject(arg0);
				ToLua.PushObject(L, o);
				return 1;
			}
			else if (count == 2 && TypeChecker.CheckTypes(L, 1, typeof(string), typeof(string)))
			{
				string arg0 = ToLua.ToString(L, 1);
				string arg1 = ToLua.ToString(L, 2);
				FairyGUI.GObject o = FairyGUI.UIPackage.CreateObject(arg0, arg1);
				ToLua.PushObject(L, o);
				return 1;
			}
			else if (count == 3 && TypeChecker.CheckTypes(L, 1, typeof(FairyGUI.UIPackage), typeof(string), typeof(System.Type)))
			{
				FairyGUI.UIPackage obj = (FairyGUI.UIPackage)ToLua.ToObject(L, 1);
				string arg0 = ToLua.ToString(L, 2);
				System.Type arg1 = (System.Type)ToLua.ToObject(L, 3);
				FairyGUI.GObject o = obj.CreateObject(arg0, arg1);
				ToLua.PushObject(L, o);
				return 1;
			}
			else if (count == 3 && TypeChecker.CheckTypes(L, 1, typeof(string), typeof(string), typeof(System.Type)))
			{
				string arg0 = ToLua.ToString(L, 1);
				string arg1 = ToLua.ToString(L, 2);
				System.Type arg2 = (System.Type)ToLua.ToObject(L, 3);
				FairyGUI.GObject o = FairyGUI.UIPackage.CreateObject(arg0, arg1, arg2);
				ToLua.PushObject(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: FairyGUI.UIPackage.CreateObject");
			}
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int CreateObjectFromURL(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 1 && TypeChecker.CheckTypes(L, 1, typeof(string)))
			{
				string arg0 = ToLua.ToString(L, 1);
				FairyGUI.GObject o = FairyGUI.UIPackage.CreateObjectFromURL(arg0);
				ToLua.PushObject(L, o);
				return 1;
			}
			else if (count == 2 && TypeChecker.CheckTypes(L, 1, typeof(string), typeof(System.Type)))
			{
				string arg0 = ToLua.ToString(L, 1);
				System.Type arg1 = (System.Type)ToLua.ToObject(L, 2);
				FairyGUI.GObject o = FairyGUI.UIPackage.CreateObjectFromURL(arg0, arg1);
				ToLua.PushObject(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: FairyGUI.UIPackage.CreateObjectFromURL");
			}
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetItemAsset(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 2 && TypeChecker.CheckTypes(L, 1, typeof(FairyGUI.UIPackage), typeof(string)))
			{
				FairyGUI.UIPackage obj = (FairyGUI.UIPackage)ToLua.ToObject(L, 1);
				string arg0 = ToLua.ToString(L, 2);
				object o = obj.GetItemAsset(arg0);
				ToLua.Push(L, o);
				return 1;
			}
			else if (count == 2 && TypeChecker.CheckTypes(L, 1, typeof(string), typeof(string)))
			{
				string arg0 = ToLua.ToString(L, 1);
				string arg1 = ToLua.ToString(L, 2);
				object o = FairyGUI.UIPackage.GetItemAsset(arg0, arg1);
				ToLua.Push(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: FairyGUI.UIPackage.GetItemAsset");
			}
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetItemAssetByURL(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			string arg0 = ToLua.CheckString(L, 1);
			object o = FairyGUI.UIPackage.GetItemAssetByURL(arg0);
			ToLua.Push(L, o);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetItemURL(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			string arg0 = ToLua.CheckString(L, 1);
			string arg1 = ToLua.CheckString(L, 2);
			string o = FairyGUI.UIPackage.GetItemURL(arg0, arg1);
			LuaDLL.lua_pushstring(L, o);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetItemByURL(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			string arg0 = ToLua.CheckString(L, 1);
			FairyGUI.PackageItem o = FairyGUI.UIPackage.GetItemByURL(arg0);
			ToLua.PushObject(L, o);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetStringsSource(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			FairyGUI.Utils.XML arg0 = (FairyGUI.Utils.XML)ToLua.CheckObject(L, 1, typeof(FairyGUI.Utils.XML));
			FairyGUI.UIPackage.SetStringsSource(arg0);
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetPixelHitTestData(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			FairyGUI.UIPackage obj = (FairyGUI.UIPackage)ToLua.CheckObject(L, 1, typeof(FairyGUI.UIPackage));
			string arg0 = ToLua.CheckString(L, 2);
			FairyGUI.PixelHitTestData o = obj.GetPixelHitTestData(arg0);
			ToLua.PushObject(L, o);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetItems(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			FairyGUI.UIPackage obj = (FairyGUI.UIPackage)ToLua.CheckObject(L, 1, typeof(FairyGUI.UIPackage));
			System.Collections.Generic.List<FairyGUI.PackageItem> o = obj.GetItems();
			ToLua.PushObject(L, o);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetItem(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			FairyGUI.UIPackage obj = (FairyGUI.UIPackage)ToLua.CheckObject(L, 1, typeof(FairyGUI.UIPackage));
			string arg0 = ToLua.CheckString(L, 2);
			FairyGUI.PackageItem o = obj.GetItem(arg0);
			ToLua.PushObject(L, o);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetItemByName(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			FairyGUI.UIPackage obj = (FairyGUI.UIPackage)ToLua.CheckObject(L, 1, typeof(FairyGUI.UIPackage));
			string arg0 = ToLua.CheckString(L, 2);
			FairyGUI.PackageItem o = obj.GetItemByName(arg0);
			ToLua.PushObject(L, o);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Lua_ToString(IntPtr L)
	{
		object obj = ToLua.ToObject(L, 1);

		if (obj != null)
		{
			LuaDLL.lua_pushstring(L, obj.ToString());
		}
		else
		{
			LuaDLL.lua_pushnil(L);
		}

		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_id(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			FairyGUI.UIPackage obj = (FairyGUI.UIPackage)o;
			string ret = obj.id;
			LuaDLL.lua_pushstring(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o == null ? "attempt to index id on a nil value" : e.Message);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_name(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			FairyGUI.UIPackage obj = (FairyGUI.UIPackage)o;
			string ret = obj.name;
			LuaDLL.lua_pushstring(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o == null ? "attempt to index name on a nil value" : e.Message);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_assetPath(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			FairyGUI.UIPackage obj = (FairyGUI.UIPackage)o;
			string ret = obj.assetPath;
			LuaDLL.lua_pushstring(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o == null ? "attempt to index assetPath on a nil value" : e.Message);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_customId(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			FairyGUI.UIPackage obj = (FairyGUI.UIPackage)o;
			string ret = obj.customId;
			LuaDLL.lua_pushstring(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o == null ? "attempt to index customId on a nil value" : e.Message);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_customId(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			FairyGUI.UIPackage obj = (FairyGUI.UIPackage)o;
			string arg0 = ToLua.CheckString(L, 2);
			obj.customId = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o == null ? "attempt to index customId on a nil value" : e.Message);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int FairyGUI_UIPackage_LoadResource(IntPtr L)
	{
		try
		{
			LuaFunction func = ToLua.CheckLuaFunction(L, 1);
			Delegate arg1 = DelegateFactory.CreateDelegate(typeof(FairyGUI.UIPackage.LoadResource), func);
			ToLua.Push(L, arg1);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}
}

