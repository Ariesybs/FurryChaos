# 单房间网络 Demo

当前实现使用 Unity Transport，业务层只依赖 `INetworkTransport`。未来接入 Fantasy 时，实现
`FantasyTransportAdapter` 即可替换传输层。

## 场景配置

1. 在启动场景创建 `GameNetworkManager` 对象并挂载同名组件。
2. Dedicated Server 构建会通过 `UNITY_SERVER` 自动使用 `Server` 模式。
3. 普通客户端将 `Launch Mode` 设置为 `Client`。
4. 本机测试使用地址 `127.0.0.1` 和端口 `7777`。
5. 先启动 Server，再启动一个或多个 Client。

连接成功后，服务端会为每个客户端分配 `EntityId`。目前骨架已经完成连接、加入房间、输入上传、
服务端快照广播以及远端快照插值；角色生成和 `CatCharacter` 的 KCC 权威模拟需要在下一阶段通过
`ServerPlayerJoined`、`ServerInputReceived` 和 `SnapshotReceived` 事件接入。

## 主要入口

- `GameNetworkManager`：生命周期和客户端/服务端入口。
- `SingleRoomServer`：单房间连接与玩家会话映射。
- `UnityTransportAdapter`：当前传输实现。
- `FantasyTransportAdapter`：未来 Fantasy 接入点。
- `LocalNetworkInputSender`：本地输入上传入口。
- `RemoteNetworkEntity`：远端角色快照插值。

