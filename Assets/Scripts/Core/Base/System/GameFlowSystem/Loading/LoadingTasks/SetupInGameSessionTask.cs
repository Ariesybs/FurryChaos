using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;

public class SetupInGameSessionTask : ILoadingTask
{
    public string Name => "SetupInGameSessionTask";
    public async UniTask Run(IProgress<float> progress, CancellationToken ct)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            var lobby = GameRoot.Instance.GameFlow.LobbySystem.LobbySession;
            var players = new List<InGamePlayerData>();
            foreach (var player in lobby.GetLobbyPlayers())
            {
                players.Add(new InGamePlayerData
                {
                    ClientId = player.ClientId,
                    PlayerName = player.PlayerName,
                    CharacterName = player.CharacterName
                });
            }
            var prefab = await GameRoot.Instance.GameLoader.AssetLoader.LoadPrefab("InGameSession");
            var go = UnityEngine.Object.Instantiate(prefab);
            var netObj = go.GetComponent<NetworkObject>();
            netObj.Spawn();
            go.GetComponent<InGameSession>().ServerSetPlayers(players);
            lobby.NetworkObject.Despawn(true);
        }
        await UniTask.WaitUntil(() => GameRoot.Instance.GameFlow.InGameSystem.Session != null, cancellationToken: ct);
        progress?.Report(1f);
    }
}