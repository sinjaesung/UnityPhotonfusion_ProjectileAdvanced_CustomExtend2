using System.Collections.Generic;
using Fusion;
using UnityEngine;
using static Fusion.NetworkEvents;
using static Unity.Collections.Unicode;

namespace Projectiles
{
    /// <summary>
    /// Represents the actual gameplay loop. Handles PlayerAgent spawning and despawning for each Player that joins gameplay.
    /// </summary>
    public class Gameplay : ContextBehaviour
    {
        // PUBLIC MEMBERS

        [Networked, Capacity(200)]
        public NetworkDictionary<PlayerRef, Player> Players { get; }

        // PRIVATE METHODS

        private SpawnPoint[] _spawnPoints;
        private int _lastSpawnPoint = -1;

        private List<SpawnRequest> _spawnRequests = new();

        // PUBLIC METHODS

        //public WorldDefinition definition;

        private void Awake()
        {
           // Debug.Log("GamePlayWorld Awake>> " + definition.worldName);
            RoomGameManager.SetWorld(this);
        }
        private void OnDestroy()
        {
            RoomGameManager.SetWorld(null);
        }

        public void Join(Player player)
        {
            if (HasStateAuthority == false)
                return;

            var playerRef = player.Object.InputAuthority;

            Debug.Log("GamePlay Join Only HasStateAuthority>>");

            if (Players.ContainsKey(playerRef) == true)
            {
                Debug.LogError($"Player {playerRef} already joined");
                return;
            }

            Players.Add(playerRef, player);

            OnPlayerJoined(player);
        }
        public void SpawnPlayer(NetworkRunner runner, RoomPlayer player)
        {
            var index = RoomPlayer.Players.IndexOf(player);
            var charId = player.CharId;

            Debug.Log($"World(GamePlay) SpawnPlayer>>{player.Object.InputAuthority} => charId:" + charId);
            var prefab = ResourceManager.Instance.characterDefinitions[charId].prefab;

            // Spawn player
            var entity = runner.Spawn(
                prefab,
                Vector3.zero,
                Quaternion.identity,
                player.Object.InputAuthority
            );

            entity.RoomUser = player;
            player.GameState = RoomPlayer.EGameState.GameReady;
            player.player = entity;

            entity.SetCharacterIndex(charId);
            Debug.Log($"Spawning Character for [{player.Username}] as {entity.name}");
            entity.transform.name = $"Character ([{player.Username}]) {charId}";
            Debug.Log("GamePlay(WorldMap) SpawnPlayer roomplayer InputAuthority(PlayerRef)" + player.Object.InputAuthority);
            Runner.SetPlayerObject(player.Object.InputAuthority, entity.Object);
        }

        public void Leave(Player player)
        {
            if (HasStateAuthority == false)
                return;

            Debug.Log("GamePlay Leave Only HasStateAuthority>>");

            if (Players.ContainsKey(player.Object.InputAuthority) == false)
                return;

            Players.Remove(player.Object.InputAuthority);

            OnPlayerLeft(player);
        }

        // NetworkBehaviour INTERFACE

