using UnityEngine;
using UnityEngine.UI;

public class PowerupPanel : MonoBehaviour
{
	[SerializeField] private Image _powerupImage;
	[SerializeField] private Image _supeImage;

	private Player _player;
	void Start()
	{
		_player = GameManager.current.Player;
	}

	void Update()
	{
		_supeImage.enabled = _player.HasSuper;
		if (_player.PowerUp != null)
		{
			_powerupImage.sprite = _player.PowerUp.Icon;
			_powerupImage.enabled = true;
		}
		else
		{
			_powerupImage.enabled = false;
		}
	}
}
