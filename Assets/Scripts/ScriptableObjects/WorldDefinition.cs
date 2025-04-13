using UnityEngine;

[CreateAssetMenu(fileName = "New world", menuName = "Scriptable Object/world Definition")]
public class WorldDefinition : ScriptableObject
{
	public string worldName;
	public Sprite worldIcon;
	public int buildIndex;
}
