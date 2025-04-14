using Fusion;
using UnityEngine;
using Projectiles;

public class World : NetworkBehaviour
{
	public static World Current { get; private set; }

	[Networked] public TickTimer StartRaceTimer { get; set; }

	public WorldDefinition definition;

	[SerializeField]
	private Gameplay _gameplayPrefab;
	private void Awake()
	{
		Current = this;

		RoomGameManager.SetWorld(this);
	}

	public override void Spawned()
	{
		base.Spawned();
	}

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
		Debug.Log($"Spawning Character for [{player.Username}] as {entity.name}");
		entity.transform.name = $"Character ([{player.Username}]) {charId}";
		Debug.Log("GamePlay(WorldMap) SpawnPlayer roomplayer InputAuthority(PlayerRef)" + player.Object.InputAuthority);
		Runner.SetPlayerObject(player.Object.InputAuthority, entity.Object);
	}

	public void GamePlaySpawn()
    {
		Runner.Spawn(_gameplayPrefab);
	}
	public void SceneLoadDone(NetworkRunner runner_)
    {
		// Prepare context
		var scene = runner_.SimulationUnityScene.GetComponent<Scene>(true);

		var context = scene.Context;
		context.Runner = runner_;

		// Assign context
		var contextBehaviours = runner_.SimulationUnityScene.GetComponents<IContextBehaviour>(true);
		foreach (var behaviour in contextBehaviours)
		{
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
	}
}