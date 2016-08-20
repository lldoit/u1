﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class FairyGUI_TransitionWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(FairyGUI.Transition), typeof(System.Object));
		L.RegFunction("Play", Play);
		L.RegFunction("PlayReverse", PlayReverse);
		L.RegFunction("Stop", Stop);
		L.RegFunction("SetValue", SetValue);
		L.RegFunction("SetHook", SetHook);
		L.RegFunction("ClearHooks", ClearHooks);
		L.RegFunction("SetTarget", SetTarget);
		L.RegFunction("Copy", Copy);
		L.RegFunction("Setup", Setup);
		L.RegFunction("New", _CreateFairyGUI_Transition);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.RegVar("autoPlay", get_autoPlay, set_autoPlay);
		L.RegVar("autoPlayRepeat", get_autoPlayRepeat, set_autoPlayRepeat);
		L.RegVar("autoPlayDelay", get_autoPlayDelay, set_autoPlayDelay);
		L.RegVar("name", get_name, null);
		L.RegVar("playing", get_playing, null);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _CreateFairyGUI_Transition(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 1)
			{
				FairyGUI.GComponent arg0 = (FairyGUI.GComponent)ToLua.CheckObject(L, 1, typeof(FairyGUI.GComponent));
				FairyGUI.Transition obj = new FairyGUI.Transition(arg0);
				ToLua.PushObject(L, obj);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to ctor method: FairyGUI.Transition.New");
			}
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Play(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 1 && TypeChecker.CheckTypes(L, 1, typeof(FairyGUI.Transition)))
			{
				FairyGUI.Transition obj = (FairyGUI.Transition)ToLua.ToObject(L, 1);
				obj.Play();
				return 0;
			}
			else if (count == 2 && TypeChecker.CheckTypes(L, 1, typeof(FairyGUI.Transition), typeof(FairyGUI.PlayCompleteCallback)))
			{
				FairyGUI.Transition obj = (FairyGUI.Transition)ToLua.ToObject(L, 1);
				FairyGUI.PlayCompleteCallback arg0 = null;
				LuaTypes funcType2 = LuaDLL.lua_type(L, 2);

				if (funcType2 != LuaTypes.LUA_TFUNCTION)
				{
					 arg0 = (FairyGUI.PlayCompleteCallback)ToLua.ToObject(L, 2);
				}
				else
				{
					LuaFunction func = ToLua.ToLuaFunction(L, 2);
					arg0 = DelegateFactory.CreateDelegate(typeof(FairyGUI.PlayCompleteCallback), func) as FairyGUI.PlayCompleteCallback;
				}

				obj.Play(arg0);
				return 0;
			}
			else if (count == 4 && TypeChecker.CheckTypes(L, 1, typeof(FairyGUI.Transition), typeof(int), typeof(float), typeof(FairyGUI.PlayCompleteCallback)))
			{
				FairyGUI.Transition obj = (FairyGUI.Transition)ToLua.ToObject(L, 1);
				int arg0 = (int)LuaDLL.lua_tonumber(L, 2);
				float arg1 = (float)LuaDLL.lua_tonumber(L, 3);
				FairyGUI.PlayCompleteCallback arg2 = null;
				LuaTypes funcType4 = LuaDLL.lua_type(L, 4);

				if (funcType4 != LuaTypes.LUA_TFUNCTION)
				{
					 arg2 = (FairyGUI.PlayCompleteCallback)ToLua.ToObject(L, 4);
				}
				else
				{
					LuaFunction func = ToLua.ToLuaFunction(L, 4);
					arg2 = DelegateFactory.CreateDelegate(typeof(FairyGUI.PlayCompleteCallback), func) as FairyGUI.PlayCompleteCallback;
				}

				obj.Play(arg0, arg1, arg2);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: FairyGUI.Transition.Play");
			}
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int PlayReverse(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 1 && TypeChecker.CheckTypes(L, 1, typeof(FairyGUI.Transition)))
			{
				FairyGUI.Transition obj = (FairyGUI.Transition)ToLua.ToObject(L, 1);
				obj.PlayReverse();
				return 0;
			}
			else if (count == 2 && TypeChecker.CheckTypes(L, 1, typeof(FairyGUI.Transition), typeof(FairyGUI.PlayCompleteCallback)))
			{
				FairyGUI.Transition obj = (FairyGUI.Transition)ToLua.ToObject(L, 1);
				FairyGUI.PlayCompleteCallback arg0 = null;
				LuaTypes funcType2 = LuaDLL.lua_type(L, 2);

				if (funcType2 != LuaTypes.LUA_TFUNCTION)
				{
					 arg0 = (FairyGUI.PlayCompleteCallback)ToLua.ToObject(L, 2);
				}
				else
				{
					LuaFunction func = ToLua.ToLuaFunction(L, 2);
					arg0 = DelegateFactory.CreateDelegate(typeof(FairyGUI.PlayCompleteCallback), func) as FairyGUI.PlayCompleteCallback;
				}

				obj.PlayReverse(arg0);
				return 0;
			}
			else if (count == 4 && TypeChecker.CheckTypes(L, 1, typeof(FairyGUI.Transition), typeof(int), typeof(float), typeof(FairyGUI.PlayCompleteCallback)))
			{
				FairyGUI.Transition obj = (FairyGUI.Transition)ToLua.ToObject(L, 1);
				int arg0 = (int)LuaDLL.lua_tonumber(L, 2);
				float arg1 = (float)LuaDLL.lua_tonumber(L, 3);
				FairyGUI.PlayCompleteCallback arg2 = null;
				LuaTypes funcType4 = LuaDLL.lua_type(L, 4);

				if (funcType4 != LuaTypes.LUA_TFUNCTION)
				{
					 arg2 = (FairyGUI.PlayCompleteCallback)ToLua.ToObject(L, 4);
				}
				else
				{
					LuaFunction func = ToLua.ToLuaFunction(L, 4);
					arg2 = DelegateFactory.CreateDelegate(typeof(FairyGUI.PlayCompleteCallback), func) as FairyGUI.PlayCompleteCallback;
				}

				obj.PlayReverse(arg0, arg1, arg2);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: FairyGUI.Transition.PlayReverse");
			}
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Stop(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 1 && TypeChecker.CheckTypes(L, 1, typeof(FairyGUI.Transition)))
			{
				FairyGUI.Transition obj = (FairyGUI.Transition)ToLua.ToObject(L, 1);
				obj.Stop();
				return 0;
			}
			else if (count == 3 && TypeChecker.CheckTypes(L, 1, typeof(FairyGUI.Transition), typeof(bool), typeof(bool)))
			{
				FairyGUI.Transition obj = (FairyGUI.Transition)ToLua.ToObject(L, 1);
				bool arg0 = LuaDLL.lua_toboolean(L, 2);
				bool arg1 = LuaDLL.lua_toboolean(L, 3);
				obj.Stop(arg0, arg1);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: FairyGUI.Transition.Stop");
			}
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetValue(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);
			FairyGUI.Transition obj = (FairyGUI.Transition)ToLua.CheckObject(L, 1, typeof(FairyGUI.Transition));
			string arg0 = ToLua.CheckString(L, 2);
			object[] arg1 = ToLua.ToParamsObject(L, 3, count - 2);
			obj.SetValue(arg0, arg1);
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetHook(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 3);
			FairyGUI.Transition obj = (FairyGUI.Transition)ToLua.CheckObject(L, 1, typeof(FairyGUI.Transition));
			string arg0 = ToLua.CheckString(L, 2);
			FairyGUI.TransitionHook arg1 = null;
			LuaTypes funcType3 = LuaDLL.lua_type(L, 3);

			if (funcType3 != LuaTypes.LUA_TFUNCTION)
			{
				 arg1 = (FairyGUI.TransitionHook)ToLua.CheckObject(L, 3, typeof(FairyGUI.TransitionHook));
			}
			else
			{
				LuaFunction func = ToLua.ToLuaFunction(L, 3);
				arg1 = DelegateFactory.CreateDelegate(typeof(FairyGUI.TransitionHook), func) as FairyGUI.TransitionHook;
			}

			obj.SetHook(arg0, arg1);
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int ClearHooks(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			FairyGUI.Transition obj = (FairyGUI.Transition)ToLua.CheckObject(L, 1, typeof(FairyGUI.Transition));
			obj.ClearHooks();
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetTarget(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 3);
			FairyGUI.Transition obj = (FairyGUI.Transition)ToLua.CheckObject(L, 1, typeof(FairyGUI.Transition));
			string arg0 = ToLua.CheckString(L, 2);
			FairyGUI.GObject arg1 = (FairyGUI.GObject)ToLua.CheckObject(L, 3, typeof(FairyGUI.GObject));
			obj.SetTarget(arg0, arg1);
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Copy(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			FairyGUI.Transition obj = (FairyGUI.Transition)ToLua.CheckObject(L, 1, typeof(FairyGUI.Transition));
			FairyGUI.Transition arg0 = (FairyGUI.Transition)ToLua.CheckObject(L, 2, typeof(FairyGUI.Transition));
			obj.Copy(arg0);
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Setup(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			FairyGUI.Transition obj = (FairyGUI.Transition)ToLua.CheckObject(L, 1, typeof(FairyGUI.Transition));
			FairyGUI.Utils.XML arg0 = (FairyGUI.Utils.XML)ToLua.CheckObject(L, 2, typeof(FairyGUI.Utils.XML));
			obj.Setup(arg0);
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_autoPlay(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			FairyGUI.Transition obj = (FairyGUI.Transition)o;
			bool ret = obj.autoPlay;
			LuaDLL.lua_pushboolean(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o == null ? "attempt to index autoPlay on a nil value" : e.Message);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_autoPlayRepeat(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			FairyGUI.Transition obj = (FairyGUI.Transition)o;
			int ret = obj.autoPlayRepeat;
			LuaDLL.lua_pushinteger(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o == null ? "attempt to index autoPlayRepeat on a nil value" : e.Message);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_autoPlayDelay(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			FairyGUI.Transition obj = (FairyGUI.Transition)o;
			float ret = obj.autoPlayDelay;
			LuaDLL.lua_pushnumber(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o == null ? "attempt to index autoPlayDelay on a nil value" : e.Message);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_name(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			FairyGUI.Transition obj = (FairyGUI.Transition)o;
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
	static int get_playing(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			FairyGUI.Transition obj = (FairyGUI.Transition)o;
			bool ret = obj.playing;
			LuaDLL.lua_pushboolean(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o == null ? "attempt to index playing on a nil value" : e.Message);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_autoPlay(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			FairyGUI.Transition obj = (FairyGUI.Transition)o;
			bool arg0 = LuaDLL.luaL_checkboolean(L, 2);
			obj.autoPlay = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o == null ? "attempt to index autoPlay on a nil value" : e.Message);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_autoPlayRepeat(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			FairyGUI.Transition obj = (FairyGUI.Transition)o;
			int arg0 = (int)LuaDLL.luaL_checknumber(L, 2);
			obj.autoPlayRepeat = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o == null ? "attempt to index autoPlayRepeat on a nil value" : e.Message);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_autoPlayDelay(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			FairyGUI.Transition obj = (FairyGUI.Transition)o;
			float arg0 = (float)LuaDLL.luaL_checknumber(L, 2);
			obj.autoPlayDelay = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o == null ? "attempt to index autoPlayDelay on a nil value" : e.Message);
		}
	}
}