       public override void Spawned()
       {
            // Register to context
            Context.Gameplay = this;

            // Prepare context
            var scene = Runner.SimulationUnityScene.GetComponent<Scene>(true);

            var context = scene.Context;
            context.Runner = Runner;

            // Assign context
            var contextBehaviours = Runner.SimulationUnityScene.GetComponents<IContextBehaviour>(true);
            foreach (var behaviour in contextBehaviours)
            {
                behaviour.Context = context;
            }

            var objectPool = Runner.GetComponent<NetworkObjectPool>();
            objectPool.Context = context;
            Debug.Log("GamePlay(World Spawned) OnSceneLoadDone");

            if (Runner.Config.PeerMode == NetworkProjectConfig.PeerModes.Multiple)
            {
                Debug.Log("GamePlay OnSceneLoadDone Spawned PeerMode==PeerModes.Multiple");
                // In case of multipeer mode, fix the scene lighting
                var renderSettingsUpdated = scene.GetComponent<RenderSettingsUpdater>();
                renderSettingsUpdated.ApplySettings();
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority == false)
                return;

            int currentTick = Runner.Tick;

            for (int i = _spawnRequests.Count - 1; i >= 0; i--)
            {
                var request = _spawnRequests[i];

                Debug.Log(i + "| GamePlay FixedUpdateNetwork spawnRequest" + request.Tick + "/" + currentTick);
                if (request.Tick > currentTick)
                    continue;
                Debug.Log(i + "| GamePlay FixedUpdateNetwork spawnRequest" + request.Tick + "<=" + currentTick);
                _spawnRequests.RemoveAt(i);

                if (request.Player == null || request.Player.Object == null)
                    continue; // Player no longer valid

                if (Players.ContainsKey(request.Player.Object.InputAuthority) == false)
                    continue; // Player left gameplay

                Debug.Log(i + "| GamePlay FixedUpdateNetwork SpawnPlayerAgent" + request.Player.transform.name);
                SpawnPlayerAgent(request.Player);
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            // Clear from context
            Context.Gameplay = null;
        }

        // PROTECTED METHODS

        protected virtual void OnPlayerJoined(Player player)
        {
            Debug.Log("GamePlay OnPlayerJoined>>" + player.transform.name);
            SpawnPlayerAgent(player);
        }

        protected virtual void OnPlayerLeft(Player player)
        {
            Debug.Log("GamePlay OnPlayerLeft>>" + player.transform.name);
            DespawnPlayerAgent(player);
        }

        protected virtual void OnPlayerDeath(Player player)
        {
            Debug.Log("GamePlay OnPlayerDeath AddSpawnRequest>>" + player.transform.name);
            AddSpawnRequest(player, 3f);
        }

        protected virtual void OnPlayerAgentSpawned(PlayerAgent agent)
        {
            agent.Health.SetImmortality(3f);
        }

        protected virtual void OnPlayerAgentDespawned(PlayerAgent agent)
        {
        }

        protected void SpawnPlayerAgent(Player player)
        {
            DespawnPlayerAgent(player);

            var agent = SpawnAgent(player.Object.InputAuthority, player.AgentPrefab) as PlayerAgent;
            player.AssignAgent(agent);

            agent.Health.FatalHitTaken += OnFatalHitTaken;

            OnPlayerAgentSpawned(agent);
        }

        protected void DespawnPlayerAgent(Player player)
        {
            if (player.ActiveAgent == null)
                return;

            player.ActiveAgent.Health.FatalHitTaken -= OnFatalHitTaken;

            OnPlayerAgentDespawned(player.ActiveAgent);

            DespawnAgent(player.ActiveAgent);
            player.ClearAgent();
        }

        protected void AddSpawnRequest(Player player, float spawnDelay)
        {
            int delayTicks = Mathf.RoundToInt(Runner.TickRate * spawnDelay);

            Debug.Log("GamePlay AddSpawnRequest" + player.transform.name + ">delayTicks" + delayTicks);
            _spawnRequests.Add(new SpawnRequest()
            {
                Player = player,
                Tick = Runner.Tick + delayTicks,
            });
        }

        // PRIVATE METHODS

        private void OnFatalHitTaken(HitData hitData)
        {
            var health = hitData.Target as Health;

            if (health == null)
                return;

            if (Players.TryGet(health.Object.InputAuthority, out Player player) == true)
            {
                OnPlayerDeath(player);
            }
        }

        private PlayerAgent SpawnAgent(PlayerRef inputAuthority, PlayerAgent agentPrefab)
        {
            if (_spawnPoints == null)
            {
                _spawnPoints = Runner.SimulationUnityScene.FindObjectsOfTypeInOrder<SpawnPoint>(false);
            }

            _lastSpawnPoint = (_lastSpawnPoint + 1) % _spawnPoints.Length;
            var spawnPoint = _spawnPoints[_lastSpawnPoint].transform;

            var agent = Runner.Spawn(agentPrefab, spawnPoint.position, spawnPoint.rotation, inputAuthority);
            return agent;
        }

        private void DespawnAgent(PlayerAgent agent)
        {
            if (agent == null)
                return;

            Runner.Despawn(agent.Object);
        }

        // HELPERS

        public struct SpawnRequest
        {
            public Player Player;
            public int Tick;
        }
    }
}


