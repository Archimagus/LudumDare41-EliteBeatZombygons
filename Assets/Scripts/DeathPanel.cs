using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeathPanel : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI _textElement;
	[SerializeField] private Button _continueButton;

	[SerializeField] private string _deathText;
	[SerializeField] private string _winText;

	private bool _win;

	private void OnEnable()
	{
		_textElement.text = _win ? _winText : _deathText;
		_continueButton.interactable = false;
		StartCoroutine(enableButton());
	}

	IEnumerator enableButton()
	{
		yield return new WaitForSeconds(1);
		_continueButton.interactable = true;
	}

	public void Show(bool win)
	{
		_win = win;
		gameObject.SetActive(true);
	}

	public void ShowScoreButtonClick()
	{
		GameManager.current.LoadScene("Score");
	}
}
