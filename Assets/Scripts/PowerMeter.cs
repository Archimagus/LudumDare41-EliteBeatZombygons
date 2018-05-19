using UnityEngine;
using UnityEngine.UI;

public class PowerMeter : MonoBehaviour
{
	private MultiplierManager _multiplierManager;
	private Slider _slider;
	void Start()
	{
		_multiplierManager = GameManager.current.MultiplierManager;
		_slider = GetComponent<Slider>();
	}

	void Update()
	{
		_slider.value = _multiplierManager.Progress;
	}
}
