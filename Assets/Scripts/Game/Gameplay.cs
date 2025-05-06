using System.Collections.Generic;
using Fusion;
using UnityEngine;
using static Fusion.NetworkEvents;
using static Unity.Collections.Unicode;
using TMPro;
using System.Collections;

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
        public bool IsNickNameAssigned { get; set; }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Join(Player player)
        {
            if (HasStateAuthority == false)
                return;

            var playerRef = player.Object.InputAuthority;

            Debug.Log("GamePlay Join Only HasStateAuthority>>"+ playerRef);

            if (Players.ContainsKey(playerRef) == true)
            {
                Debug.LogError($"Player {playerRef} already joined");
                return;
            }

            Players.Add(playerRef, player);

            OnPlayerJoined(player);
        }

        public void Leave(Player player)
        {
            if (HasStateAuthority == false)
                return;

            Debug.Log("GamePlay Leave Only HasStateAuthority>>"+ player.Object.InputAuthority);

            if (Players.ContainsKey(player.Object.InputAuthority) == false)
                return;

            Players.Remove(player.Object.InputAuthority);

            OnPlayerLeft(player);
        }

        // NetworkBehaviour INTERFACE

        public override void Spawned()
        {
            // Register to context
            Debug.Log("GamePlay Spawned>>"+ Context);
            Context.Gameplay = this;
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
        public override void Render()
        {
            int e = 0;
            foreach(var playeritem in Players)
            {
                Player player_instance = playeritem.Value;
                //Debug.Log(e + $"| GamePlay Render {playeritem.Key}:{player_instance.name}");

                if(player_instance && player_instance.ActiveAgent && player_instance.ActiveAgent.GetComponentInChildren<Nickname>()) 
                {
                    player_instance.ActiveAgent.GetComponentInChildren<Nickname>().GetComponent<TextMeshPro>().text = player_instance.nickname;
                    //Debug.Log(e+$"| >>GamePlay Render set nickname object:{player_instance.ActiveAgent.GetComponentInChildren<Nickname>().transform.name}=>{player_instance.nickname}>>");
                    e++;
                }
            }
            IsNickNameAssigned = true;
        }
        public void SceneLoadedCharacterMoves()
        {
            int e = 0;
            int tempSpawnPoint = -1;

            _spawnPoints = Runner.SimulationUnityScene.FindObjectsOfTypeInOrder<SpawnPoint>(false);

            foreach (var player in Players)
            {
                Debug.Log(e + $"|Gameplay SceneLoadedCharacterMoves  접속 플레이어:{player.Key},{player.Value.transform.name}");

                tempSpawnPoint = (tempSpawnPoint + 1) % _spawnPoints.Length;
                var spawnPoint = _spawnPoints[tempSpawnPoint].transform;

                Debug.Log(e + $"|Gameplay SceneLoadedCharacterMoves  접속 플레이어 agent:{player.Value.ActiveAgent.transform.name},set position:{spawnPoint.position}");

                player.Value.ActiveAgent.transform.position = spawnPoint.position;
                //player.Value.ActiveAgent.KCC.SetPosition(spawnPoint.position);

                e++;
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
            Debug.Log("spawnagentname>>" + agent.transform.name);
            player.AssignAgent(agent);
            //PlayerNicknames.Add(player.Object.InputAuthority, agent.GetComponentInChildren<Nickname>());

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

        public void SpawnPointsAssign()
        {      
            _spawnPoints = Runner.SimulationUnityScene.FindObjectsOfTypeInOrder<SpawnPoint>(false);
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
            SpawnPointsAssign();

            _lastSpawnPoint = (_lastSpawnPoint + 1) % _spawnPoints.Length;
            var spawnPoint = _spawnPoints[_lastSpawnPoint].transform;

            var agent = Runner.Spawn(agentPrefab, spawnPoint.position, spawnPoint.rotation, inputAuthority);
            return agent;
        }

        private void DespawnAgent(PlayerAgent agent)
        {
            if (agent == null)
                return;

            Debug.Log("GamePlay DespawnAgent>>" + agent.name);
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


