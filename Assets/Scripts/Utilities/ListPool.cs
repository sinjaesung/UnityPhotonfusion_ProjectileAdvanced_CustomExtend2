using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Projectiles
{
    public class ListPool<T>
    {
        // CONSTANTS

        private const int POOL_CAPACITY = 4;
        private const int LIST_CAPACITY = 16;

        // PUBLIC MEMBERS

        public static readonly ListPool<T> Shared = new();

        // PRIVATE MEMBERS

        private List<List<T>> _pool = new(POOL_CAPACITY);

        // PUBLIC METHODS

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<T> Get(int capacity)
        {
            lock (_pool)
            {
                int poolCount = _pool.Count;

                if (poolCount == 0)
                {

                    UnityEngine.Debug.Log($"ListPool _pool Count==0 new List 생성>>");
                    return new List<T>(capacity > 0 ? capacity : LIST_CAPACITY);
                }

                UnityEngine.Debug.Log($"ListPool poolCount {poolCount}");
                for (int i = 0; i < poolCount; ++i)
                {
                    List<T> list = _pool[i];

                    UnityEngine.Debug.Log(i+$"| ListPool list item:{list},list.Capacity{list.Capacity}<{capacity}");
                    if (list.Capacity < capacity)
                        continue;
                    UnityEngine.Debug.Log(i + $"| target list item RemoveBySwap({i}) return {list}");
                    _pool.RemoveBySwap(i);
                    UnityEngine.Debug.Log(i + $"| 해당 {i} list item _pool.RemoveBySwap{i}=>{list}");
                    return list;
                }

                int lastListIndex = poolCount - 1;

                List<T> lastList = _pool[lastListIndex];
                UnityEngine.Debug.Log($"ListPool lastList item return {lastListIndex} => {lastList}");
                lastList.Capacity = capacity;

                UnityEngine.Debug.Log($"Listpool lastListIndex[{lastListIndex}] lastList return>>");
                _pool.RemoveAt(lastListIndex);

                return lastList;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(List<T> list)
        {
            if (list == null)
                return;

            list.Clear();

            lock (_pool)
            {
                UnityEngine.Debug.Log($"ListPool Return list >> _pool.Add({list})");

                _pool.Add(list);
            }
        }
    }

    public static class ListPool
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> Get<T>(int capacity)
        {
            return ListPool<T>.Shared.Get(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return<T>(List<T> list)
        {
            ListPool<T>.Shared.Return(list);
        }
    }
}
