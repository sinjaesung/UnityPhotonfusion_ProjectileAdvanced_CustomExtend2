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
    public class IEnemyMeleeCollider_Network : NetworkBehaviour
    {
        // PRIVATE MEMBERS

        [SerializeField]

        public float _damagePerSecond = 20f;
        [SerializeField]
        public int _hitsPerSecond = 4;
        [SerializeField]
        private float damageflag = 1;
        public bool damageSwing = false;
        [Networked]
        private TickTimer _cooldown { get; set; }

        public HashSet<IHitTarget> _targets = new();

        // NetworkBehaviour INTERFACE
        [SerializeField] public IEnemyFSM_Network referMother;

        [Header("Sound")]
        [SerializeField]
        private Transform AttackAudioEffectsRoot;
        [SerializeField]
        private AudioSetup _AttackSound;
        private AudioEffect[] _AttackAudioEffects;

        void Start()
        {
            Debug.Log("IEnemyMeleeCollider_Network origin Start:");
            //characterstats = playerController.characterStats;
            // StartCoroutine(CalcFineStats());
            // 
            if (AttackAudioEffectsRoot != null)
            {
                _AttackAudioEffects = AttackAudioEffectsRoot.GetComponentsInChildren<AudioEffect>(true);
            }
        }
        private void OnDisable()
        {
            Debug.Log("IEnemyMeleeCollider_Network OnDisable:");
            StopAllCoroutines();
            _targets.Clear();
        }
        public override void FixedUpdateNetwork()
        {
            if (_damagePerSecond <= 0f)
                return;

            // Remove invalid targets
            _targets.RemoveWhere(t => t.IsActive == false);

           /* if (_cooldown.ExpiredOrNotRunning(Runner) == true)
            {
                Fire();
            }*/
        }
        public override void Render()
        {
            //_AttackAudioEffects.PlaySound(_AttackSound, EForceBehaviour.ForceAny);
        }
        // MONOBEHAVIOUR

        private void OnTriggerEnter(Collider other)
        {
            if (HasStateAuthority == false)
                return;

            var target = other.GetComponentInParent<IHitTarget>();
            if (target != null)
            {
                var target_obj = target as MonoBehaviour;
                if (target_obj.CompareTag("Player"))
                {
                    _targets.Add(target);

                    if (damageSwing)
                    {
                        var targetPosition = (target as MonoBehaviour).transform.position;
                        float damage = (_damagePerSecond * damageflag);
                        Debug.Log("IEnemyMeleeCollider_Network damageSwing Targets>>" + (target as MonoBehaviour).transform.name + ">damage:" + damage);
                        //AudioManager.PlayAndFollow("Hit", referMother.transform, AudioManager.MixerTarget.SFX);
                        _AttackAudioEffects.PlaySound(_AttackSound, EForceBehaviour.ForceAny);

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

        private void Fire()
        {
            // Restart the hit interval
            _cooldown = TickTimer.CreateFromSeconds(Runner, 1f / _hitsPerSecond);

            float damage = (_damagePerSecond* damageflag) / _hitsPerSecond;
            foreach (var target in _targets)
            {
                Debug.Log("IEnemyMeleeCollider_Network body Targets>>" + (target as MonoBehaviour).transform.name+">damage:"+damage);
                var targetPosition = (target as MonoBehaviour).transform.position;
                var target_obj = target as MonoBehaviour;

                if (target_obj.CompareTag("Player"))
                {
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
            }
        }
    }
}
