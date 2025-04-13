using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{

	private void OnEnable()
	{
		SelectChar(ClientInfo.CharId);
	}

	public void SelectChar(int charIndex)
	{
		ClientInfo.CharId = charIndex;
		if (SpotlightGroup.Search("Character Display", out SpotlightGroup spotlight)) spotlight.FocusIndex(charIndex);
		//ApplyStats();

		if (RoomPlayer.Local != null)
		{
			RoomPlayer.Local.RPC_SetCharId(charIndex);
		}
	}
	/*public void SelectChar(int charIndex)
	{
		ClientInfo.CharId = charIndex;

		if (RoomPlayer.Local != null)
		{
			RoomPlayer.Local.RPC_SetCharId(charIndex);
		}
	}*/

	private void ApplyStats()
	{
		CharacterDefinition def = ResourceManager.Instance.characterDefinitions[ClientInfo.CharId];
	}
}
