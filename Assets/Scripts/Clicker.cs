using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Clicker : MonoBehaviour
{
	[SerializeField] private AudioSource _audioSource;
	[SerializeField] private AudioClip _defaultAudioClip;


	private bool _started;
	private void Start()
	{
		_started = true;
		OnStartOrEnable();
	}

	private void OnEnable()
	{
		if (_started) OnStartOrEnable();
	}

	private void OnStartOrEnable()
	{
		SongManager.Instance.Beat += onBeat;
	}
	
	private void OnDisable()
	{
		SongManager.Instance.Beat -= onBeat;
	}

	private void onBeat(int obj)
	{
		_audioSource.Play();
	}

	private void OnValidate()
	{
		if (_audioSource == null)
			_audioSource = GetComponent<AudioSource>();
		if (_audioSource.clip == null)
			_audioSource.clip = _defaultAudioClip;
		_audioSource.priority = 0;
		_audioSource.playOnAwake = false;
		_audioSource.loop = false;
	}
}