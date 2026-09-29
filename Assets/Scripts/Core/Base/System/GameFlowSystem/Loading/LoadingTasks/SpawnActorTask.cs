using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class SpawnActorTask : ILoadingTask
{
    public string Name => "SpawnActorTask";
    public async UniTask Run(IProgress<float> progress, CancellationToken ct)
    {
        var session = GameRoot.Instance.GameFlow.InGameSystem.Session;
        if (NetworkManager.Singleton.IsServer)
        {
            var points = UnityEngine.Object.FindAnyObjectByType<ActorSpawnMgr>()?.GetPlayerSpawnPositions();
            var players = session.Players;
            for (int i = 0; i < players.Count; i++)
            {
                var prefab = await GameRoot.Instance.GameLoader.AssetLoader.LoadPrefab("Cat");
                var go = UnityEngine.Object.Instantiate(prefab, points[i].position, points[i].rotation);
                go.GetComponent<NetworkObject>().SpawnAsPlayerObject(players[i].ClientId);
                progress?.Report((i + 1f) / players.Count);
            }
            session.ServerMarkSpawned();
        }
        await UniTask.WaitUntil(() => session.PlayersSpawned.Value, cancellationToken: ct);
        progress?.Report(1f);
        Log.Info(Name,"玩家生成完毕");
    }
}