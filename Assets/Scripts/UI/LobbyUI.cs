using System.Collections.Generic;
using System.Linq;
//using Managers;
using UnityEngine;
using UnityEngine.UI;
using Projectiles;
using Fusion;

public class LobbyUI : MonoBehaviour, IDisabledUI
{
	public GameObject textPrefab;
	public Transform parent;
	public Button readyUp;
	public Button customizeButton;
	public Text worldNameText;
	public Text lobbyNameText;
	public Dropdown worldNameDropdown;
	public Image worldIconImage;

	private static readonly Dictionary<RoomPlayer, LobbyItemUI> ListItems = new Dictionary<RoomPlayer, LobbyItemUI>();
	private static bool IsSubscribed;

	private void Awake()
	{
		worldNameDropdown.onValueChanged.AddListener(x =>
		{
			var gm = RoomGameManager.Instance;
			if (gm != null) gm.worldId = x;
			Debug.Log("LobbyUI worldName worldId value changed>>" + x);
		});


		RoomGameManager.OnLobbyDetailsUpdated += UpdateDetails;

		RoomPlayer.PlayerChanged += (player) =>
		{
			var isLeader = RoomPlayer.Local.IsLeader;
			worldNameDropdown.interactable = isLeader;
			customizeButton.interactable = !RoomPlayer.Local.IsReady;
		};
	}

	void UpdateDetails(RoomGameManager manager)
	{
		Debug.Log("LobbyUI UpdateDetails>>");
		lobbyNameText.text = "Room Code: " + manager.LobbyName;
		//worldNameText.text = manager.worldName;

		var worlds = ResourceManager.Instance.worlds;
		var trackOptions = worlds.Select(x => x.worldName).ToList();

		worldNameDropdown.ClearOptions();
		worldNameDropdown.AddOptions(trackOptions);
		worldNameDropdown.value = RoomGameManager.Instance.worldId;

		worldIconImage.sprite = ResourceManager.Instance.worlds[RoomGameManager.Instance.worldId].worldIcon;
	}

	public void Setup()
	{
		if (IsSubscribed) return;

		RoomPlayer.PlayerJoined += AddPlayer;
		RoomPlayer.PlayerLeft += RemovePlayer;

		RoomPlayer.PlayerChanged += EnsureAllPlayersReady;

		readyUp.onClick.AddListener(ReadyUpListener);

		IsSubscribed = true;

		Debug.Log("LobbyUI Setup>>");
	}

	private void OnDestroy()
	{
		if (!IsSubscribed) return;

		RoomPlayer.PlayerJoined -= AddPlayer;
		RoomPlayer.PlayerLeft -= RemovePlayer;

		readyUp.onClick.RemoveListener(ReadyUpListener);

		IsSubscribed = false;
	}

	private void AddPlayer(RoomPlayer player)
	{
		if (ListItems.ContainsKey(player))
		{
			var toRemove = ListItems[player];
			Destroy(toRemove.gameObject);

			ListItems.Remove(player);
		}

		var obj = Instantiate(textPrefab, parent).GetComponent<LobbyItemUI>();
		obj.SetPlayer(player);

		ListItems.Add(player, obj);
		Debug.Log("LobbyUI PlayerAdded AddPlayer>>" + player.Object.InputAuthority+">charId:"+player.CharId);

		UpdateDetails(RoomGameManager.Instance);
	}

	private void RemovePlayer(RoomPlayer player)
	{
		if (!ListItems.ContainsKey(player))
			return;

		var obj = ListItems[player];
		if (obj != null)
		{
			Destroy(obj.gameObject);
			Debug.Log("LobbyUI RemovePlayer>>" + player.Object.InputAuthority + ">charId:" + player.CharId);
			ListItems.Remove(player);
		}
	}

	public void OnDestruction()
	{
	}

	private void ReadyUpListener()
	{
		var local = RoomPlayer.Local;
		if (local && local.Object && local.Object.IsValid)
		{
			Debug.Log("LobbyUI ReadyUpListener LocalPlayer ReadyUp>>" + local.transform.name);
			local.RPC_ChangeReadyState(!local.IsReady);
		}
	}

	private void EnsureAllPlayersReady(RoomPlayer lobbyPlayer)
	{
		if (!RoomPlayer.Local.IsLeader)
			return;

		if (IsAllReady())
		{
			int scene = ResourceManager.Instance.worlds[RoomGameManager.Instance.worldId].buildIndex;

			Debug.Log("LobbyUI IsAllReady()>> LevelManager.LoadTrack:" + scene);
			LevelManager.LoadTrack(scene);
		}
	}

	private static bool IsAllReady() => RoomPlayer.Players.Count > 0 && RoomPlayer.Players.All(player => player.IsReady);
}