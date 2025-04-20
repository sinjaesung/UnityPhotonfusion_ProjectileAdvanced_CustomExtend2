using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
	private void OnEnable()
	{
	}

	public void SelectChar(int charIndex)
	{
		Debug.Log("접속 컴퓨터 SelectChar>>" + charIndex);
		ClientInfo.CharId = charIndex;
	}
}
