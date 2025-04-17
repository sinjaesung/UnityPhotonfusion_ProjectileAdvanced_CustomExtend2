using System.Collections;
using System.Collections.Generic;
using Fusion;
using FusionExamples.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;
using Projectiles;

public class LevelManager : NetworkSceneManagerDefault
{
	public const int LAUNCH_SCENE = 0;
	public const int LOBBY_SCENE = 1;

	[SerializeField] private UIScreen _dummyScreen;
	[SerializeField] private UIScreen _lobbyScreen;
	//[SerializeField] private CanvasFader fader;

	public static LevelManager Instance => Singleton<LevelManager>.Instance;

	public static void LoadMenu()
	{
		Instance.Runner.LoadScene(SceneRef.FromIndex(LOBBY_SCENE));
	}

	public static void LoadTrack(int sceneIndex)
	{
		Instance.Runner.LoadScene(SceneRef.FromIndex(sceneIndex));
	}

	protected override IEnumerator LoadSceneCoroutine(SceneRef sceneRef, NetworkLoadSceneParameters sceneParams)
	{
		Debug.Log($"LevelManager LoadSceneCoroutine Loading scene {sceneRef}");

		PreLoadScene(sceneRef.AsIndex);

		yield return base.LoadSceneCoroutine(sceneRef, sceneParams);

		// Delay one frame, so we're sure level objects has spawned locally
		yield return null;

		// Now we can safely spawn characters
		if (RoomGameManager.CurrentWorld != null && sceneRef.AsIndex > LOBBY_SCENE)
		{
			//FindObjectOfType<World>().GamePlaySpawn(Runner);

			yield return new WaitForSeconds(0.2f);
			//FindObjectOfType<World>().SceneLoadDone(Runner);


			Debug.Log("LevelManager LoadSceneCoroutine>> Runner.GameMode>> RoomGameManager.CurrentWorld" + Runner.GameMode);
			if (Runner.GameMode == GameMode.Host)
			{
				int d = 0;
				foreach (var player in RoomPlayer.Players)
				{
					Debug.Log("LevelManager LoadSceneCoroutine Spawn Players>>" + d + "| " + player.transform.name);
					player.GameState = RoomPlayer.EGameState.GameReady;
					RoomGameManager.CurrentWorld.SpawnPlayer(Runner, player);
					d++;
				}
			}
			//FindObjectOfType<World>().SceneLoadDone(Runner);
		}

		//PostLoadScene();
	}

	private void PreLoadScene(int scene)
	{
		if (scene > LOBBY_SCENE)
		{
			// Show an empty dummy UI screen - this will stay on during the game so that the game has a place in the navigation stack. Without this, Back() will break
			Debug.Log("PreLoadScene Showing Dummy LoadingScreen");
			UIScreen.Focus(_dummyScreen);
		}
		else if (scene == LOBBY_SCENE)
		{
			foreach (RoomPlayer player in RoomPlayer.Players)
			{
				player.IsReady = false;
			}
			Debug.Log("PreLoadScene Load LobbyScene>>");
			UIScreen.activeScreen.BackTo(_lobbyScreen);
		}
		else
		{
			Debug.Log("PreLoadScene BackToInitial>>");
			UIScreen.BackToInitial();
		}
		//fader.gameObject.SetActive(true);
		//fader.FadeIn();
	}

	private void PostLoadScene()
	{
		//fader.FadeOut();
	}
}
