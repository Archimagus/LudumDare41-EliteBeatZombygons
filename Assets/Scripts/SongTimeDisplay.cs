using UnityEngine;
using UnityEngine.UI;

public class SongTimeDisplay : MonoBehaviour
{
	private Slider _slider;

	private void Start()
	{
		_slider = GetComponent<Slider>();
	}

	void Update()
	{
		_slider.value = 1-(float)(SongManager.Instance.PlayTime / SongManager.Instance.Length);
	}
}
