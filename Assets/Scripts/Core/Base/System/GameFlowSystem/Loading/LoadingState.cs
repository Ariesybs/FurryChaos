using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingState : GameFlowState
{
    private UILoadingCtrl m_LoadingUI;
    public LoadingState(GameFlowSystem flow) : base(flow)
    {
        
    }
    
    public override void OnEnter()
    {
        // 生成加载界面
        m_LoadingUI = UIMgr.Open<UILoadingCtrl>();
        m_LoadingUI?.SetProgress(0f);
        Network.LoadCompleted += OnLoadCompleted;
        Network.SceneLoadProgress += OnProgress;
        if (Network.IsServer)
        {
            Network.LoadScene(SceneDefine.GameScene);
        }
    }

    private void OnProgress(string scene, float p)
    {
        m_LoadingUI?.SetProgress(p);
    }

    private void OnLoadCompleted( string sceneName, IReadOnlyList<ulong> clientsCompleted)
    {
        GameRoot.Instance.GameTimer.RegisterOnce(0.5f,DelayEnter);
    }

    private void DelayEnter()
    {
        UIMgr.CloseAll();
        Flow.ChangeState(new InGameState(Flow));
    }
    public override void OnExit()
    {
        Network.LoadCompleted -= OnLoadCompleted;
    }
}