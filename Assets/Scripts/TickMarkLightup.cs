using UnityEngine;
using UnityEngine.UI;

public class TickMarkLightup : MonoBehaviour
{
	[SerializeField] private int _minMultiplier;
	private Image _tickMark;
	private Color _originalColor;

	void Start()
	{
		_tickMark = GetComponent<Image>();
		_originalColor = _tickMark.color;
	}

	void Update()
	{
		if (GameManager.current.MultiplierManager.Multiplier >= _minMultiplier)
			_tickMark.color = Color.cyan;
		else
			_tickMark.color = _originalColor;
	}
}
