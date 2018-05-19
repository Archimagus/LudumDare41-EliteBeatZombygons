using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

[CreateAssetMenu]
public class Song : ScriptableObject
{
	[Tooltip("The Music")] public AudioClip MusicClip;
	[Tooltip("Beats Per Minute")] public uint BPM = 100;
	[Tooltip("Number of beats for intro")] public uint IntroBeats = 8;
	[Tooltip("Number of beats before the end of the track to be considered as the end of the song")] public uint OutroBeats;
	[Tooltip("Offset in seconds from the beginning of the song to where the beat starts.")]
	[SerializeField] private double _beatOffset;
	[Tooltip("How close to the beat does the player need to have an input for success.")]
	[SerializeField] private double _beatInputThreshold = 0.1;
	public bool Loop;
	[Space]
	[Header("Level Data")]
	public GameObject LevelPrefab;
	public Wave[] Waves;

	public RatingThreshold RatingThreshold;

	public int LastBeat { get { return _lastBeatIndex; } }
	public double BeatInputThreshold { get { return _beatInputThreshold; } }

	public double BeatTime
	{
		get { return _beatTime; }
		private set { _beatTime = value; }
	}

	public double StartTime
	{
		get { return _startTime; }
		set
		{
			_startTime = value;
			_nextBeat = 0;
		}
	}

	public int AudioOffsetMilliseconds
	{
		get { return _audioOffsetMilliseconds; }
		set
		{
			_audioOffsetMilliseconds = value;
			_audioOffsetSeconds = TimeSpan.FromMilliseconds(value).TotalSeconds;
		}
	}

	public double Length { get { return Loop ? double.PositiveInfinity : MusicClip.length; } }


	[SerializeField, HideInInspector]
	private double[] _beatTimes;
	[SerializeField, HideInInspector]
	private double _length;
	[SerializeField, HideInInspector]
	private double _beatTime;
	[SerializeField, HideInInspector]
	private int _lastBeatIndex;

	private int _nextBeat;
	private double _startTime;
	private int _audioOffsetMilliseconds;
	private double _audioOffsetSeconds;
	private AudioSource _audioSource;
	private int _nextBeatIndex;
	private double _lastBeat;

	private void OnValidate()
	{
#if UNITY_EDITOR
		if (MusicClip != null && BPM > 0)
		{
			BeatTime = 60.0 / BPM;
			var numBeats = (int)((MusicClip.length - _beatOffset) / BeatTime);
			_length = (double)MusicClip.samples / MusicClip.frequency;
			_beatTimes = new double[numBeats];
			double time = _beatOffset;
			for (int i = 0; i < numBeats; i++)
			{
				_beatTimes[i] = time;
				time += BeatTime;
			}

			_lastBeatIndex = Loop ? int.MaxValue : _beatTimes.Length - (int)OutroBeats;
		}
#endif
	}

	public void StartSong(AudioSource audioSoruce, SongManager songManager)
	{
		_audioSource = audioSoruce;
		foreach (var wave in Waves)
		{
			wave.Spawned = false;
		}

		songManager.StartCoroutine(startSong());
	}
	IEnumerator startSong()
	{
		while (MusicClip.loadState != AudioDataLoadState.Loaded)
		{
			yield return null;
		}
		Application.targetFrameRate = (int)Mathf.Max(BPM * 2, 100);
		AudioOffsetMilliseconds = PlayerPrefs.GetInt("_audioOffset", 0);
		_audioSource.clip = MusicClip;
		_audioSource.loop = Loop;
		StartTime = AudioSettings.dspTime + 1;
		_audioSource.PlayScheduled(StartTime);
	}

	private double timeInSong()
	{
		var time = AudioSettings.dspTime - StartTime;
		while (time > _length)
		{
			time -= _length;
		}

		return time;
	}

	private int nextBeatIndex()
	{
		double time = timeInSong();
		for (int i = 0; i < _beatTimes.Length; i++)
		{
			if (_beatTimes[i] + _audioOffsetSeconds > time)
			{
				return i;
			}
		}

		return 0;
	}
	private double nextBeatTime()
	{
		int i = nextBeatIndex();
		double nextBeat;
		if (i == 0)
		{
			_lastBeat = _beatTimes[_beatTimes.Length - 1];
			nextBeat = _lastBeat + BeatTime;
		}
		else
		{
			_lastBeat = _beatTimes[i - 1];
			nextBeat = _beatTimes[i];
		}
		_lastBeat += _audioOffsetSeconds;
		nextBeat += _audioOffsetSeconds;
		return nextBeat;
	}

	public double TimeFromBeat()
	{
		if (_nextBeat > LastBeat)
			return double.PositiveInfinity;

		double time = timeInSong();
		var nextBeat = nextBeatTime();
		var timeFromBeat = Math.Min(Math.Abs(time - _lastBeat), Math.Abs(nextBeat - time));
		return timeFromBeat;
	}


	public bool CheckBeat()
	{
		if (AudioSettings.dspTime < StartTime)
			return false;

		int nextBeat = nextBeatIndex();
		if (nextBeat > _nextBeat || (_nextBeat != 0 && nextBeat == 0))
		{
			_nextBeat = nextBeat;
			SpawnWaves();
			return true;
		}


		return false;
	}

	private void SpawnWaves()
	{
		if (Waves.Length > 0)
		{
			double time = timeInSong();
			foreach (var wave in Waves)
			{
				if (wave.SpawnTime < time && wave.Spawned == false)
				{
					wave.Spawned = true;
					int spawnPointIndex = 0;
					foreach (var enemy in wave.Enemies)
					{
						for (int i = 0; i < wave.SpawnLocations.Length; i++)
						{
							bool success = GameManager.current.SpawnEnemy(enemy, wave.SpawnLocations[spawnPointIndex++]);
							if (spawnPointIndex >= wave.SpawnLocations.Length)
								spawnPointIndex = 0;
							if (success)
								break;
						}
					}
					break;
				}
			}
		}
	}
}

[Serializable]
public class Wave
{
	public double SpawnTime;
	public ZombygonData[] Enemies;
	public string[] SpawnLocations;
	public bool Spawned { get; set; }
}

[Serializable]
public class RatingThreshold
{
	public float[] thresholds;
}