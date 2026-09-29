using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LoadingState : GameFlowState
{
    private UILoadingCtrl m_LoadingUI;
    private readonly LoadingBus m_LoadingBus;
    public LoadingState(GameFlowSystem flow) : base(flow)
    {
        m_LoadingBus = new LoadingBus();
    }
    
    public override void OnEnter()
    {
        // 生成加载界面
        m_LoadingUI = UIMgr.Open<UILoadingCtrl>();
        m_LoadingUI?.SetProgress(0f);
        
        // 启动loading
        StartLoadingAsync().Forget();
    }

    private async UniTaskVoid StartLoadingAsync()
    {
        var cts = new CancellationTokenSource();
        // 构建loading任务
        m_LoadingBus.Add(new SetupInGameSessionTask());
        m_LoadingBus.Add(new NetworkSceneLoadTask(SceneDefine.GameScene));
        m_LoadingBus.Add(new SpawnActorTask());
        
        // 启动loading流程
        await m_LoadingBus.Run((p) =>
        {
            m_LoadingUI?.SetProgress(p);
        },cts.Token);
        Flow.ChangeState(new InGameState(Flow));
    }
}