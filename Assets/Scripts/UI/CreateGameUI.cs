using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CreateGameUI : MonoBehaviour
{
	public InputField lobbyName;
	public Dropdown world;
	public Slider playerCountSlider;
	public Image worldImage;
	public Text playerCountSliderText;
	public Image playerCountIcon;
	public Button confirmButton;

	//resources
	public Sprite padlockSprite, publicLobbyIcon;

	private void Start()
	{

		playerCountSlider.SetValueWithoutNotify(8);
		SetPlayerCount();

		world.ClearOptions();
		world.AddOptions(ResourceManager.Instance.worlds.Select(x => x.worldName).ToList());
		world.onValueChanged.AddListener(SetWorld);
		SetWorld(0);

		playerCountSlider.wholeNumbers = true;
		playerCountSlider.minValue = 1;
		playerCountSlider.maxValue = 8;
		playerCountSlider.value = 2;
		playerCountSlider.onValueChanged.AddListener(x => ServerInfo.MaxUsers = (int)x);

		lobbyName.onValueChanged.AddListener(x =>
		{
			ServerInfo.LobbyName = x;
			confirmButton.interactable = !string.IsNullOrEmpty(x);
		});
		lobbyName.text = ServerInfo.LobbyName = "Session" + Random.Range(0, 1000);

		ServerInfo.WorldId = world.value;
		ServerInfo.MaxUsers = (int)playerCountSlider.value;
	}

	public void SetWorld(int worldid)
	{
		ServerInfo.WorldId = worldid;
		worldImage.sprite = ResourceManager.Instance.worlds[worldid].worldIcon;
	}

	public void SetPlayerCount()
	{
		playerCountSlider.value = ServerInfo.MaxUsers;
		playerCountSliderText.text = $"{ServerInfo.MaxUsers}";
		playerCountIcon.sprite = ServerInfo.MaxUsers > 1 ? publicLobbyIcon : padlockSprite;
	}

	// UI Hooks

	private bool _lobbyIsValid;

	public void ValidateLobby()
	{
		_lobbyIsValid = string.IsNullOrEmpty(ServerInfo.LobbyName) == false;
	}

	public void TryFocusScreen(UIScreen screen)
	{
		if (_lobbyIsValid)
		{
			UIScreen.Focus(screen);
		}
	}

	public void TryCreateLobby(GameLauncher launcher)
	{
		if (_lobbyIsValid)
		{
			launcher.JoinOrCreateLobby();
			_lobbyIsValid = false;
		}
	}
}