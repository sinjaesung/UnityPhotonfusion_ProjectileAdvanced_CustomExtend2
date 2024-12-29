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

        [SerializeField] public float attackDamage;
        public HashSet<IHitTarget> _targets = new();

        public GameObject ImpactPrefab;

        public Animator anim;

        [SerializeField]
        private EInputButton _fireButton = EInputButton.Fire;

        public NetworkButtons Buttons;
       // public NetworkButtons PressedButtons;

        [Networked] private TickTimer AttackTimer { get; set; }
        public bool IsAttackTime => !(AttackTimer.ExpiredOrNotRunning(Runner));

        public Transform motherCharacter;
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
            if (attackDamage <= 0f)
                return;

            // Remove invalid targets
            _targets.RemoveWhere(t => t.IsActive == false);

            ProcessInput();
        }
        private void ProcessInput()
        {
            if (IsProxy == true)
                return;

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
                //PressedButtons = input.Buttons.GetPressed(_agent.Input.PreviousButtons);

                // _agent.Health.StopImmortality();

                if (Buttons.IsSet(_fireButton))
                {
                    if (!IsAttackTime)
                    {
                        Debug.Log("PlayerMelee 근접공격>>");

                        AttackTimer = TickTimer.CreateFromSeconds(Runner, 0.6f);

                        if (anim)
                            anim.SetBool("Shooting", true);
                        Invoke(nameof(ShootingBooleanStatus), 0.6f);
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
            if (anim)
                anim.SetBool("Shooting", false);

            AttackTimer = TickTimer.None;
        }
        public override void Render()
        {
            //_AttackAudioEffects.PlaySound(_AttackSound, EForceBehaviour.ForceAny);
        }
        // MONOBEHAVIOUR

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<IHitTarget>() != null && IsAttackTime)
            {
                var target_obj = other.GetComponentInParent<IHitTarget>() as MonoBehaviour;
                //if (target_obj.CompareTag("Player"))
                //{
                if (target_obj.transform != motherCharacter.transform)
                {
                    Instantiate(ImpactPrefab.transform, other.transform.position, Quaternion.identity);
                }
            }

            if (HasStateAuthority == false)
                return;

            var target = other.GetComponentInParent<IHitTarget>();
            if (target != null && IsAttackTime)
            {
                var target_obj = target as MonoBehaviour;
                //if (target_obj.CompareTag("Player"))
                //{
                if(target_obj.transform != motherCharacter.transform) { 
                    _targets.Add(target);

                    var targetPosition = (target as MonoBehaviour).transform.position;
                    float damage = (attackDamage);
                    Debug.Log("PlayerMelee damageSwing Targets>>" + (target as MonoBehaviour).transform.name + ">damage:" + damage);
                    //AudioManager.PlayAndFollow("Hit", referMother.transform, AudioManager.MixerTarget.SFX);
                    //_AttackAudioEffects.PlaySound(_AttackSound, EForceBehaviour.ForceAny);

                    HitData hitData = new HitData();
                    hitData.Action = EHitAction.Damage;
                    hitData.Amount = damage;
                    hitData.Position = targetPosition;
                    hitData.InstigatorRef = Object.InputAuthority;
                    hitData.Direction = (targetPosition - transform.position).normalized;
                    hitData.Normal = Vector3.up;
                    hitData.Target = target;
                    hitData.HitType = EHitType.Suicide;

                    HitUtility.ProcessHit(ref hitData);

                }
                else
                {
                    Debug.Log("PlayerMelee damageSwing Targets>>" + (target as MonoBehaviour).transform.name + ">자기자신인경우는 제외" );
                }

            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (HasStateAuthority == false)
                return;

            /*  var target = other.GetComponentInParent<IHitTarget>();
              if (target != null)
              {
                  _targets.Remove(target);
              }*/
        }

        // PRIVATE METHODS
    }
}
