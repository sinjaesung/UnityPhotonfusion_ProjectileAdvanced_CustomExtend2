using System;
using Fusion.Sockets;
using System.Collections;
using Projectiles.UI;

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

        public Player roomconnectplayer;

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
            roomconnectplayer = player;
        }
        public void LeaveGame()
        {
            if (Runner != null)
                Runner.Shutdown();
        }
        
        public void RequestSceneChange(string sceneName)
        {
            var context = FindObjectOfType<Scene>().Context;

            /* Debug.Log($"RequestSceneChange Changing scene 할때 ObjectCache랑 NetworkObjectPool 싹다 비우기");

            context.ObjectCache.ClearExecute();

            GetComponent<NetworkObjectPool>().ClearExecute();*/
            FindObjectOfType<UIGameplayView>().SceneLoading.gameObject.SetActive(true);

            Gameplay gameplayObj=FindObjectOfType<Gameplay>();
            int t = 0;
            foreach(var e in gameplayObj.Players)
            {
                Debug.Log(t + $"| RequestSceneChange {e.Key} => {e.Value.transform.name}");
                Player playerlocal = Runner.GetPlayerObject(e.Key).GetComponent<Player>();
                Debug.Log(t+ $"| RequestSceneChange Changing scene 대기동안 모든 컴퓨터 플레이어{playerlocal.transform.name}{playerlocal.ActiveAgent.transform.name} 별 입력제한");
                playerlocal.ActiveAgent.GetComponent<PlayerInput>().InputReset();
                playerlocal.ActiveAgent.IsSceneLoading = true;
                playerlocal.ActiveAgent.KCC.SetGravity(0);
                t++;
            }

            StartCoroutine(MoveSceneExecute(sceneName)); 
        }
        private IEnumerator MoveSceneExecute(string sceneName)
        {
            if (Runner.IsServer)
            {
                yield return new WaitForSeconds(6f);

                Debug.Log("GameManager MoveSceneExecute 코루틴 scene 이동>>" + sceneName);
                Runner.LoadScene(sceneName);
            }
        }

        void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef playerRef)
        {
            Debug.Log("GameManager OnPlayerLeft Runner.IsServer" + Runner.IsServer);

            if (Runner.IsServer == false)
                return;

            Debug.Log($"GameManager OnPlayerLeft playerRef {playerRef}");
            //var player = runner.GetPlayerObject(playerRef);
            Gameplay gameplayObj = FindObjectOfType<Gameplay>();
           /* foreach (var e in gameplayObj.Players) { 
                if(e.Key == playerRef)
                {
                    player = e.Value;
                    break;
                }
            }*/
            if(gameplayObj.Players.TryGet(playerRef,out Player player))
            {
                if (player != null)
                {
                    Debug.Log("GameManager OnPlayerLeft Player Despawn" + player.name);
                    Runner.Despawn(player.GetComponent<NetworkObject>());
                }
                TestRoomPlayer.RemovePlayer(runner, playerRef);
            }
        }

        void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner)
        {
            Debug.Log("GameManager OnSceneLoadDone Runner.Spawn runner.LocalPlayer>>" + runner.LocalPlayer);
            //roomPlayer.RPC_SetCharId(ClientInfo.CharId);
            FindObjectOfType<UIGameplayView>().SceneLoading.gameObject.SetActive(false);

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
      
            /* NetworkObject localPlayer = Runner.GetPlayerObject(Runner.LocalPlayer);
             if (localPlayer)
             {
                 if (localPlayer.GetComponent<Player>() && localPlayer.GetComponent<Player>().ActiveAgent)
                 {
                     Debug.Log($"GameManager OnSceneLoadDone 씬 이동 이후 씬 로드시에 캐릭터 입력조작 복귀 {localPlayer.GetComponent<Player>().ActiveAgent.transform.name}");
                     localPlayer.GetComponent<Player>().ActiveAgent.GetComponent<PlayerInput>().InputRecover();
                 }
             }*/
             
            if (context.ObjectCache && GetComponent<NetworkObjectPool>())
            {
                Debug.Log($"GameManager OnSceneLoadDone , ObjectCache랑 NetworkObjectPool 싹다 비우기");

                context.ObjectCache.ClearExecute();

                GetComponent<NetworkObjectPool>().ClearExecute();              
            }

        /*    if (FindObjectOfType<Gameplay>())
            {
                FindObjectOfType<Gameplay>().SceneLoadedCharacterMoves();
            }*/

            Gameplay gameplayObj = FindObjectOfType<Gameplay>();
            if(gameplayObj && gameplayObj.Players.Capacity > 0)
            {
                int t = 0;
                foreach (var e in gameplayObj.Players)
                {
                    Debug.Log(t + $"| GameManager OnSceneLoadDone {e.Key} => {e.Value.transform.name}");
                    Player playerlocal = Runner.GetPlayerObject(e.Key).GetComponent<Player>();
                    Debug.Log(t + $"| GameManager OnSceneLoadDone 씬 이동 이후 씬 로드시에 모든 접속 캐릭터별 입력조작 복귀 {playerlocal.transform.name}{playerlocal.ActiveAgent.transform.name} 별 입력복귀");
                    playerlocal.ActiveAgent.GetComponent<PlayerInput>().InputRecover();
                    playerlocal.ActiveAgent.IsSceneLoading = false;

                    t++;
                }
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
