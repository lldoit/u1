using UnityEngine;
using System.Collections;
using LuaInterface;

namespace LuaFramework {
    public class LuaManager : Manager {
        private LuaState lua;
        private LuaLoader loader;
        private LuaLooper loop = null;

        // Use this for initialization
        void Awake() {
            loader = new LuaLoader();
            lua = new LuaState();
            this.OpenLibs();
            lua.LuaSetTop(0);

            LuaBinder.Bind(lua);
            LuaCoroutine.Register(lua, this);
        }

        public void InitStart() {
            InitLuaPath();
            InitLuaBundle();
            this.lua.Start();    //启动LUAVM
            this.StartMain();
            this.StartLooper();
        }

        void StartLooper() {
            loop = gameObject.AddComponent<LuaLooper>();
            loop.luaState = lua;
        }

        //cjson 比较特殊，只new了一个table，没有注册库，这里注册一下
        protected void OpenCJson()
        {
            lua.LuaGetField(LuaIndexes.LUA_REGISTRYINDEX, "_LOADED");
            lua.OpenLibs(LuaDLL.luaopen_cjson);
            lua.LuaSetField(-2, "cjson");

            lua.OpenLibs(LuaDLL.luaopen_cjson_safe);
            lua.LuaSetField(-2, "cjson.safe");
        }

        void StartMain() {
            lua.DoFile("Main.lua");

            LuaFunction main = lua.GetFunction("Main");
            main.Call();
            main.Dispose();
            main = null;    
        }
        
        /// <summary>
        /// 初始化加载第三方库
        /// </summary>
        void OpenLibs() {
            lua.OpenLibs(LuaDLL.luaopen_pb);
            lua.OpenLibs(LuaDLL.luaopen_lpeg);
            lua.OpenLibs(LuaDLL.luaopen_bit);

            // 这两个库需要自己继承，原tolua并没有，如果只用了pb，可以不用
            lua.OpenLibs(LuaDLL.luaopen_sproto_core);
            lua.OpenLibs(LuaDLL.luaopen_protobuf_c);

            // 貌似非必须，只在调试时需要，可以做个开关
            lua.OpenLibs(LuaDLL.luaopen_socket_core);

            this.OpenCJson();
        }

        /// <summary>
        /// 初始化Lua代码加载路径
        /// </summary>
        void InitLuaPath() {
            if (AppConst.DebugMode) {
                lua.AddSearchPath(Application.dataPath + "/Lua");
                lua.AddSearchPath(Application.dataPath + "/ToLua/Lua");
            } else {
                lua.AddSearchPath(Util.DataPath + "lua");
            }
        }

        /// <summary>
        /// 初始化LuaBundle
        /// </summary>
        void InitLuaBundle() {
            if (loader.beZip) {
                loader.AddBundle("lua" + AppConst.ExtName);
                loader.AddBundle("lua_system" + AppConst.ExtName);
                loader.AddBundle("lua_system_reflection" + AppConst.ExtName);
                loader.AddBundle("lua_unityEngine" + AppConst.ExtName);
                loader.AddBundle("lua_common" + AppConst.ExtName);
                loader.AddBundle("lua_logic" + AppConst.ExtName);
                loader.AddBundle("lua_view" + AppConst.ExtName);
                loader.AddBundle("lua_controller" + AppConst.ExtName);
                loader.AddBundle("lua_misc" + AppConst.ExtName);

                loader.AddBundle("lua_protobuf" + AppConst.ExtName);
                loader.AddBundle("lua_3rd_cjson" + AppConst.ExtName);
                loader.AddBundle("lua_3rd_luabitop" + AppConst.ExtName);
                loader.AddBundle("lua_3rd_pbc" + AppConst.ExtName);
                loader.AddBundle("lua_3rd_pblua" + AppConst.ExtName);
                loader.AddBundle("lua_3rd_sproto" + AppConst.ExtName);
            }
        }

        public object[] DoFile(string filename) {
            return lua.DoFile(filename);
        }

        // Update is called once per frame
        public object[] CallFunction(string funcName, params object[] args) {
            LuaFunction func = lua.GetFunction(funcName);
            if (func != null) {
                return func.Call(args);
            }
            return null;
        }

        public void LuaGC() {
            lua.LuaGC(LuaGCOptions.LUA_GCCOLLECT);
        }

        public void Close() {
            loop.Destroy();
            loop = null;

            lua.Dispose();
            lua = null;
            loader = null;
        }
    }
}