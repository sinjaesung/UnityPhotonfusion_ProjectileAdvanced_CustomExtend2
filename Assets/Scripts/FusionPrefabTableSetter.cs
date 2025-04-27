/*using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Projectiles;

public class CustomPrefabTable : MonoBehaviour
{
    [Header("네트워크로 스폰할 Prefab 리스트")]
    public List<NetworkObject> networkPrefabs = new List<NetworkObject>();

    private Dictionary<NetworkPrefabId, NetworkObject> prefabIdToPrefab = new Dictionary<NetworkPrefabId, NetworkObject>();
    private Dictionary<NetworkObject, NetworkPrefabId> prefabToPrefabId = new Dictionary<NetworkObject, NetworkPrefabId>();

    private void Awake()
    {
        for (int i = 0; i < networkPrefabs.Count; i++)
        {
            var prefab = networkPrefabs[i];
            if (prefab == null) continue;

            NetworkPrefabId id = (NetworkPrefabId)i;
            prefabIdToPrefab[id] = prefab;
            prefabToPrefabId[prefab] = id;
        }

        var runner = FindObjectOfType<NetworkRunner>();
        if (runner != null)
        {
            runner.Config.PrefabSource = this;
        }
        else
        {
            Debug.LogError("CustomPrefabTable: NetworkRunner를 찾을 수 없습니다.");
        }
    }

    public NetworkObject GetPrefab(NetworkPrefabId prefabId)
    {
        if (prefabIdToPrefab.TryGetValue(prefabId, out var prefab))
        {
            return prefab;
        }
        return null;
    }

    public NetworkPrefabId GetPrefabId(NetworkObject prefab)
    {
        if (prefabToPrefabId.TryGetValue(prefab, out var id))
        {
            return id;
        }
        return NetworkPrefabId.None;
    }
}
*/