using System.Collections.Generic;
using UnityEngine;

namespace Projectiles.UI
{
    public class UIHitNumbers : UIBehaviour
    {
        // PRIVATE MEMBERS

        [SerializeField]
        private UIHitNumber _hitItem;

        private List<UIHitNumber> _activeItems = new(32);
        private List<UIHitNumber> _inactiveItems = new(32);

        private List<HitData> _pendingHits = new(32);
        private Camera _camera;

        // PUBLIC METHODS

        public void HitPerformed(HitData hitData)
        {
            for (int i = 0; i < _pendingHits.Count; i++)
            {
                var pending = _pendingHits[i];

                Debug.Log(i+"| UIHitNumbers HitPerformed pendingHits update>>");
                // Try to merge hit data
                if (pending.Target == hitData.Target && pending.Target != null)
                {
                    pending.Amount += hitData.Amount;
                    pending.IsFatal |= hitData.IsFatal;

                    Debug.Log(i + "| UIHitNumbers HitPerformed pendingHits update hitdamage>>"+pending.Amount);
                    _pendingHits[i] = pending;
                    return;
                }
            }

            _pendingHits.Add(hitData);
        }

        // MONOBEHAVIOUR
        protected void Awake()
        {
            _hitItem.SetActive(false);
            _camera = Camera.main;
        }

        protected void OnDisable()
        {
            _pendingHits.Clear();
        }

        // MONOBEHAVIOUR

        protected void Update()
        {
            for (int i = 0; i < _pendingHits.Count; i++)
            {
                Debug.Log(i + "| UIHitNumbers _pendingHits Process");
                ProcessHit(_pendingHits[i]);
            }

            _pendingHits.Clear();
        }

        protected void LateUpdate()
        {
            UpdateActiveItems(_activeItems, _inactiveItems);
        }

        // PRIVATE METHODS
        private void ProcessHit(HitData hitData)
        {
            var hitItem = _inactiveItems.PopLast();
            if (hitItem == null)
            {
                hitItem = Instantiate(_hitItem, _hitItem.transform.parent);
            }

            _activeItems.Add(hitItem);

            var hitPosition = hitData.Position;
            if (hitData.Target != null)
            {
                hitPosition = hitData.Target.HeadPivot.position;
            }
            Debug.Log("UIHitNumbers ProcessHit hitPosition hitItem added" + hitPosition);
            Debug.Log("UIHitNumbers ProcessHit hitPosition hitItem damage" + hitData.Amount);
            hitItem.SetNumber(hitData.Amount);
            hitItem.WorldPosition = hitPosition;

            hitItem.SetActive(true);
            hitItem.transform.SetAsLastSibling();
        }

        private void UpdateActiveItems(List<UIHitNumber> activeItems, List<UIHitNumber> inactiveItems)
        {
            for (int i = 0; i < _activeItems.Count; i++)
            {
                Debug.Log(i + "| UIHitNumbers UpdateActiveItems>>");

                var item = activeItems[i];
                if (item.IsFinished == true)
                {
                    item.SetActive(false);
                    activeItems.RemoveBySwap(i);
                    inactiveItems.Add(item);
                    Debug.Log(i + "| 해당 index hitItem activeItems Remove>> and inactiveItems Add");
                }
                else
                {
                    item.transform.position = _camera.WorldToScreenPoint(item.WorldPosition);
                    Debug.Log(i + "| 해당 index hitItem WorldToScreenPoint"+item.transform.position);
                }
            }
        }
    }
}
