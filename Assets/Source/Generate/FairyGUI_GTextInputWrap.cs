﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class FairyGUI_GTextInputWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(FairyGUI.GTextInput), typeof(FairyGUI.GTextField));
		L.RegFunction("ReplaceSelection", ReplaceSelection);
		L.RegFunction("Setup_BeforeAdd", Setup_BeforeAdd);
		L.RegFunction("Setup_AfterAdd", Setup_AfterAdd);
		L.RegFunction("New", _CreateFairyGUI_GTextInput);
		L.RegFunction("__tostring", Lua_ToString);
		L.RegVar("editable", get_editable, set_editable);
		L.RegVar("maxLength", get_maxLength, set_maxLength);
		L.RegVar("caretPosition", get_caretPosition, set_caretPosition);
		L.RegVar("promptText", get_promptText, set_promptText);
		L.RegVar("keyboardType", get_keyboardType, set_keyboardType);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _CreateFairyGUI_GTextInput(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 0)
			{
				FairyGUI.GTextInput obj = new FairyGUI.GTextInput();
				ToLua.PushObject(L, obj);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to ctor method: FairyGUI.GTextInput.New");
			}
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int ReplaceSelection(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			FairyGUI.GTextInput obj = (FairyGUI.GTextInput)ToLua.CheckObject(L, 1, typeof(FairyGUI.GTextInput));
			string arg0 = ToLua.CheckString(L, 2);
			obj.ReplaceSelection(arg0);
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Setup_BeforeAdd(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			FairyGUI.GTextInput obj = (FairyGUI.GTextInput)ToLua.CheckObject(L, 1, typeof(FairyGUI.GTextInput));
			FairyGUI.Utils.XML arg0 = (FairyGUI.Utils.XML)ToLua.CheckObject(L, 2, typeof(FairyGUI.Utils.XML));
			obj.Setup_BeforeAdd(arg0);
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Setup_AfterAdd(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			FairyGUI.GTextInput obj = (FairyGUI.GTextInput)ToLua.CheckObject(L, 1, typeof(FairyGUI.GTextInput));
			FairyGUI.Utils.XML arg0 = (FairyGUI.Utils.XML)ToLua.CheckObject(L, 2, typeof(FairyGUI.Utils.XML));
			obj.Setup_AfterAdd(arg0);
			return 0;
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
	static int get_editable(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			FairyGUI.GTextInput obj = (FairyGUI.GTextInput)o;
			bool ret = obj.editable;
			LuaDLL.lua_pushboolean(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o == null ? "attempt to index editable on a nil value" : e.Message);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_maxLength(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			FairyGUI.GTextInput obj = (FairyGUI.GTextInput)o;
			int ret = obj.maxLength;
			LuaDLL.lua_pushinteger(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o == null ? "attempt to index maxLength on a nil value" : e.Message);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_caretPosition(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			FairyGUI.GTextInput obj = (FairyGUI.GTextInput)o;
			int ret = obj.caretPosition;
			LuaDLL.lua_pushinteger(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o == null ? "attempt to index caretPosition on a nil value" : e.Message);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_promptText(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			FairyGUI.GTextInput obj = (FairyGUI.GTextInput)o;
			string ret = obj.promptText;
			LuaDLL.lua_pushstring(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o == null ? "attempt to index promptText on a nil value" : e.Message);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_keyboardType(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			FairyGUI.GTextInput obj = (FairyGUI.GTextInput)o;
			int ret = obj.keyboardType;
			LuaDLL.lua_pushinteger(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o == null ? "attempt to index keyboardType on a nil value" : e.Message);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_editable(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			FairyGUI.GTextInput obj = (FairyGUI.GTextInput)o;
			bool arg0 = LuaDLL.luaL_checkboolean(L, 2);
			obj.editable = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o == null ? "attempt to index editable on a nil value" : e.Message);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_maxLength(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			FairyGUI.GTextInput obj = (FairyGUI.GTextInput)o;
			int arg0 = (int)LuaDLL.luaL_checknumber(L, 2);
			obj.maxLength = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o == null ? "attempt to index maxLength on a nil value" : e.Message);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_caretPosition(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			FairyGUI.GTextInput obj = (FairyGUI.GTextInput)o;
			int arg0 = (int)LuaDLL.luaL_checknumber(L, 2);
			obj.caretPosition = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o == null ? "attempt to index caretPosition on a nil value" : e.Message);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_promptText(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			FairyGUI.GTextInput obj = (FairyGUI.GTextInput)o;
			string arg0 = ToLua.CheckString(L, 2);
			obj.promptText = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o == null ? "attempt to index promptText on a nil value" : e.Message);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_keyboardType(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			FairyGUI.GTextInput obj = (FairyGUI.GTextInput)o;
			int arg0 = (int)LuaDLL.luaL_checknumber(L, 2);
			obj.keyboardType = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o == null ? "attempt to index keyboardType on a nil value" : e.Message);
		}
	}
}
