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
        public void CreateUI(string assetName, GComponent parent, LuaTable luaTable, LuaFunction func = null) 
        {
            string abName = assetName.ToLower() + AppConst.ExtName;

            string tmpAssetName = assetName;
            if (AppConst.DebugMode)
            {
                UIPackage.AddPackage("UI/" + assetName);
                _CreateUI(assetName, parent, luaTable, func);
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
                    _CreateUI(assetName, parent, luaTable, func);
                });
            }
        }

        private void _CreateUI(string assetName, GComponent parent, LuaTable luaTable, LuaFunction func)
        {
            GComponent _ui = UIPackage.CreateObject(assetName, "Main").asCom;
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

            if (func != null) func.Call(_ui);
            Debug.LogWarning("CreatePanel----------------::>> " + assetName);
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