using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectUI : MonoBehaviour
{
	[SerializeField] private TMP_InputField textfd;
	private void OnEnable()
	{
		textfd.onValueChanged.AddListener(delegate { SetName(); });
	}

	public void SelectChar(int charIndex)
	{
		Debug.Log("접속 컴퓨터 SelectChar>>" + charIndex);
		ClientInfo.CharId = charIndex;
	}

	public void SetName()
    {
		Debug.Log("SetName>>" + textfd.text);
		ClientInfo.Name = textfd.text;
    }
}
