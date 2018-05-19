using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
	public GameObject mainPanel;
	public TextMeshProUGUI scoreUI;
	public TextMeshProUGUI hitStreakUI;
	public TextMeshProUGUI enemiesKilledUI;
	public Button continueButton;
	public GameObject message;

	[SerializeField] private Sprite __filledStar;

	public Ratings[] ratings;

	private AudioSource _sfxSource;

	private float finalScore = 0;
	private bool _runCounters = false;
	private ScreenData _screenData;
	[SerializeField] private UISounds _uISounds;

	[SerializeField] private Song _song;
	private float lastSound = 0.0f;
	private int _ratingAchieved = 0;

	void Start ()
	{
		_sfxSource = GetComponent<AudioSource>();
		_screenData = new ScreenData();

		if(LevelSelectPersistence.Instance != null)
		{
			_song = LevelSelectPersistence.Instance.SelectedSong;
		}

		ratings[0].scoreThreshold.text = _song.RatingThreshold.thresholds[0].ToString();
		ratings[1].scoreThreshold.text = _song.RatingThreshold.thresholds[1].ToString();
		ratings[2].scoreThreshold.text = _song.RatingThreshold.thresholds[2].ToString();

		if (PlayerScoreData.Instance != null)
		{
			_screenData.score = PlayerScoreData.Instance.Score;
			_screenData.streak = PlayerScoreData.Instance.LongestStreak;
			_screenData.enemiesKilled = PlayerScoreData.Instance.EnemiesKilled;

			PlayerScoreData.Instance.FromScoreScreen = true;
		}
		else
		{
			_screenData.score = 10000;
			_screenData.streak = 120;
			_screenData.enemiesKilled = 40;
		}

		hitStreakUI.text = _screenData.streak.ToString();
		enemiesKilledUI.text = _screenData.enemiesKilled.ToString();

		StartCoroutine(DelayedStart());
	}

	private void Update()
	{
		if(_runCounters)
		{
			ScoreCounter();

			if(Input.GetKeyDown(KeyCode.Space))
			{
				message.SetActive(false);
				SkipCounter();
			}
		}
	}

	public void Continue()
	{
		SaveRating();
		_sfxSource.PlayOneShot(_uISounds.buttonClick);
		mainPanel.SetActive(false);

		StartCoroutine(BackToMainMenu());
	}

	private void StartDelay()
	{
		StartCoroutine(DelayedStart());
	}

	private void ScoreCounter()
	{
		if(finalScore < _screenData.score)
		{
			float counter = (Time.deltaTime * 10000);
			finalScore += (int)counter;

			if(Time.time > 0.05f + lastSound)
			{
				_sfxSource.PlayOneShot(_uISounds.buttonClick);
				lastSound = Time.time;
			}
		}
		else
		{
			finalScore = _screenData.score;
			_runCounters = false;
			continueButton.interactable = true;
			message.SetActive(false);
		}

		if(_song != null)
		{
			UpdateRatings();
		}

		scoreUI.text = finalScore.ToString();
	}

	private void SkipCounter()
	{
		finalScore = _screenData.score;
		FinalRating();
		_runCounters = false;	
		scoreUI.text = finalScore.ToString();
		continueButton.interactable = true;
	}

	private void UpdateRatings()
	{
		if(_runCounters)
		{
			if (finalScore >= _song.RatingThreshold.thresholds[0] && !ratings[0].ratingAchieved)
			{
				ratings[0].ratingAchieved = true;
				ratings[0].ratingImage.sprite = __filledStar;
				_sfxSource.PlayOneShot(_uISounds.achievedRating[0]);
				_ratingAchieved = 1;
			}

			if (finalScore >= _song.RatingThreshold.thresholds[1] && !ratings[1].ratingAchieved)
			{
				ratings[1].ratingAchieved = true;
				ratings[1].ratingImage.sprite = __filledStar;
				_sfxSource.PlayOneShot(_uISounds.achievedRating[1]);
				_ratingAchieved = 2;
			}

			if (finalScore >= _song.RatingThreshold.thresholds[2] && !ratings[2].ratingAchieved)
			{
				ratings[2].ratingAchieved = true;
				ratings[2].ratingImage.sprite = __filledStar;
				_sfxSource.PlayOneShot(_uISounds.achievedRating[2]);
				_ratingAchieved = 3;
			}
		}		
	}

	private void FinalRating()
	{
		if (finalScore >= _song.RatingThreshold.thresholds[0])
		{
			ratings[0].ratingAchieved = true;
			ratings[0].ratingImage.sprite = __filledStar;
			_ratingAchieved = 1;
		}

		if (finalScore >= _song.RatingThreshold.thresholds[1])
		{
			ratings[1].ratingAchieved = true;
			ratings[1].ratingImage.sprite = __filledStar;
			_ratingAchieved = 2;
		}

		if (finalScore >= _song.RatingThreshold.thresholds[2])
		{
			ratings[2].ratingAchieved = true;
			ratings[2].ratingImage.sprite = __filledStar;
			_ratingAchieved = 3;
		}
	}

	private void SaveRating()
	{
		// Set to default song for initialization
		string key =_song.MusicClip.name + "Played";
		string key2 = _song.MusicClip.name + "Rating";

		string currentSongPlayed = "N";
		int currentSavedRating = 0;		

		if (LevelSelectPersistence.Instance != null)
		{
			key = LevelSelectPersistence.Instance.SelectedSong.MusicClip.name + "Played";
			key2 = LevelSelectPersistence.Instance.SelectedSong.MusicClip.name + "Rating";
		}

		currentSongPlayed = PlayerPrefs.GetString(key, "N");
		currentSavedRating = PlayerPrefs.GetInt(key2, 0);

		if (currentSongPlayed == "N")
		{
			PlayerPrefs.SetString(key, "Y");
		}

		if (_ratingAchieved > currentSavedRating)
		{
			PlayerPrefs.SetInt(key2, _ratingAchieved);
		}
	}

	IEnumerator DelayedStart()
	{
		yield return new WaitForSeconds(2);
		_runCounters = true;
		message.SetActive(true);
	}

	IEnumerator BackToMainMenu()
	{
		yield return new WaitForSeconds(1);
		SceneManager.LoadScene("MainMenu");
	}

	[System.Serializable]
	public class ScreenData
	{
		public int score;
		public int streak;
		public int enemiesKilled;
		public Ratings[] ratings;
	}

	[System.Serializable]
	public class Ratings
	{
		public Image ratingImage;
		public bool ratingAchieved;
		public TextMeshProUGUI scoreThreshold;
	}

	[System.Serializable]
	public class UISounds
	{
		public AudioClip buttonHover;
		public AudioClip buttonClick;
		public AudioClip[] achievedRating;
	}
}
