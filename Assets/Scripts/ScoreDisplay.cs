using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ScoreDisplay : MonoBehaviour
{
	private TextMeshProUGUI _text;
	private void Start()
	{
		PlayerScoreData.Instance.PropertyChanged += Player_PropertyChanged;
		_text = GetComponent<TextMeshProUGUI>();
		_text.text = "Score: 0000";
	}

	private void Player_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(PlayerScoreData.Score))
		{
			_text.text = $"Score: {PlayerScoreData.Instance.Score:0000}";
		}
	}
}
