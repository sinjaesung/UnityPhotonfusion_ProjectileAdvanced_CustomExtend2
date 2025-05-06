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

    [Networked, Capacity(200)]
    public NetworkDictionary<PlayerRef, NetworkObject> Request_Players { get; }
    [Networked]
    private bool SceneMoving { get; set; }
    private int PrevPlayerCount;
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
            //roomplayer.RPC_RequestSceneChange(movescene, no.InputAuthority);
 
            if (!Request_Players.ContainsKey(no.InputAuthority))
            {
                Request_Players.Add(no.InputAuthority,no);                
            }   
        }
    }
    private void OnTriggerExit(Collider other)
    {
        roomplayer = FindObjectOfType<TestRoomPlayer>();

        if (other.TryGetComponent<NetworkObject>(out var no) && no.HasStateAuthority)
        {
            Debug.Log("Portal TRGEReExit" + no.transform.name + "," + no.HasStateAuthority);

            if (Request_Players.ContainsKey(no.InputAuthority))
            {
                Request_Players.Remove(no.InputAuthority);
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
        {
            return;
        }

        int c = 0;
        foreach(var e in Request_Players)
        {
            if (e.Value)
            {
                PlayerAgent item = e.Value.GetComponent<PlayerAgent>();
                if (item && item.IsValid)
                {
                    Debug.Log(c + $"|playeragent유효성검사>> {e.Key}:{e.Value.name}=>{item.IsValid}");
                }
            }
            else
            {
                Debug.Log(c + $"| {e.Key} 해당 networkObject Despawn되거나 유효하지 않음>>");
                //Request_Players.Remove(e.Key);
                Request_Players.Clear();
            }
            Debug.Log(c + $"| Request_Players {e.Key}:{(e.Value ? e.Value.name : "")}");
            c++;
        }

        var players = TestRoomPlayer.Players;
        int playercount = players.Count;
        int req_count = 0;

        for(int e=0; e<players.Count; e++)
        {
            var player_inputauthority = players[e].Object.InputAuthority;
            Debug.Log(e + $"| [[Portal]] roomplayer inputauthority: {player_inputauthority}");
            if (Request_Players.TryGet(player_inputauthority,out NetworkObject player))
            {
                if (!player) continue;
                if(player && player.GetComponent<Health>().IsSpawned && player.GetComponent<Health>().IsAlive)
                //Debug.Log(e+$"| [[Portal]] Request_Players 요청내역:{player.name}");
                req_count++;
            }
        }
        Debug.Log($"[[Portal]] req_count/playercount:{req_count}/{playercount}");
        if(playercount == req_count)
        {
            if (!SceneMoving)
            {
                Debug.Log("[[Portal]] 조건 만족시에 씬 이동>>");
                roomplayer.RPC_RequestSceneChange(movescene, players[0].Object.InputAuthority);
            }

            SceneMoving = true;
        }
    }
}
