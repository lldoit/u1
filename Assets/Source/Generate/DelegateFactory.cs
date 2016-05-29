﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using System.Collections.Generic;
using LuaInterface;

public static class DelegateFactory
{
	public delegate Delegate DelegateValue(LuaFunction func);
	public static Dictionary<Type, DelegateValue> dict = new Dictionary<Type, DelegateValue>();

	static DelegateFactory()
	{
		Register();
	}

	[NoToLuaAttribute]
	public static void Register()
	{
		dict.Clear();
		dict.Add(typeof(System.Action), System_Action);
		dict.Add(typeof(UnityEngine.Events.UnityAction), UnityEngine_Events_UnityAction);
		dict.Add(typeof(UnityEngine.Camera.CameraCallback), UnityEngine_Camera_CameraCallback);
		dict.Add(typeof(UnityEngine.Application.LogCallback), UnityEngine_Application_LogCallback);
		dict.Add(typeof(UnityEngine.Application.AdvertisingIdentifierCallback), UnityEngine_Application_AdvertisingIdentifierCallback);
		dict.Add(typeof(UnityEngine.AudioClip.PCMReaderCallback), UnityEngine_AudioClip_PCMReaderCallback);
		dict.Add(typeof(UnityEngine.AudioClip.PCMSetPositionCallback), UnityEngine_AudioClip_PCMSetPositionCallback);
		dict.Add(typeof(System.Action<NotiData>), System_Action_NotiData);
		dict.Add(typeof(FairyGUI.EventCallback0), FairyGUI_EventCallback0);
		dict.Add(typeof(FairyGUI.EventCallback1), FairyGUI_EventCallback1);
		dict.Add(typeof(FairyGUI.ListItemRenderer), FairyGUI_ListItemRenderer);
		dict.Add(typeof(FairyGUI.PlayCompleteCallback), FairyGUI_PlayCompleteCallback);
		dict.Add(typeof(FairyGUI.TransitionHook), FairyGUI_TransitionHook);
		dict.Add(typeof(FairyGUI.UIPackage.LoadResource), FairyGUI_UIPackage_LoadResource);
		dict.Add(typeof(FairyGUI.GObjectPool.InitCallbackDelegate), FairyGUI_GObjectPool_InitCallbackDelegate);
	}

    [NoToLuaAttribute]
    public static Delegate CreateDelegate(Type t, LuaFunction func = null)
    {
        DelegateValue create = null;

        if (!dict.TryGetValue(t, out create))
        {
            throw new LuaException(string.Format("Delegate {0} not register", LuaMisc.GetTypeName(t)));            
        }
        
        return create(func);        
    }

    [NoToLuaAttribute]
    public static Delegate RemoveDelegate(Delegate obj, LuaFunction func)
    {
        LuaState state = func.GetLuaState();
        Delegate[] ds = obj.GetInvocationList();

        for (int i = 0; i < ds.Length; i++)
        {
            LuaDelegate ld = ds[i].Target as LuaDelegate;

            if (ld != null && ld.func == func)
            {
                obj = Delegate.Remove(obj, ds[i]);
                state.DelayDispose(ld.func);
                break;
            }
        }

        return obj;
    }

	class System_Action_Event : LuaDelegate
	{
		public System_Action_Event(LuaFunction func) : base(func) { }

		public void Call()
		{
			func.Call();
		}
	}

	public static Delegate System_Action(LuaFunction func)
	{
		if (func == null)
		{
			System.Action fn = delegate { };
			return fn;
		}

		System.Action d = (new System_Action_Event(func)).Call;
		return d;
	}

	class UnityEngine_Events_UnityAction_Event : LuaDelegate
	{
		public UnityEngine_Events_UnityAction_Event(LuaFunction func) : base(func) { }

		public void Call()
		{
			func.Call();
		}
	}

	public static Delegate UnityEngine_Events_UnityAction(LuaFunction func)
	{
		if (func == null)
		{
			UnityEngine.Events.UnityAction fn = delegate { };
			return fn;
		}

		UnityEngine.Events.UnityAction d = (new UnityEngine_Events_UnityAction_Event(func)).Call;
		return d;
	}

	class UnityEngine_Camera_CameraCallback_Event : LuaDelegate
	{
		public UnityEngine_Camera_CameraCallback_Event(LuaFunction func) : base(func) { }

		public void Call(UnityEngine.Camera param0)
		{
			func.BeginPCall();
			func.Push(param0);
			func.PCall();
			func.EndPCall();
		}
	}

