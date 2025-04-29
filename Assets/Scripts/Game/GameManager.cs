using System;
using Fusion.Sockets;

namespace Projectiles
{
    using System.Collections.Generic;
    using UnityEngine;
    using Fusion;
    using UnityEngine.InputSystem;

    /// <summary>
    /// Handles player connections (spawning of Player instances) and prepares SceneContext when a gameplay scene load is done.
    /// </summary>
    [RequireComponent(typeof(NetworkRunner))]
    [RequireComponent(typeof(NetworkEvents))]
    [DefaultExecutionOrder(-100)]

    public sealed class GameManager : SimulationBehaviour, INetworkRunnerCallbacks
    {
        // PRIVATE MEMBERS

        [SerializeField]
        private Gameplay _gameplayPrefab;
        [SerializeField]
        private Player _playerPrefab;

        private bool _gameplaySpawned;

        [SerializeField]
        private Player[] _playerPrefabs;
        [SerializeField]
        private TestRoomPlayer roomplayerprefab;

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }
        // INetworkRunnerCallbacks INTERFACE
        void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef playerRef)
        {
            Debug.Log("GameManager OnPlayerJoined Runner.IsServer" + Runner.IsServer);

            if (Runner.IsServer == false)
                return;

            Runner.Spawn(roomplayerprefab,inputAuthority:playerRef);
            if (_gameplaySpawned == false)
            {
                Runner.Spawn(_gameplayPrefab);
                _gameplaySpawned = true;
            }        
        }

        public void SpawnPlayer(PlayerRef playerref,int charIndex)
        {
           // var charIndex = (playerRef.AsIndex - 1) % 11;
            Debug.Log("GameManager SpawnPlayer "+ playerref + ">>charIndex:"+ charIndex);//0,1,2,3,4,5,6,7,8,9%10 => 0,1,2,3,4,5,6,7,8,9,0,....

            var player = Runner.Spawn(_playerPrefabs[charIndex], inputAuthority: playerref);
            Runner.SetPlayerObject(playerref, player.Object);
        }
        public void LeaveGame()
        {
            if (Runner != null)
                Runner.Shutdown();
        }

        public void RequestSceneChange(string sceneName)
        {
            var context = FindObjectOfType<Scene>().Context;
            if (context.ObjectCache && GetComponent<NetworkObjectPool>())
            {
                Debug.Log($"RequestSceneChange Changing scene ÇÒ¶§ ObjectCache¶û NetworkObjectPool ½Ï´Ù ºñ¿ì±â");

                context.ObjectCache.ClearExecute();

                GetComponent<NetworkObjectPool>().ClearExecute();
            }

            if (Runner.IsServer)
            {
                Debug.Log($"RequestSceneChange IsServer Changing scene to {sceneName}");
           
                Runner.LoadScene(sceneName);
            }
        }
        void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef playerRef)
        {
            Debug.Log("GameManager OnPlayerLeft Runner.IsServer" + Runner.IsServer);

            if (Runner.IsServer == false)
                return;

            var player = Runner.GetPlayerObject(playerRef);
            Debug.Log($"GameManager OnPlayerLeft {playerRef}");
            if (player != null)
            {
                Debug.Log("GameManager OnPlayerLeft Player Despawn");
               Runner.Despawn(player);
            }
           TestRoomPlayer.RemovePlayer(runner, playerRef);
        }

        void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner)
        {
            Debug.Log("GameManager OnSceneLoadDone Runner.Spawn runner.LocalPlayer>>" + runner.LocalPlayer);
            //roomPlayer.RPC_SetCharId(ClientInfo.CharId);

            // Prepare context
            //var scene = runner.SimulationUnityScene.GetComponent<Scene>(true);
            var scene = FindObjectOfType<Scene>();
            Debug.Log("GameManager scene>>" + scene.transform.name);

            var context = scene.Context;
            context.Runner = Runner;

            // Assign context
            var contextBehaviours = runner.SimulationUnityScene.GetComponents<IContextBehaviour>(true);
            int b = 0;
            foreach (var behaviour in contextBehaviours)
            {
                Debug.Log(b + $"|GameManager OnSceneLoadDone contextBehaviours {behaviour}.Context={context}");
                behaviour.Context = context;
            }

            var objectPool = Runner.GetComponent<NetworkObjectPool>();
            objectPool.Context = context;
            Debug.Log("GameManager OnSceneLoadDone");

            if (runner.Config.PeerMode == NetworkProjectConfig.PeerModes.Multiple)
            {
                Debug.Log("GameManager OnSceneLoadDone PeerMode==PeerModes.Multiple");
                // In case of multipeer mode, fix the scene lighting
                var renderSettingsUpdated = scene.GetComponent<RenderSettingsUpdater>();
                renderSettingsUpdated.ApplySettings();
            }

            if (FindObjectOfType<Gameplay>())
            {
                FindObjectOfType<Gameplay>().SceneLoadedCharacterMoves();
            }
        }
        void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input) { }
        void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {
            Debug.Log($"OnShutdown {shutdownReason}");
        }
        void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner) { }
        void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        void INetworkRunnerCallbacks.OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner) { }
    }
}
