using UnityEngine;
using System.Collections.Generic;
using Fusion;
using Fusion.LagCompensation;
using System.Web;

namespace Projectiles
{
    /// <summary>
    /// An area that deals damage over time to any IHitTarget that stays inside it.
    /// </summary>
    public class Player_Melee : NetworkBehaviour
    {
        // PRIVATE MEMBERS

        [SerializeField] public float _damagePerSecond=20f;
        [SerializeField] private int _hitsPerSecond = 4;

        [Networked]
        private TickTimer _cooldown { get; set; }
        public HashSet<IHitTarget> _targets = new();

        public GameObject ImpactPrefab;

        public Animator anim;

        [SerializeField]
        private EInputButton _fireButton = EInputButton.Fire;

        public NetworkButtons Buttons;
        public NetworkButtons PressedButtons;

        public GameObject TrailRender;
        [Networked] private TickTimer AttackTimer { get; set; }
        public bool IsAttackTime => !(AttackTimer.ExpiredOrNotRunning(Runner));
        public float attackcooldown = 1f;

        public Transform motherCharacter;
        public PlayerAgent motherAgent;
        /* public bool CanFire()
         {
             if(IsBusy == true)
                 return false;

             return _fireOnKeyDownOnly == true ? PressedButtons.IsSet(_fireButton) : Buttons.IsSet(_fireButton);
         }*/
        void Start()
        {
            Debug.Log("PlayerMelee origin Start:");       
        }
        private void OnDisable()
        {
            Debug.Log("PlayerMelee OnDisable:");
            StopAllCoroutines();
            _targets.Clear();
        }
        public override void FixedUpdateNetwork()
        {
            if (_damagePerSecond <= 0f)
                return;

            // Remove invalid targets
            _targets.RemoveWhere(t => t.IsActive == false);

            ProcessInput();
            ShootingBooleanStatus();

            if (_cooldown.ExpiredOrNotRunning(Runner) == true)
            {
                Fire();
            }
        }
        private void ProcessInput()
        {
            /*if (IsProxy == true)
                return;*/

            /* if (CurrentWeapon == null)
                 return;*/

            /* if (GetInput(out GameplayInput input) == false)
                 return;*/

            /* SwitchWeapon(input.WeaponSlot, false);

             if (IsSwitchingWeapon == true)
                 return;

             _weaponContext.Buttons = input.Buttons;
             _weaponContext.PressedButtons = input.Buttons.GetPressed(_agent.Input.PreviousButtons);
             _weaponContext.MoveVelocity = _agent.KCC.RealVelocity;*/

            if (GetInput(out GameplayInput input)){

                Buttons = input.Buttons;
                PressedButtons = input.Buttons.GetPressed(motherAgent.Input.PreviousButtons);

                // _agent.Health.StopImmortality();

                if (/*Buttons.IsSet(_fireButton)*/PressedButtons.IsSet(_fireButton))
                {
                    if (!IsAttackTime)
                    {
                        Debug.Log("PlayerMelee 근접공격>>");
                        TrailRender.SetActive(true);

                        AttackTimer = TickTimer.CreateFromSeconds(Runner, attackcooldown);

                        if (anim)
                            anim.SetBool("Shooting", true);
                        //Invoke(nameof(ShootingBooleanStatus), attackcooldown);
                    }
                    else
                    {
                        Debug.Log("PlayerMelee AttackTime 적용 중에는 공격제한>>");
                    }
                }            
            }         
        }
        private void ShootingBooleanStatus()
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Shooting")==true)
            {
                //원하는 애니메이션이라면 플레이 중인지 체크
                float animTime = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
                if (animTime == 0)
                {
                    Debug.Log("PlayerMelee ShootingBooleanStatus>>플레이중이아님");
                }
                if(animTime >0 && animTime < 1.0f)
                {
                    Debug.Log("PlayerMelee ShootingBooleanStatus>>플레이중임");
                }
                else if(animTime >= 1.0f)
                {
                    Debug.Log("PlayerMelee ShootingBooleanStatus>>애니메이션종료");

                    if (anim)
                        anim.SetBool("Shooting", false);

                    TrailRender.SetActive(false);

                    AttackTimer = TickTimer.None;
                }
            }     
        }
        // MONOBEHAVIOUR

        private void OnTriggerStay(Collider other)
        {
            if (HasStateAuthority == false)
                return;

            var target = other.GetComponentInParent<IHitTarget>();
            if (target != null)
            {
                _targets.Add(target);
            }
            Debug.Log("PlayerMelee targetsCount>>" + _targets.Count);
        }
        private void OnTriggerExit(Collider other)
        {
            if (HasStateAuthority == false)
                return;

              var target = other.GetComponentInParent<IHitTarget>();
              if (target != null)
              {
                  _targets.Remove(target);
              }
        }

        private void Fire()
        {
            //Restart the hit interval
            _cooldown = TickTimer.CreateFromSeconds(Runner, 1f / _hitsPerSecond);

            if (IsAttackTime)
            {
                float damage = _damagePerSecond / _hitsPerSecond;
                foreach (var target in _targets)
                {
                    if((target as MonoBehaviour).transform != motherCharacter){
                        var targetPosition = (target as MonoBehaviour).transform.position;

                        HitData hitData = new HitData();
                        hitData.Action = EHitAction.Damage;
                        hitData.Amount = damage;
                        hitData.Position = targetPosition;
                        hitData.InstigatorRef = Object.InputAuthority;
                        hitData.Direction = (targetPosition - transform.position).normalized;
                        hitData.Normal = Vector3.up;
                        hitData.Target = target;
                        hitData.HitType = EHitType.Explosion;

                        HitUtility.ProcessHit(ref hitData);
                    }   
                }
            }
        }
    }
}
