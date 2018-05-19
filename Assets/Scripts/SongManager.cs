using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SongManager : MonoBehaviour
{
	public static SongManager Instance { get; private set; }

	[SerializeField] private Song _song;
	[SerializeField] private GameObject _levelLayoutGameObject;

	[SerializeField] private bool _getSongFromPersistence=true;

	public double Length
	{
		get { return _song.Length; }
	}

	public double PlayTime
	{
		get { return _audioSoruce.time; }
	}

	public int BMP
	{
		get { return (int)_song.BPM; }
	}
	public double BeatTime
	{
		get { return _song.BeatTime; }
	}

	public double BeatInputThreshold
	{
		get { return _song.BeatInputThreshold; }
	}

	public int AudioOffsetMilliseconds
	{
		get { return _song.AudioOffsetMilliseconds; }
		set { _song.AudioOffsetMilliseconds = value; }
	}

	public string SongName { get { return _song.name; } }
	public int LastBeatIndex { get{return _song.LastBeat; } }
	public int FirstBeatIndex { get { return (int)_song.IntroBeats; } }

	public double TimeFromBeat()
	{
		double time = _song.TimeFromBeat();
		return time;
	}

	public event Action<int> Beat;

	private AudioSource _audioSoruce;
	private int _beatNumber;
	private double _songStartTime;

	private void Awake()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		_audioSoruce = GetComponent<AudioSource>();
		_beatNumber = -1000;
		if (LevelSelectPersistence.Instance != null && _getSongFromPersistence)
		{
			_song = LevelSelectPersistence.Instance.SelectedSong;
			if(_levelLayoutGameObject != null)
				DestroyImmediate(_levelLayoutGameObject);
			_levelLayoutGameObject = Instantiate(_song.LevelPrefab, Vector3.zero, Quaternion.identity);
		}
	}

	void Start()
	{
		_beatNumber = -1;
		_song.StartSong(_audioSoruce, this);
	}



	void Update()
	{
		if (_audioSoruce.isPlaying && _song.CheckBeat())
		{
			_beatNumber++;
			Beat?.Invoke(_beatNumber);
		}
	}

}
