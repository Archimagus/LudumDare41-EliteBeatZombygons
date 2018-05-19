using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI _audioOffsetText;
	[SerializeField] private Slider _audioSlider;


	private int _audioOffset;

	public float AudioOffset
	{
		get
		{
			return Mathf.InverseLerp(-100, 100, _audioOffset);
		}
		set
		{
			_audioOffset = (int)Mathf.Lerp(-100, 100, value);
			_audioOffsetText.text = $"{_audioOffset}ms";
			SongManager.Instance.AudioOffsetMilliseconds = _audioOffset;
		}
	}
	
	private void OnEnable()
	{
		_audioOffset = PlayerPrefs.GetInt("_audioOffset", 0);

		_audioOffsetText.text = $"{_audioOffset}ms";
		_audioSlider.value = AudioOffset;
	}

	public void ResetValues()
	{
		_audioOffset = 0;
		_audioSlider.value = AudioOffset;
	}

	public void Confirm()
	{
		PlayerPrefs.SetString("DidSetupScreen", "Y");
		PlayerPrefs.SetInt("_audioOffset", _audioOffset);
	}
}
