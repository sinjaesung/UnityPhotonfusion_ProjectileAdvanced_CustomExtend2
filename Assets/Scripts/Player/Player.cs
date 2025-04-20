using Fusion;
using UnityEngine;
using static Fusion.NetworkEvents;
using static Unity.Collections.Unicode;

namespace Projectiles
{
    /// <summary>
    /// Component representing joined player. Each player can have a visual representation in the gameplay - player agent.
    /// </summary>
    public class Player : ContextBehaviour
    {
        // PUBLIC MEMBERS

        [Networked]
        public PlayerAgent ActiveAgent { get; private set; }
        public PlayerAgent AgentPrefab => _agentPrefab;

        // PRIVATE MEMBERS

        [SerializeField]
        private PlayerAgent _agentPrefab;

        private PlayerAgent _assignedAgent;
        private int _lastWeaponSlot;

        [Networked]
        // PUBLIC METHODS
        public bool ContextGameplayJoin { get; private set; }

        public void AssignAgent(PlayerAgent agent)
        {
            ActiveAgent = agent;
            ActiveAgent.Owner = this;

            if (HasStateAuthority == true && _lastWeaponSlot != 0)
            {
                agent.Weapons.SwitchWeapon(_lastWeaponSlot, true);
            }
        }

        public void ClearAgent()
        {
            if (ActiveAgent == null)
                return;

            ActiveAgent.Owner = null;
            ActiveAgent = null;
        }

        // NetworkBehaviour INTERFACE

        public override void Spawned()
        {
            Debug.Log("Player Spawned>>"+Context);
            if (Context.Gameplay != null)
            {
                if (!ContextGameplayJoin)
                {
                    Debug.Log("Player Spawned>> gameplay join");
                    Context.Gameplay.Join(this);
                    ContextGameplayJoin = true;
                } 
            }
        }

        public override void FixedUpdateNetwork()
        {
            bool agentValid = ActiveAgent != null && ActiveAgent.Object != null;
            if (agentValid == true && HasStateAuthority == true)
            {
                _lastWeaponSlot = ActiveAgent.Weapons.CurrentWeaponSlot;
            }

            if (Context.Gameplay != null)
            {
                if (!ContextGameplayJoin)
                {
                    Debug.Log("Player FixedUpdateNetwork gameplay join");
                    Context.Gameplay.Join(this);

                    ContextGameplayJoin = true;
                }
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (hasState == false)
                return;

            Debug.Log("Player Despawned>>");

            if (Context.Gameplay != null)
            {
                Context.Gameplay.Leave(this);
            }

            if (HasStateAuthority == true && ActiveAgent != null)
            {
                Debug.Log("Player Despawned Runner.Despawn(ActiveAgent.Object)>>");

                Runner.Despawn(ActiveAgent.Object);
            }

            ActiveAgent = null;
        }
    }
}