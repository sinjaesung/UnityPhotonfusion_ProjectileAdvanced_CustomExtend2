using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;
using Projectiles;

public class TestRoomPlayer : NetworkBehaviour
{
    //공유한다>>그 개체의값은 RoomPlayer생성시마다 초기화되지않고 값 유지공유
    public static readonly List<TestRoomPlayer> Players = new List<TestRoomPlayer>();

    [Networked] public int CharId { get; set; }
    public static TestRoomPlayer Local;
    public GameManager gamemanger;
    void Start()
    {
        Debug.Log("TestRoomPlayer Start??");
        DontDestroyOnLoad(gameObject);
    }

    public override void Spawned()
    {
        gamemanger = FindObjectOfType<GameManager>();
        base.Spawned();

        Debug.Log("TestRoomPlayer Spawned");

        if (Object.HasInputAuthority)
        {
            Local = this;

            Debug.Log($"{Object.InputAuthority} >> TestRoomPlayer Spawned>> HasInputAuthority 선택했었던 CharId>>" + ClientInfo.CharId);
            RPC_SetCharId(ClientInfo.CharId,Object.InputAuthority);
        }
        Players.Add(this);
        Debug.Log("TestRoomPlayer Spawned PlayersAdd playersCount:" + Players.Count);
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    public void RPC_SetCharId(int id,PlayerRef playerref)
    {
        Debug.Log($"{playerref} >> TestRoomPlayer RPC_SetCharId>>" + id);
        CharId = id;

        gamemanger.SpawnPlayer(playerref, id);
    }
    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void RPC_RequestSceneChange(string targetSceneName, PlayerRef playerref)
    {
        if (gamemanger != null)
        {
            Debug.Log($"{playerref} >> TestRoomPlayer RPC_RequestSceneChange>>" + targetSceneName);
            gamemanger.RequestSceneChange(targetSceneName);
        }
    }

    private void OnDisable()
    {
        // OnDestroy does not get called for pooled objects
        Players.Remove(this);
    }
    public static void RemovePlayer(NetworkRunner runner, PlayerRef p)
    {
        var roomPlayer = Players.FirstOrDefault(x => x.Object.InputAuthority == p);
        if (roomPlayer != null)
        {
            Debug.Log("TestRoomPlayer RemovePlayer>>"+ p);
            /*if (roomPlayer.player_ != null)
                runner.Despawn(roomPlayer.player_.Object);*/

            Players.Remove(roomPlayer);
            runner.Despawn(roomPlayer.Object);
        }
    }
}
