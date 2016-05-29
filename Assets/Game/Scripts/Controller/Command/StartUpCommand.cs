using UnityEngine;
using System.Collections;
using LuaFramework;

public class StartUpCommand : ControllerCommand 
{
    public override void Execute(IMessage message) 
    {
        if (!Util.CheckEnvironment()) return;

        GameObject gameMgr = GameObject.Find("GameManager");
        if (gameMgr != null)
            gameMgr.AddComponent<AppView>();

        //-----------------关联命令-----------------------
        AppFacade.Instance.RegisterCommand(NotiConst.DISPATCH_MESSAGE, typeof(SocketCommand));

        //-----------------初始化管理器-----------------------
        AppFacade.Instance.AddManager<LuaManager>();
        AppFacade.Instance.AddManager<GameResFactory>();
        AppFacade.Instance.AddManager<SoundManager>();
        AppFacade.Instance.AddManager<TimerManager>();
        AppFacade.Instance.AddManager<NetworkManager>();
        AppFacade.Instance.AddManager<ResourceManager>();
        AppFacade.Instance.AddManager<ThreadManager>();
        AppFacade.Instance.AddManager<GameManager>();
    }
}