	public static Delegate UnityEngine_Camera_CameraCallback(LuaFunction func)
	{
		if (func == null)
		{
			UnityEngine.Camera.CameraCallback fn = delegate { };
			return fn;
		}

		UnityEngine.Camera.CameraCallback d = (new UnityEngine_Camera_CameraCallback_Event(func)).Call;
		return d;
	}

	class UnityEngine_Application_LogCallback_Event : LuaDelegate
	{
		public UnityEngine_Application_LogCallback_Event(LuaFunction func) : base(func) { }

		public void Call(string param0,string param1,UnityEngine.LogType param2)
		{
			func.BeginPCall();
			func.Push(param0);
			func.Push(param1);
			func.Push(param2);
			func.PCall();
			func.EndPCall();
		}
	}

	public static Delegate UnityEngine_Application_LogCallback(LuaFunction func)
	{
		if (func == null)
		{
			UnityEngine.Application.LogCallback fn = delegate { };
			return fn;
		}

		UnityEngine.Application.LogCallback d = (new UnityEngine_Application_LogCallback_Event(func)).Call;
		return d;
	}

	class UnityEngine_Application_AdvertisingIdentifierCallback_Event : LuaDelegate
	{
		public UnityEngine_Application_AdvertisingIdentifierCallback_Event(LuaFunction func) : base(func) { }

		public void Call(string param0,bool param1,string param2)
		{
			func.BeginPCall();
			func.Push(param0);
			func.Push(param1);
			func.Push(param2);
			func.PCall();
			func.EndPCall();
		}
	}

	public static Delegate UnityEngine_Application_AdvertisingIdentifierCallback(LuaFunction func)
	{
		if (func == null)
		{
			UnityEngine.Application.AdvertisingIdentifierCallback fn = delegate { };
			return fn;
		}

		UnityEngine.Application.AdvertisingIdentifierCallback d = (new UnityEngine_Application_AdvertisingIdentifierCallback_Event(func)).Call;
		return d;
	}

	class UnityEngine_AudioClip_PCMReaderCallback_Event : LuaDelegate
	{
		public UnityEngine_AudioClip_PCMReaderCallback_Event(LuaFunction func) : base(func) { }

		public void Call(float[] param0)
		{
			func.BeginPCall();
			func.Push(param0);
			func.PCall();
			func.EndPCall();
		}
	}

	public static Delegate UnityEngine_AudioClip_PCMReaderCallback(LuaFunction func)
	{
		if (func == null)
		{
			UnityEngine.AudioClip.PCMReaderCallback fn = delegate { };
			return fn;
		}

		UnityEngine.AudioClip.PCMReaderCallback d = (new UnityEngine_AudioClip_PCMReaderCallback_Event(func)).Call;
		return d;
	}

	class UnityEngine_AudioClip_PCMSetPositionCallback_Event : LuaDelegate
	{
		public UnityEngine_AudioClip_PCMSetPositionCallback_Event(LuaFunction func) : base(func) { }

		public void Call(int param0)
		{
			func.BeginPCall();
			func.Push(param0);
			func.PCall();
			func.EndPCall();
		}
	}

	public static Delegate UnityEngine_AudioClip_PCMSetPositionCallback(LuaFunction func)
	{
		if (func == null)
		{
			UnityEngine.AudioClip.PCMSetPositionCallback fn = delegate { };
			return fn;
		}

		UnityEngine.AudioClip.PCMSetPositionCallback d = (new UnityEngine_AudioClip_PCMSetPositionCallback_Event(func)).Call;
		return d;
	}

	class System_Action_NotiData_Event : LuaDelegate
	{
		public System_Action_NotiData_Event(LuaFunction func) : base(func) { }

		public void Call(NotiData param0)
		{
			func.BeginPCall();
			func.PushObject(param0);
			func.PCall();
			func.EndPCall();
		}
	}

	public static Delegate System_Action_NotiData(LuaFunction func)
	{
		if (func == null)
		{
			System.Action<NotiData> fn = delegate { };
			return fn;
		}

		System.Action<NotiData> d = (new System_Action_NotiData_Event(func)).Call;
		return d;
	}

	class FairyGUI_EventCallback0_Event : LuaDelegate
	{
		public FairyGUI_EventCallback0_Event(LuaFunction func) : base(func) { }

		public void Call()
		{
			func.Call();
		}
	}

