using UnityEngine;

public class BlipSpawner : MonoBehaviour
{
	[SerializeField] private BlipMovement _blipPrefab;
	[SerializeField] private int _numLiveBeats=4;
	[SerializeField] private RectTransform _blipParent;
	private ObjectPooler<BlipMovement> _blipPooler;

	// Use this for initialization
	void Start()
	{
		_blipPooler = new ObjectPooler<BlipMovement>(_blipPrefab);
		SongManager.Instance.Beat += OnBeat;
	}
	
	private void OnBeat(int beatNum)
	{
		var offsetBeat = beatNum - SongManager.Instance.FirstBeatIndex;
		if (offsetBeat >= -_numLiveBeats && beatNum <= SongManager.Instance.LastBeatIndex-_numLiveBeats)
		{
			var beat = _blipPooler.GetObject();
			beat.transform.SetParent(_blipParent, false);
			var rt = ((RectTransform) beat.transform);
			rt.anchoredPosition = new Vector3(_blipParent.rect.xMin, 0, 0);
			beat.LifeTime = (float) (SongManager.Instance.BeatTime * _numLiveBeats);
			beat.Speed = _blipParent.rect.width / beat.LifeTime;
		}
	}
}
