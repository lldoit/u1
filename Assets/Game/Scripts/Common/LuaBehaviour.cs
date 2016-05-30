using UnityEngine;
using LuaInterface;
using System.Collections;
using System.Collections.Generic;
using System;
using FairyGUI;

namespace LuaFramework
{
    public class LuaBehaviour : View
    {
        private LuaState mLuaState = null;
        private LuaTable mLuaTable = null;

        private Dictionary<string, LuaFunction> mButtonCallbacks = new Dictionary<string, LuaFunction>();

        private LuaFunction mFixedUpdateFunc = null;
        private LuaFunction mUpdateFunc = null;
        private LuaFunction mLateUpdateFunc = null;

        private bool mIsStarted = false;


        private void SafeRelease(ref LuaFunction func)
        {
            if (func != null)
            {
                func.Dispose();
                func = null;
            }
        }

        private void SafeRelease(ref LuaTable table)
        {
            if (table != null)
            {
                table.Dispose();
                table = null;
            }
        }

        private bool CheckValid()
        {
            if (mLuaState == null) return false;
            if (mLuaTable == null) return false;
            return true;
        }

        public void Init(LuaTable tb, GObject ui)
        {
            mLuaState = MyLuaClient.GetMainState();
            if (mLuaState == null) return;

            if (tb == null)
            {
                mLuaTable = mLuaState.GetTable(name);
            }
            else
            {
                mLuaTable = tb;
            }
            if (mLuaTable == null)
            {
                Debug.LogWarning("mLuaTable is null:" + name);
                return;
            }
            mLuaTable["gameObject"] = gameObject;
            mLuaTable["ui"] = ui;
            mLuaTable["lua_behaviour"] = this;

            LuaFunction awakeFunc = mLuaTable.GetLuaFunction("Awake") as LuaFunction;
            if (awakeFunc != null)
            {
                awakeFunc.BeginPCall();
                awakeFunc.Push(mLuaTable);
                awakeFunc.PCall();
                awakeFunc.EndPCall();

                awakeFunc.Dispose();
                awakeFunc = null;
            }

            mUpdateFunc = mLuaTable.GetLuaFunction("Update") as LuaFunction;
            mFixedUpdateFunc = mLuaTable.GetLuaFunction("FixedUpdate") as LuaFunction;
            mLateUpdateFunc = mLuaTable.GetLuaFunction("LateUpdate") as LuaFunction;
        }

        private void Start()
        {
            if (!CheckValid()) return;

            LuaFunction startFunc = mLuaTable.GetLuaFunction("Start") as LuaFunction;
            if (startFunc != null)
            {
                startFunc.BeginPCall();
                startFunc.Push(mLuaTable);
                startFunc.PCall();
                startFunc.EndPCall();

                startFunc.Dispose();
                startFunc = null;
            }

            AddUpdate();
            mIsStarted = true;
        }

        private void AddUpdate()
        {
            if (!CheckValid()) return;

            LuaLooper luaLooper = MyLuaClient.Instance.GetLooper();
            if (luaLooper == null) return;

            if (mUpdateFunc != null)
            {
                luaLooper.UpdateEvent.Add(mUpdateFunc, mLuaTable);
            }

            if (mLateUpdateFunc != null)
            {
                luaLooper.LateUpdateEvent.Add(mLateUpdateFunc, mLuaTable);
            }

            if (mFixedUpdateFunc != null)
            {
                luaLooper.FixedUpdateEvent.Add(mFixedUpdateFunc, mLuaTable);
            }
        }

        private void RemoveUpdate()
        {
            if (!CheckValid()) return;

            LuaLooper luaLooper = MyLuaClient.Instance.GetLooper();
            if (luaLooper == null) return;

            if (mUpdateFunc != null)
            {
                luaLooper.UpdateEvent.Remove(mUpdateFunc, mLuaTable);
            }
            if (mLateUpdateFunc != null)
            {
                luaLooper.LateUpdateEvent.Remove(mLateUpdateFunc, mLuaTable);
            }
            if (mFixedUpdateFunc != null)
            {
                luaLooper.FixedUpdateEvent.Remove(mFixedUpdateFunc, mLuaTable);
            }
        }

        public void AddClick(GObject go, LuaFunction luafunc)
        {
            if (!CheckValid()) return;
            if (go == null || luafunc == null) return;
            if (!mButtonCallbacks.ContainsKey(go.name))
            {
                mButtonCallbacks.Add(go.name, luafunc);
                go.onClick.Add(
                    delegate ()
                    {
                        luafunc.BeginPCall();
                        luafunc.Push(go);
                        luafunc.PCall();
                        luafunc.EndPCall();
                    });
            }
        }

        public void RemoveClick(GObject go)
        {
            if (!CheckValid()) return;
            if (go == null) return;
            LuaFunction luafunc = null;
            if (mButtonCallbacks.TryGetValue(go.name, out luafunc))
            {
                luafunc.Dispose();
                luafunc = null;
                mButtonCallbacks.Remove(go.name);
            }
        }

        public void ClearClick()
        {
            foreach (var de in mButtonCallbacks)
            {
                if (de.Value != null)
                {
                    de.Value.Dispose();
                }
            }
            mButtonCallbacks.Clear();
        }

        //-----------------------------------------------------------------
        protected void OnDestroy()
        {
            if (!CheckValid()) return;
            ClearClick();
            LuaFunction destroyFunc = mLuaTable.GetLuaFunction("OnDestroy") as LuaFunction;
            if (destroyFunc != null)
            {
                destroyFunc.BeginPCall();
                destroyFunc.PCall();
                destroyFunc.EndPCall();

                destroyFunc.Dispose();
                destroyFunc = null;
            }

            SafeRelease(ref mFixedUpdateFunc);
            SafeRelease(ref mUpdateFunc);
            SafeRelease(ref mLateUpdateFunc);
            SafeRelease(ref mLuaTable);

#if ASYNC_MODE
            string abName = name.ToLower();
            ResManager.UnloadAssetBundle(abName + AppConst.ExtName);
#endif

            Util.ClearMemory();
            Debug.Log("~" + name + " was destroy!");
        }
    }
}