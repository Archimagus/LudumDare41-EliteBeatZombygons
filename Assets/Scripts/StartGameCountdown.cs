using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartGameCountdown : MonoBehaviour
{
	[SerializeField] private ScoreDisplay _scoreDisplay;
	private TextMeshProUGUI _text;
	private SongManager _songManager;
	void Start()
	{
		_text = GetComponent<TextMeshProUGUI>();
		_songManager = SongManager.Instance;
		_songManager.Beat += _songManager_Beat;
	}

	private void _songManager_Beat(int beat)
	{
		int b = -(beat - _songManager.FirstBeatIndex);
		if (b <= 3)
		{
			_text.text = b.ToString();
		}

		if (b == 0)
		{
			_text.text = "GO!";
		}

		if (b == -1)
		{
			gameObject.SetActive(false);
			_scoreDisplay?.gameObject.SetActive(true);
			_songManager.Beat -= _songManager_Beat;
		}
	}
}