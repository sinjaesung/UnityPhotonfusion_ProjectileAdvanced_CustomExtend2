using Fusion;
using FusionExamples.Utility;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
	public CharacterDefinition[] characterDefinitions;
	public WorldDefinition[] worlds;

	//public GameObject[] Characters;

	public static ResourceManager Instance => Singleton<ResourceManager>.Instance;

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}
}
