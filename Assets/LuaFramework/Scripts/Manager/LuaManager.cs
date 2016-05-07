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
        }

        public void InitStart() {
            InitLuaPath();
            InitLuaBundle();
            this.lua.Start();    //启动LUAVM
            LuaCoroutine.Register(lua, this);
            this.StartMain();
            this.StartLooper();
        }

        void StartLooper() {
            loop = gameObject.AddComponent<LuaLooper>();
            loop.luaState = lua;
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
            lua.OpenLibs(LuaDLL.luaopen_sproto_core);
            lua.OpenLibs(LuaDLL.luaopen_protobuf_c);
            lua.OpenLibs(LuaDLL.luaopen_lpeg);
            lua.OpenLibs(LuaDLL.luaopen_cjson);
            lua.OpenLibs(LuaDLL.luaopen_cjson_safe);
            lua.OpenLibs(LuaDLL.luaopen_bit);
            lua.OpenLibs(LuaDLL.luaopen_socket_core);
        }

        /// <summary>
        /// 初始化Lua代码加载路径
        /// </summary>
        void InitLuaPath() {
            if (AppConst.DebugMode) {
                string rootPath = AppConst.FrameworkRoot;
                lua.AddSearchPath(rootPath + "/Lua");
                lua.AddSearchPath(rootPath + "/ToLua/Lua");
            } else {
                lua.AddSearchPath(Util.DataPath + "lua");
            }
        }

        /// <summary>
        /// 初始化LuaBundle
        /// </summary>
        void InitLuaBundle() {
            if (loader.beZip) {
                loader.AddBundle("Lua/Lua" + AppConst.ExtName);
                loader.AddBundle("Lua/Lua_System" + AppConst.ExtName);
                loader.AddBundle("Lua/Lua_System_Reflection" + AppConst.ExtName);
                loader.AddBundle("Lua/Lua_UnityEngine" + AppConst.ExtName);
                loader.AddBundle("Lua/Lua_Common" + AppConst.ExtName);
                loader.AddBundle("Lua/Lua_Logic" + AppConst.ExtName);
                loader.AddBundle("Lua/Lua_View" + AppConst.ExtName);
                loader.AddBundle("Lua/Lua_Controller" + AppConst.ExtName);
                loader.AddBundle("Lua/Lua_Misc" + AppConst.ExtName);

                loader.AddBundle("Lua/Lua_protobuf" + AppConst.ExtName);
                loader.AddBundle("Lua/Lua_3rd_cjson" + AppConst.ExtName);
                loader.AddBundle("Lua/Lua_3rd_luabitop" + AppConst.ExtName);
                loader.AddBundle("Lua/Lua_3rd_pbc" + AppConst.ExtName);
                loader.AddBundle("Lua/Lua_3rd_pblua" + AppConst.ExtName);
                loader.AddBundle("Lua/Lua_3rd_sproto" + AppConst.ExtName);
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