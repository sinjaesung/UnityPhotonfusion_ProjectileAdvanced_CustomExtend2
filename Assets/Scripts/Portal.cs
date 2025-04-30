using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Projectiles;
using Fusion;

public class Portal : NetworkBehaviour
{
    public TestRoomPlayer roomplayer;
    public string movescene;
    // Start is called before the first frame update
    void Start()
    {
        roomplayer = FindObjectOfType<TestRoomPlayer>();  
    }

    private void OnTriggerEnter(Collider other)
    {
        roomplayer = FindObjectOfType<TestRoomPlayer>();

        if (other.TryGetComponent<NetworkObject>(out var no) && no.HasStateAuthority)
        {
            Debug.Log("Portal TRGEReNTER" + no.transform.name + "," + no.HasStateAuthority);
            roomplayer.RPC_RequestSceneChange(movescene, no.InputAuthority);
        }
    }
}
