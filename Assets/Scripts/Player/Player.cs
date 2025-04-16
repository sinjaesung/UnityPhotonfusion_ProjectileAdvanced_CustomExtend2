using Fusion;
using UnityEngine;
using static Fusion.NetworkEvents;
using static Unity.Collections.Unicode;
using System.Collections;

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
        [Networked] public RoomPlayer RoomUser { get; set; }

        [Networked]
        public int CharacterIndex { get; set; } = -1;
        // PUBLIC METHODS

        private bool Isgameplayjoin = false;

        public void AssignAgent(PlayerAgent agent)
        {
            ActiveAgent = agent;
            ActiveAgent.Owner = this;

            Debug.Log("Player AssignAgent>>" + agent.transform.name);
            if (HasStateAuthority == true && _lastWeaponSlot != 0)
            {
                agent.Weapons.SwitchWeapon(_lastWeaponSlot, true);
                Debug.Log("Player AssignAgent HasStateAuthority SwitchWeapon>>");
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
            if (Context && Context.Gameplay != null)
            {
                Context.Gameplay.Join(this);
            }
            Debug.Log("Player Spawned>> gameplay join");
            StartCoroutine(ContextGamePlayJoin());
        }
        private IEnumerator ContextGamePlayJoin()
        {
            while (true)
            {
                //Debug.Log("ContextGamePlayJoin whiles" + (Context ? Context.transform.name:""));
                if (Context && Context.Gameplay!=null)
                {
                    break;
                }
                Debug.Log("Player ContextGamePlayJoin pending>>");
                yield return new WaitForSeconds(0.1f);
            }

            if (Context && Context.Gameplay != null)
            {
                Debug.Log("Player ContextGamePlayJoin>>");
                Context.Gameplay.Join(this);
            }
        }
        public void SetCharacterIndex(int _index)
        {
            CharacterIndex = _index;
        }
        public override void FixedUpdateNetwork()
        {
            bool agentValid = ActiveAgent != null && ActiveAgent.Object != null;
            if (agentValid == true && HasStateAuthority == true)
            {
                _lastWeaponSlot = ActiveAgent.Weapons.CurrentWeaponSlot;
                Debug.Log("Player agentValid == true  _lastWeaponSlot:>>" + _lastWeaponSlot);
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            Debug.Log("Player Despawned>>");

            if (hasState == false)
                return;

            if (Context.Gameplay != null)
            {
                Context.Gameplay.Leave(this);
            }

            if (HasStateAuthority == true && ActiveAgent != null)
            {
                Debug.Log("Player Despawned HasStateAuthority Runner.Despawn(ActiveAgent.Object)>>");

                Runner.Despawn(ActiveAgent.Object);
            }

            ActiveAgent = null;
        }
    }
}