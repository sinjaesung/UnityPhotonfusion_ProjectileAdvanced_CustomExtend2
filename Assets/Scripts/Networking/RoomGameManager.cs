using System;
using Fusion;
using UnityEngine;
using Projectiles;

public class RoomGameManager : NetworkBehaviour
{
	public static event Action<RoomGameManager> OnLobbyDetailsUpdated;

	/*[SerializeField, Layer] private int groundLayer;
	public static int GroundLayer => Instance.groundLayer;
	[SerializeField, Layer] private int kartLayer;
	public static int KartLayer => Instance.kartLayer;*/


	//public new Camera camera;
	//private ICameraController cameraController;

	public static World CurrentWorld { get; private set; }
	public static bool IsPlaying => CurrentWorld != null;

	public static RoomGameManager Instance { get; private set; }

	public string worldName => ResourceManager.Instance.worlds[worldId].worldName;

	[Networked] public NetworkString<_32> LobbyName { get; set; }
	[Networked] public int worldId { get; set; }
	[Networked] public int MaxUsers { get; set; }


	private static void OnLobbyDetailsChangedCallback(RoomGameManager changed)
	{
		Debug.Log("RoomGameManager OnLobbyDetailsChangedCallback>>");
		OnLobbyDetailsUpdated?.Invoke(changed);
	}

	private ChangeDetector _changeDetector;

	private void Awake()
	{
		if (Instance)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	public override void Spawned()
	{
		base.Spawned();

		_changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
		Debug.Log("RoomGameManager Spawned>>");
		if (Object.HasStateAuthority)
		{
			LobbyName = ServerInfo.LobbyName;
			worldId = ServerInfo.WorldId;
			MaxUsers = ServerInfo.MaxUsers;
			Debug.Log("RoomGameManager Spawned HasStateAuthority Networked LobbyName,TrackId,GameTypeId,MaxUsers"+ LobbyName+",worldId:"+worldId+",MaxUsers:"+MaxUsers);
		}
	}

	public override void Render()
	{
		foreach (var change in _changeDetector.DetectChanges(this))
		{
			Debug.Log("RoomGameManager access LobbyName,worldId" + LobbyName + "," + worldId);
			switch (change)
			{
				case nameof(LobbyName):
				case nameof(worldId):
				case nameof(MaxUsers):
					OnLobbyDetailsChangedCallback(this);
					break;
			}
		}
	}

	/*private void LateUpdate()
	{
		// this shouldn't really be an interface due to how Unity handle's interface lifecycles (null checks dont work).
		if (cameraController == null) return;
		if (cameraController.Equals(null))
		{
			Debug.LogWarning("Phantom object detected");
			cameraController = null;
			return;
		}

		if (cameraController.ControlCamera(camera) == false)
		{
			Debug.Log("GameManager LateUpdate ControlCamera false>>");
			cameraController = null;
		}
	}*/

	/*public static void GetCameraControl(ICameraController controller)
	{
		Instance.cameraController = controller;
	}

	public static bool IsCameraControlled => Instance.cameraController != null;*/

	public static void SetWorld(World world)
	{
		CurrentWorld = world;
	}
}