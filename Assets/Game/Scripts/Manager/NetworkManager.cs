using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;

namespace LuaFramework 
{
    public class NetworkManager : Manager
    {
        private LuaState mLuaState = null;
        private LuaTable mLuaTable = null;

        private LuaFunction mLuaOnSocketDataFunc = null;

        private SocketClient socket;
        static Queue<KeyValuePair<int, ByteBuffer>> sEvents = new Queue<KeyValuePair<int, ByteBuffer>>();

        SocketClient SocketClient 
        {
            get 
            { 
                if (socket == null)
                    socket = new SocketClient();
                return socket;                    
            }
        }

        void Awake() 
        {
            Init();
        }

        void Init() 
        {
            SocketClient.OnRegister();
        }

        public void SetLuaTable(LuaTable tb)
        {
            mLuaState = LuaClient.GetMainState();
            if (mLuaState == null) return;

            if (tb == null)
                mLuaTable = mLuaState.GetTable("NetworkManager");
            else
                mLuaTable = tb;

            if (mLuaTable != null)
                mLuaOnSocketDataFunc = mLuaTable.GetLuaFunction("on_socket_data") as LuaFunction;
            else
                Debug.LogWarning("NetworkManager is null:");
        }

        public void OnSocketData(int key, ByteBuffer value)
        {
            if (mLuaOnSocketDataFunc != null)
            {
                mLuaOnSocketDataFunc.BeginPCall();
                mLuaOnSocketDataFunc.Push(key);
                mLuaOnSocketDataFunc.Push(value);
                mLuaOnSocketDataFunc.PCall();
                mLuaOnSocketDataFunc.EndPCall();
            }
        }

        private bool CheckValid()
        {
            if (mLuaState == null) return false;
            if (mLuaTable == null) return false;
            return true;
        }

        public void OnInit()
        {
            if (!CheckValid()) return;
            LuaFunction OnInitFunc = mLuaTable.GetLuaFunction("on_init") as LuaFunction;
            if (OnInitFunc != null)
            {
                OnInitFunc.BeginPCall();
                OnInitFunc.PCall();
                OnInitFunc.EndPCall();

                OnInitFunc.Dispose();
                OnInitFunc = null;
            }
        }

        public void OnUnload()
        {
            if (!CheckValid()) return;
            LuaFunction OnUnLoadFunc = mLuaTable.GetLuaFunction("on_unload") as LuaFunction;
            if (OnUnLoadFunc != null)
            {
                OnUnLoadFunc.BeginPCall();
                OnUnLoadFunc.PCall();
                OnUnLoadFunc.EndPCall();

                OnUnLoadFunc.Dispose();
                OnUnLoadFunc = null;
            }
        }

        public static void AddEvent(int _event, ByteBuffer data) 
        {
            sEvents.Enqueue(new KeyValuePair<int, ByteBuffer>(_event, data));
        }

        /// <summary>
        /// 交给Command，这里不想关心发给谁。
        /// </summary>
        void Update() 
        {
            if (sEvents.Count > 0)
            {
                while (sEvents.Count > 0) 
                {
                    KeyValuePair<int, ByteBuffer> _event = sEvents.Dequeue();
                    facade.SendMessageCommand(NotiConst.DISPATCH_MESSAGE, _event);
                }
            }
        }

        /// <summary>
        /// 发送链接请求
        /// </summary>
        public void SendConnect() 
        {
            SocketClient.SendConnect();
        }

        /// <summary>
        /// 发送SOCKET消息
        /// </summary>
        public void SendMessage(ByteBuffer buffer)
        {
            SocketClient.SendMessage(buffer);
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        void OnDestroy()
        {
            OnUnload();
            SocketClient.OnRemove();
            Debug.Log("~NetworkManager was destroy");

            if (mLuaOnSocketDataFunc != null)
            {
                mLuaOnSocketDataFunc.Dispose();
                mLuaOnSocketDataFunc = null;
            }

            if (mLuaTable != null)
            {
                mLuaTable.Dispose();
                mLuaTable = null;
            }
        }
    }
}