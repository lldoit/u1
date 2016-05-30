using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FairyGUI;
using LuaInterface;

namespace LuaFramework 
{
    public class GameResFactory : Manager 
    {

#if ASYNC_MODE
        /// <summary>
        /// 创建面板，请求资源管理器
        /// </summary>
        /// <param name="type"></param>
        public void CreateUI(string assetName, string res_name, GComponent parent, LuaTable luaTable, LuaFunction func = null) 
        {
            string abName = assetName.ToLower() + AppConst.ExtName;

            string tmpAssetName = assetName;
            if (AppConst.DebugMode)
            {
                UIPackage.AddPackage("UI/" + assetName);
                _CreateUI(assetName, res_name, parent, luaTable, func);
            }
            else
            {
                string[] assetnames = { };
                ResManager.LoadPrefab(abName, assetnames, delegate (UnityEngine.Object[] objs)
                {
                    if (objs.Length == 0) return;
                    
                    AssetBundle ab = objs[0] as AssetBundle;
                    if (ab == null) return;

                    UIPackage.AddPackage(ab);
                    _CreateUI(assetName, res_name, parent, luaTable, func);
                });
            }
        }

        public void CreatePopUp(string assetName, string resName, LuaTable luaTable, LuaFunction func = null)
        {
            string abName = assetName.ToLower() + AppConst.ExtName;

            string tmpAssetName = assetName;
            if (AppConst.DebugMode)
            {
                UIPackage.AddPackage("UI/" + assetName);
                _CreatePopUp(assetName, resName, luaTable, func);
            }
            else
            {
                string[] assetnames = { };
                ResManager.LoadPrefab(abName, assetnames, delegate (UnityEngine.Object[] objs)
                {
                    if (objs.Length == 0) return;

                    AssetBundle ab = objs[0] as AssetBundle;
                    if (ab == null) return;

                    UIPackage.AddPackage(ab);
                    _CreatePopUp(assetName, resName, luaTable, func);
                });
            }
        }

        void _CreateUI(string assetName, string res_name, GComponent parent, LuaTable luaTable, LuaFunction func)
        {
            GComponent _ui = UIPackage.CreateObject(assetName, res_name).asCom;
            _ui.fairyBatching = true;
            _ui.displayObject.gameObject.name = assetName;
            _ui.SetSize(GRoot.inst.width, GRoot.inst.height);
            _ui.AddRelation(GRoot.inst, RelationType.Size);
            parent.AddChild(_ui);

            LuaBehaviour tmpBehaviour = _ui.displayObject.gameObject.AddComponent<LuaBehaviour>();
            tmpBehaviour.Init(luaTable, _ui);

            if (func != null)
            {
                func.BeginPCall();
                func.Push(_ui);
                func.PCall();
                func.EndPCall();
            }
            
            Debug.LogWarning("CreateUI----------------::>> " + assetName);
        }

        void _CreatePopUp(string assetName, string resName, LuaTable luaTable, LuaFunction func)
        {
            Window win = new Window();
            win.contentPane = UIPackage.CreateObject(assetName, resName).asCom;
            win.Center();
            win.modal = true;
            
            LuaBehaviour tmpBehaviour = win.displayObject.gameObject.AddComponent<LuaBehaviour>();
            tmpBehaviour.Init(luaTable, win);
            
            if (func != null)
            {
                func.BeginPCall();
                func.Push(win);
                func.PCall();
                func.EndPCall();
            }

            Debug.LogWarning("CreatePopUp----------------::>> " + assetName);
        }
#else
        /// <summary>
        /// 创建面板，请求资源管理器
        /// </summary>
        /// <param name="type"></param>
        public void CreateUI(string name, LuaFunction func = null) {
            string assetName = name + "Panel";
            GameObject prefab = ResManager.LoadAsset<GameObject>(name, assetName);
            if (Parent.FindChild(name) != null || prefab == null) {
                return;
            }
            GameObject go = Instantiate(prefab) as GameObject;
            go.name = assetName;
            go.layer = LayerMask.NameToLayer("Default");
            go.transform.SetParent(Parent);
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = Vector3.zero;
            go.AddComponent<LuaBehaviour>();

            if (func != null) func.Call(go);
            Debug.LogWarning("CreatePanel::>> " + name + " " + prefab);
        }
#endif
    }
}