	public static Delegate FairyGUI_EventCallback0(LuaFunction func)
	{
		if (func == null)
		{
			FairyGUI.EventCallback0 fn = delegate { };
			return fn;
		}

		FairyGUI.EventCallback0 d = (new FairyGUI_EventCallback0_Event(func)).Call;
		return d;
	}

	class FairyGUI_EventCallback1_Event : LuaDelegate
	{
		public FairyGUI_EventCallback1_Event(LuaFunction func) : base(func) { }

		public void Call(FairyGUI.EventContext param0)
		{
			func.BeginPCall();
			func.PushObject(param0);
			func.PCall();
			func.EndPCall();
		}
	}

	public static Delegate FairyGUI_EventCallback1(LuaFunction func)
	{
		if (func == null)
		{
			FairyGUI.EventCallback1 fn = delegate { };
			return fn;
		}

		FairyGUI.EventCallback1 d = (new FairyGUI_EventCallback1_Event(func)).Call;
		return d;
	}

	class FairyGUI_ListItemRenderer_Event : LuaDelegate
	{
		public FairyGUI_ListItemRenderer_Event(LuaFunction func) : base(func) { }

		public void Call(int param0,FairyGUI.GObject param1)
		{
			func.BeginPCall();
			func.Push(param0);
			func.PushObject(param1);
			func.PCall();
			func.EndPCall();
		}
	}

	public static Delegate FairyGUI_ListItemRenderer(LuaFunction func)
	{
		if (func == null)
		{
			FairyGUI.ListItemRenderer fn = delegate { };
			return fn;
		}

		FairyGUI.ListItemRenderer d = (new FairyGUI_ListItemRenderer_Event(func)).Call;
		return d;
	}

	class FairyGUI_PlayCompleteCallback_Event : LuaDelegate
	{
		public FairyGUI_PlayCompleteCallback_Event(LuaFunction func) : base(func) { }

		public void Call()
		{
			func.Call();
		}
	}

	public static Delegate FairyGUI_PlayCompleteCallback(LuaFunction func)
	{
		if (func == null)
		{
			FairyGUI.PlayCompleteCallback fn = delegate { };
			return fn;
		}

		FairyGUI.PlayCompleteCallback d = (new FairyGUI_PlayCompleteCallback_Event(func)).Call;
		return d;
	}

	class FairyGUI_TransitionHook_Event : LuaDelegate
	{
		public FairyGUI_TransitionHook_Event(LuaFunction func) : base(func) { }

		public void Call()
		{
			func.Call();
		}
	}

	public static Delegate FairyGUI_TransitionHook(LuaFunction func)
	{
		if (func == null)
		{
			FairyGUI.TransitionHook fn = delegate { };
			return fn;
		}

		FairyGUI.TransitionHook d = (new FairyGUI_TransitionHook_Event(func)).Call;
		return d;
	}

	class FairyGUI_UIPackage_LoadResource_Event : LuaDelegate
	{
		public FairyGUI_UIPackage_LoadResource_Event(LuaFunction func) : base(func) { }

		public UnityEngine.Object Call(string param0,string param1,System.Type param2)
		{
			func.BeginPCall();
			func.Push(param0);
			func.Push(param1);
			func.Push(param2);
			func.PCall();
			UnityEngine.Object ret = (UnityEngine.Object)func.CheckObject(typeof(UnityEngine.Object));
			func.EndPCall();
			return ret;
		}
	}

	public static Delegate FairyGUI_UIPackage_LoadResource(LuaFunction func)
	{
		if (func == null)
		{
			FairyGUI.UIPackage.LoadResource fn = delegate { return null; };
			return fn;
		}

		FairyGUI.UIPackage.LoadResource d = (new FairyGUI_UIPackage_LoadResource_Event(func)).Call;
		return d;
	}

	class FairyGUI_GObjectPool_InitCallbackDelegate_Event : LuaDelegate
	{
		public FairyGUI_GObjectPool_InitCallbackDelegate_Event(LuaFunction func) : base(func) { }

		public void Call(FairyGUI.GObject param0)
		{
			func.BeginPCall();
			func.PushObject(param0);
			func.PCall();
			func.EndPCall();
		}
	}

	public static Delegate FairyGUI_GObjectPool_InitCallbackDelegate(LuaFunction func)
	{
		if (func == null)
		{
			FairyGUI.GObjectPool.InitCallbackDelegate fn = delegate { };
			return fn;
		}

		FairyGUI.GObjectPool.InitCallbackDelegate d = (new FairyGUI_GObjectPool_InitCallbackDelegate_Event(func)).Call;
		return d;
	}

}

