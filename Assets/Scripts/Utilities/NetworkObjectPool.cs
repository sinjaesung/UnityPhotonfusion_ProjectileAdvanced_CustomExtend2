using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

namespace Projectiles
{
    public class NetworkObjectPool : Fusion.Behaviour, INetworkObjectProvider
    {
        public SceneContext Context { get; set; }

        [SerializeField] private Dictionary<NetworkPrefabId, Stack<NetworkObject>> _cached = new(32);
        [SerializeField] private Dictionary<NetworkObject, NetworkPrefabId> _borrowed = new();

        NetworkObjectAcquireResult INetworkObjectProvider.AcquirePrefabInstance(NetworkRunner runner, in NetworkPrefabAcquireContext context, out NetworkObject result)
        {
            if (_cached.TryGetValue(context.PrefabId, out var objects) == false)
            {
                objects = _cached[context.PrefabId] = new Stack<NetworkObject>();
                Debug.Log("NetworkObjectPool AcquirePrefabInstance 오브젝트 새로 생성 요청Stack:" + objects.Count);
            }

            if (objects.Count > 0)
            {
                var oldInstance = objects.Pop();
                Debug.Log("NetworkObjectPool AcquirePrefabInstance objects.Pop()" + oldInstance + "~" + !oldInstance);
                if (!oldInstance)
                {
                    result = default;
                    return NetworkObjectAcquireResult.Ignore;
                }

                Debug.Log("NetworkObjectgPool AcquirePrefabInstance get oldInstance pop" + oldInstance.transform.name);
                _borrowed[oldInstance] = context.PrefabId;
                Debug.Log("NetworkObjectgPool AcquirePrefabInstance objects.Count _borrowed[oldInstance] = context.PrefabId > 0 >>" + oldInstance.name);

#if UNITY_EDITOR
                var originalPrefab = runner.Config.PrefabTable.Load(context.PrefabId, true);
                oldInstance.name = originalPrefab.name;
#endif

                oldInstance.SetActive(true);

                result = oldInstance;
                return NetworkObjectAcquireResult.Success;        
            }

            var original = runner.Config.PrefabTable.Load(context.PrefabId, true);
            Debug.Log("NetworkObjectPool AcquirePrefabInstance 새로생성 object>>" + original);
            if (original == null)
            {
                result = default;
                return NetworkObjectAcquireResult.Failed;
            }

            var instance = Instantiate(original);
            if (!instance)
            {
                result = default;
                return NetworkObjectAcquireResult.Ignore;
            }
            
            runner.MoveToRunnerScene(instance.gameObject);
            Debug.Log("NetworkObjectgPool AcquirePrefabInstance objects.Count==0 Instantiate instance>>" + instance.name);

#if UNITY_EDITOR
            instance.name = original.name;
#endif

            _borrowed[instance] = context.PrefabId;
            Debug.Log("NetworkObjectgPool AcquirePrefabInstance objects.Count==0  _borrowed[instance] = context.PrefabId >>" + instance.name);

            AssignContext(instance);

            for (int i = 0; i < instance.NestedObjects.Length; i++)
            {
                AssignContext(instance.NestedObjects[i]);
            }

            Debug.Log("NetworkObjectPool AcquirePrefabInstance");
            result = instance;
            return NetworkObjectAcquireResult.Success;
        }
        public void ClearExecute()
        {
            Debug.Log(">> NetworkObjectPool ClearExecute>>");
            _cached.Clear();
            _borrowed.Clear();
        }  
        void INetworkObjectProvider.ReleaseInstance(NetworkRunner runner, in NetworkObjectReleaseContext context)
        {
            if (context.IsNestedObject == true)
                return;

            NetworkObject instance = context.Object;
            if (instance == null)
                return;

            Debug.Log("NetworkObjectgPool ReleaseInstance context.Object name" + instance.name);

            if (instance.NetworkTypeId.IsSceneObject == false && runner.IsShutdown == false)
            {
                if (_borrowed.TryGetValue(instance, out var prefabID) == true)
                {
                    _borrowed.Remove(instance);
                    Debug.Log("NetworkObjectgPool ReleaseInstance _borrowed.Remove(instance);" + instance.name);
                    _cached[prefabID].Push(instance);
                    Debug.Log($"NetworkObjectgPool ReleaseInstance _cached[{prefabID}].Push(instance);" + instance.name);

                    instance.SetActive(false);
                    instance.transform.parent = null;
                    instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

#if UNITY_EDITOR
                    instance.name = $"(Cached) {instance.name}";
#endif
                }
                else
                {
                    Debug.Log("NetworkObjectgPool ReleaseInstance Not _borrowed Destroy" + instance.name);
                    Destroy(instance.gameObject);
                }
            }
            else
            {
                Debug.Log("NetworkObjectgPool ReleaseInstance runner.IsShutdown Destroy" + instance.name);
                try
                {
                    Destroy(instance.gameObject);
                }catch(Exception e)
                {
                    Debug.Log("NetworkObjectgPool ReleaseInstance Destroy" + e.Message);
                }
            }
        }

        private void AssignContext(NetworkObject instance)
        {
            for (int i = 0, count = instance.NetworkedBehaviours.Length; i < count; i++)
            {
                if (instance.NetworkedBehaviours[i] is IContextBehaviour cachedBehaviour)
                {
                    Debug.Log(i+"| NetworkObjectPool 생성오브젝트 AssignContext>>" + Context);
                    cachedBehaviour.Context = Context;
                }
            }
        }
    }
}
