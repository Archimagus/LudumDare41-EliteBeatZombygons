using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

public class Player : MonoBehaviour, INotifyPropertyChanged
{
	[SerializeField]private GameObject _shield;
	public float health = 100;

	private int _powerupDuration;
	public PowerupData PowerUp { get; private set; }
	public bool HasSuper { get; set; }
	public int ShieldDuration { get; set; }

	private AudioSource _source;
	[SerializeField] private AudioClip _pickupSound;

	private void Start()
	{
		SongManager.Instance.Beat += onBeat;
		_source = GetComponent<AudioSource>();
	}

	private void OnDisable()
	{
		SongManager.Instance.Beat -= onBeat;
	}

	private void onBeat(int beatNum)
	{
		if(beatNum >= SongManager.Instance.LastBeatIndex)
			GameManager.current.ShowGameOverPanel(true);

		ShieldDuration--;
		if(ShieldDuration <=0)
			_shield.SetActive(false);

		_powerupDuration--;
		if (_powerupDuration <= 0)
			PowerUp = null;
	}

	public void PickupPowerup(PowerupData powerup)
	{
		_source.PlayOneShot(_pickupSound);

		switch (powerup.PowerupType)
		{
			case PowerUpTypes.Burst:
			case PowerUpTypes.Laser:
			case PowerUpTypes.Spread:
				PowerUp = powerup;
				_powerupDuration = powerup.DurationInBeats;
				break;
			case PowerUpTypes.Super:
				HasSuper = true;
				break;
			case PowerUpTypes.Shield:
				_shield.SetActive(true);
				ShieldDuration = powerup.DurationInBeats;
				break;
			case PowerUpTypes.TempoUp:
				break;
			case PowerUpTypes.TempoDown:
				break;
		}
	}

	public void DamagePlayer(float amt)
	{
		health -= amt;

		if(health <= 0)
		{
			Death();
		}
	}

	private void Death()
	{
		GameManager.current.DisableEnemyPathfinding();
		gameObject.SetActive(false);
		GameManager.current.ShowGameOverPanel(false);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if(collision.CompareTag("Enemy"))
		{
			Enemy enemy = collision.gameObject.GetComponent<Enemy>();
			if(ShieldDuration <= 0)
				DamagePlayer(enemy.damageAmount);
			enemy.gameObject.SetActive(false);
		}
		else if (collision.CompareTag("PowerUp"))
		{
			PickupPowerup(collision.GetComponent<PowerUp>().Data);
			collision.gameObject.SetActive(false);
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	[NotifyPropertyChangedInvocator]
	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
