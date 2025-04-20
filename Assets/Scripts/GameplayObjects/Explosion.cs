using Fusion;
using Fusion.LagCompensation;
using System.Web;
using UnityEngine;

namespace Projectiles
{
    /// <summary>
    /// Deals damage to all IHitTargets within specified radius right after spawning.
    /// </summary>
    public class Explosion : ContextBehaviour
    {
        // PRIVATE MEMBERS

        [SerializeField]
        private LayerMask _targetMask;
        [SerializeField]
        private LayerMask _blockingMask;
        [SerializeField]
        private EHitType _hitType = EHitType.Explosion;

        [SerializeField, Tooltip("It is usually better to check from point slightly above explosion to filter out terrain discrepancies")]
        private Vector3 _explosionCheckOffset = new(0f, 0.5f, 0f);
        [SerializeField]
        private float _innerRadius = 1f;
        [SerializeField]
        private float _outerRadius = 6f;

        [SerializeField]
        private float _innerDamage = 100f;
        [SerializeField]
        private float _outerDamage = 10f;
        [SerializeField]
        private bool _canDamageOwner = true;

        [SerializeField]
        private float _despawnDelay = 3f;

        [SerializeField]
        private Transform _effectRoot;

        private TickTimer _despawnTimer;

        // NetworkBehaviour INTERFACE

        public override void Spawned()
        {
            ShowEffect();

            if (HasStateAuthority == true)
            {
                Debug.Log("Explosion Spawned HasStateAuthority Only Explode>>");
                Explode();
            }

            _despawnTimer = TickTimer.CreateFromSeconds(Runner, _despawnDelay);
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority == false)
                return;
            Debug.Log("Explosion FixedUpdateNetwork HasStateAuthority Only Execute>>");
            if (_despawnTimer.Expired(Runner) == false)
                return;

            Runner.Despawn(Object);
        }

        // PRIVATE METHODS

        private void Explode()
        {
            var hits = ListPool.Get<LagCompensatedHit>(16);
            var hitRoots = ListPool.Get<int>(16);

            var position = transform.position + _explosionCheckOffset;

            var hitOptions = HitOptions.IncludePhysX;
            if (_canDamageOwner == false)
            {
                hitOptions |= HitOptions.IgnoreInputAuthority;//이 개체의 Owner에게 데미지를 주는지여부
            }
            Debug.Log("Explosion Explode Object.InputAuthority" + Object.InputAuthority.PlayerId);
            int count = Runner.LagCompensation.OverlapSphere(position, _outerRadius, Object.InputAuthority, hits, _targetMask, hitOptions);

            bool damageFalloff = _innerRadius < _outerRadius && _innerDamage != _outerDamage;

            for (int i = 0; i < count; i++)
            {
                var hit = hits[i];

                var hitTarget = HitUtility.GetHitTarget(hit.Hitbox, hit.Collider);

                if (hitTarget == null)
                    continue;

                int hitRootID = hit.Hitbox != null ? hit.Hitbox.Root.GetInstanceID() : 0;
                if (hitRoots.Contains(hitRootID) == true)
                    continue; // Same object was hit multiple times

                var direction = hit.GameObject.transform.position - position;
                float distance = direction.magnitude;
                direction /= distance; // Normalize

                Debug.Log(i + "| Explosion Explode hitTarget" + hitTarget);

                // Check if direction to the hitbox is not obstructed
                if (Runner.GetPhysicsScene().Raycast(position, direction, distance, _blockingMask) == true)
                {
                    continue;
                }

                if (hitRootID != 0)
                {
                    hitRoots.Add(hitRootID);
                }

                float damage = _innerDamage;

                if (damageFalloff == true && distance > _innerRadius)
                {
                    damage = MathUtility.Map(_innerRadius, _outerRadius, _innerDamage, _outerDamage, distance);
                }

                hit.Point = hit.GameObject.transform.position;
                hit.Normal = -direction;

                Debug.Log(i + "| Explosion Explode Object.InputAuthority, direction,damage, _hitType" + Object.InputAuthority.PlayerId + ","+ direction+","+damage+","+ _hitType);
                HitUtility.ProcessHit(Object.InputAuthority, direction, hit, damage, _hitType);
            }

            ListPool.Return(hitRoots);
            ListPool.Return(hits);
        }

        private void ShowEffect()
        {
            if (Runner.Mode == SimulationModes.Server)
                return;

            Debug.Log("Explosion ShowEffect RunnerMode>>" + Runner.Mode);

            if (_effectRoot != null)
            {
                _effectRoot.SetActive(true);
                _effectRoot.localScale = Vector3.one * _outerRadius * 2f;
            }
        }
    }
}