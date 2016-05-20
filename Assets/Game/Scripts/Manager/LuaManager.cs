using UnityEngine;
using System.Collections;
using LuaInterface;

namespace LuaFramework 
{
    public class MyLuaClient : LuaClient
    {
        protected override void LoadLuaFiles()
        {
        }

        protected override void OpenLibs()
        {
            base.OpenLibs();
            OpenCJson();
        }

        public void OnInit()
        {
            OnLoadFinished();
        }
    }

    public class LuaManager : Manager
    {
        private LuaState lua = null;
        private LuaLoader loader = null;
        private MyLuaClient mMyLuaClient;

        // Use this for initialization
        void Awake()
        {
            loader = new LuaLoader();
            mMyLuaClient = gameObject.AddComponent<MyLuaClient>();
            lua = MyLuaClient.GetMainState();
        }

        public void InitStart() 
        {
            InitLuaPath();
            InitLuaBundle();

            mMyLuaClient.OnInit();
        }

        /// <summary>
        /// 初始化Lua代码加载路径
        /// </summary>
        void InitLuaPath() 
        {
            if (AppConst.DebugMode) 
            {
                lua.AddSearchPath(Application.dataPath + "/Lua");
                lua.AddSearchPath(Application.dataPath + "/ToLua/Lua");
            } 
            else
            {
                lua.AddSearchPath(Util.DataPath + "lua");
            }
        }

        /// <summary>
        /// 初始化LuaBundle
        /// </summary>
        void InitLuaBundle() 
        {
            if (loader.beZip) 
            {
                loader.AddBundle("lua" + AppConst.ExtName);
                loader.AddBundle("lua_system" + AppConst.ExtName);
                loader.AddBundle("lua_system_reflection" + AppConst.ExtName);
                loader.AddBundle("lua_unityengine" + AppConst.ExtName);
                loader.AddBundle("lua_common" + AppConst.ExtName);
                loader.AddBundle("lua_logic" + AppConst.ExtName);
                loader.AddBundle("lua_view" + AppConst.ExtName);
                loader.AddBundle("lua_controller" + AppConst.ExtName);
                loader.AddBundle("lua_misc" + AppConst.ExtName);

                loader.AddBundle("lua_protobuf" + AppConst.ExtName);
                loader.AddBundle("lua_3rd_cjson" + AppConst.ExtName);
                loader.AddBundle("lua_3rd_luabitop" + AppConst.ExtName);
                loader.AddBundle("lua_3rd_pblua" + AppConst.ExtName);
            }
        }

        public object[] DoFile(string filename) 
        {
            return lua.DoFile(filename);
        }

        // Update is called once per frame
        public object[] CallFunction(string funcName, params object[] args) 
        {
            LuaFunction func = lua.GetFunction(funcName);
            if (func != null)
                return func.Call(args);

            return null;
        }

        public void LuaGC()
        {
            if (lua != null)
                lua.LuaGC(LuaGCOptions.LUA_GCCOLLECT);
        }

        public void Close() 
        {
            if (loader != null)
            {
                loader.Dispose();
                loader = null;
            }
        }
    }
}