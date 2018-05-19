using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerScoreData : MonoBehaviour, INotifyPropertyChanged
{
	private int _score;
	private int _longestStreak;
	private int _enemiesKilled;

	public int Score
	{
		get { return _score; }
		set
		{
			if (value == _score) return;
			_score = value;
			if (Score > HighScore) HighScore = Score;
			OnPropertyChanged();
		}
	}

	public int CurrentStreak
	{
		get { return _currentStreak; }
		set
		{
			if (value == _currentStreak) return;
			_currentStreak = value;
			if (CurrentStreak > LongestStreak) LongestStreak = CurrentStreak;
			OnPropertyChanged();
		}
	}

	public int LongestStreak
	{
		get { return _longestStreak; }
		set
		{
			if (value == _longestStreak) return;
			_longestStreak = value;
			if (LongestStreak > LongestStreakEver) LongestStreakEver = LongestStreak;
			OnPropertyChanged();
		}
	}

	public int EnemiesKilled
	{
		get { return _enemiesKilled; }
		set
		{
			if (value == _enemiesKilled) return;
			_enemiesKilled = value;
			if (EnemiesKilled > MostEnemiesKilled) MostEnemiesKilled = EnemiesKilled;
			OnPropertyChanged();
		}
	}

	public int HighScore { get; set; }
	public int LongestStreakEver { get; set; }
	public int MostEnemiesKilled { get; set; }
	public bool FromScoreScreen = false;
	public bool FromCalibrationScreen = false;

	private string _songName;
	private int _currentStreak;
	public static PlayerScoreData Instance { get; private set; }
	private void Awake()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
	}

	void Start()
	{
		DontDestroyOnLoad(gameObject);
		SceneManager.sceneLoaded += SceneManager_sceneLoaded;
	}

	private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
	{
		if (arg0.name == "Game")
		{
			_songName = SongManager.Instance.SongName;
			Score = CurrentStreak = LongestStreak = EnemiesKilled = 0;
			HighScore = PlayerPrefs.GetInt($"{_songName}_HighScore", 0);
			LongestStreakEver = PlayerPrefs.GetInt($"{_songName}_LongestStreakEver", 0);
			MostEnemiesKilled = PlayerPrefs.GetInt($"{_songName}_MostEnemiesKilled", 0);
		}
		else if (arg0.name == "Score")
		{
			PlayerPrefs.GetInt($"{_songName}_HighScore", HighScore);
			PlayerPrefs.GetInt($"{_songName}_LongestStreakEver", LongestStreakEver);
			PlayerPrefs.GetInt($"{_songName}_MostEnemiesKilled", MostEnemiesKilled);
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	[NotifyPropertyChangedInvocator]
	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
