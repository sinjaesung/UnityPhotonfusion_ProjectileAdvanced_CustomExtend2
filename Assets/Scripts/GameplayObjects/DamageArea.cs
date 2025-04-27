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
    public class DamageArea : NetworkBehaviour
    {
        // PRIVATE MEMBERS

        [SerializeField]

        private float _damagePerSecond = 20f;
        [SerializeField]
        private int _hitsPerSecond = 4;

        [Networked]
        private TickTimer _cooldown { get; set; }

        private HashSet<IHitTarget> _targets = new();

        // NetworkBehaviour INTERFACE

        public override void FixedUpdateNetwork()
        {
            if (_damagePerSecond <= 0f)
                return;

            // Remove invalid targets
            _targets.RemoveWhere(t => t.IsActive == false);

            if (_cooldown.ExpiredOrNotRunning(Runner) == true)
            {
                Fire();
            }
        }

        // MONOBEHAVIOUR

        private void OnTriggerEnter(Collider other)
        {
            if (HasStateAuthority == false)
                return;

            Debug.Log("DamageArea ColliderEnter HasStateAuthority Network Only _targets는 오직 HasStateAuthority Computer의 요소에서만 처리>>");
            var target = other.GetComponentInParent<IHitTarget>();
            if (target != null)
            {
                _targets.Add(target);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (HasStateAuthority == false)
                return;

            var target = other.GetComponentInParent<IHitTarget>();
            Debug.Log("DamageArea ColliderExit HasStateAuthority Network Only _targets는 오직 HasStateAuthority Computer의 요소에서만 처리>>"+target);
            if (target != null)
            {
                _targets.Remove(target);
            }
        }

        // PRIVATE METHODS

        private void Fire()
        {
            // Restart the hit interval
            _cooldown = TickTimer.CreateFromSeconds(Runner, 1f / _hitsPerSecond);

            float damage = _damagePerSecond / _hitsPerSecond;
            //Debug.Log("DamageArea Fire damage execute" + _cooldown + "," + damage);
            int c = 0;
            foreach (var target in _targets)
            {
                var targetPosition = (target as MonoBehaviour).transform.position;

                Debug.Log(c+"| DamageArea Fire damage execute" + targetPosition);

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

                c++;
            }
        }
    }
}
