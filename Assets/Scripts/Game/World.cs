using Fusion;
using UnityEngine;
using Projectiles;
using System;
using System.Collections.Generic;
using Fusion.Sockets;

public class World : SimulationBehaviour
{
	public static World Current { get; private set; }

	[Networked] public TickTimer StartRaceTimer { get; set; }

	public WorldDefinition definition;

	[SerializeField]
	private Gameplay _gameplayPrefab;
	private NetworkRunner runners; 
	private void Start()
	{
		Debug.Log("World Start>>");
		Current = this;

		RoomGameManager.SetWorld(this);
	}

	/*public override void Spawned()
	{
		base.Spawned();
	}
*/
	private void OnDestroy()
	{
		RoomGameManager.SetWorld(null);
	}

	public void SpawnPlayer(NetworkRunner runner, RoomPlayer player)
	{
		var index = RoomPlayer.Players.IndexOf(player);
		var charId = player.CharId;

		Debug.Log($"World SpawnPlayer>>{player.Object.InputAuthority} => charId:" + charId);
		var prefab = ResourceManager.Instance.characterDefinitions[charId].prefab;

		// Spawn player
		var entity = runner.Spawn(
			prefab,
			Vector3.zero,
			Quaternion.identity,
			player.Object.InputAuthority
		);

		entity.RoomUser = player;
		player.GameState = RoomPlayer.EGameState.GameReady;
		player.player = entity;

		entity.SetCharacterIndex(charId);
		Debug.Log($"Spawning Character for [{player.Username}]");
		entity.transform.name = $"Character ([{player.Username}]) {charId}";
		Debug.Log("GamePlay(WorldMap) SpawnPlayer roomplayer InputAuthority(PlayerRef)" + player.Object.InputAuthority);
		runner.SetPlayerObject(runner.LocalPlayer, entity.Object);

		runners = runner;
	}

    private void Update()
    {
		//if (!runners) return;

		Debug.Log("World Update>>");

		if (!FindObjectOfType<Scene>())
		{
			Debug.Log("World Scene no");
			return;
		}

		var SceneContext = FindObjectOfType<Scene>().Context;

		//if ((runners.LocalPlayer)==null) return;
		if (!SceneContext) return;
		if (!SceneContext.Runner) return;

		var localPlayer = SceneContext.Runner.GetPlayerObject(SceneContext.Runner.LocalPlayer);
		if (!localPlayer) return;
		Debug.Log("World Update  localPlayer  " + localPlayer.transform.name+","+ localPlayer.InputAuthority);
		if (localPlayer.GetComponent<Player>().ActiveAgent == null) return;
		SceneContext.LocalAgent = localPlayer != null ? localPlayer.GetComponent<Player>().ActiveAgent : null;
		Debug.Log("World Update SceneContext.LocalAgent " + SceneContext.LocalAgent.transform.name);
	}

    public void GamePlaySpawn(NetworkRunner runner)
    {
		Debug.Log("World GamePlaySpawn>>" + _gameplayPrefab.transform.name);
		runner.Spawn(_gameplayPrefab);
	}
	/*public void SceneLoadDone(NetworkRunner runner_)
    {
		// Prepare context
		var scene = runner_.SimulationUnityScene.GetComponent<Scene>(true);

		var context = scene.Context;
		context.Runner = runner_;

		// Assign context
		var contextBehaviours = runner_.SimulationUnityScene.GetComponents<IContextBehaviour>(true);
		int b = 0;
		foreach (var behaviour in contextBehaviours)
		{
			Debug.Log(b+"| World SceneLoadDone contextBehaviours behaviour" + behaviour);
			behaviour.Context = context;
		}

		var objectPool = GetComponent<NetworkObjectPool>();
		objectPool.Context = context;
		Debug.Log("World OnSceneLoadDone");

		if (runner_.Config.PeerMode == NetworkProjectConfig.PeerModes.Multiple)
		{
			Debug.Log("World OnSceneLoadDone PeerMode==PeerModes.Multiple");
			// In case of multipeer mode, fix the scene lighting
			var renderSettingsUpdated = scene.GetComponent<RenderSettingsUpdater>();
			renderSettingsUpdated.ApplySettings();
		}

		FindObjectOfType<Gameplay>().Context_GamePlayAssign();
	}*/
}