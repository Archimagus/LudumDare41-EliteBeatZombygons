using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class Shoot : MonoBehaviour
{
	private Player _player;
	private ObjectPooler<Bullet> _objectPooler;
	[SerializeField] private Bullet _bulletPrefab;
	[SerializeField] private Transform _projectileStartingPoint;
	[SerializeField] private LineRenderer _beamWeapon;
	[SerializeField] private ParticleSystem _superParticleSystem;
	[SerializeField] private float _superRadius=50;
	[SerializeField] private float _fireRate = 100;

	public bool canShoot = false;

	private AudioSource _source;
	[SerializeField] private PlayerSounds _playerSounds;

	private WaitForSeconds _tripletWait;

	void Start ()
	{
		_player = GameManager.current.Player;
		_objectPooler = new ObjectPooler<Bullet>(_bulletPrefab);
		SongManager.Instance.Beat += OnBeat;
		_tripletWait = new WaitForSeconds((float)SongManager.Instance.BeatTime/6);
		_source = GetComponent<AudioSource>();
	}

	void Update ()
	{
		if(canShoot)
		{
			MouseAim();

			// Fire projectile when Fire1 is pressed and occurred within the beat range
			// otherwise penalize player
			if (Input.GetButtonDown("Fire1"))
			{
				if (SongManager.Instance.TimeFromBeat() < SongManager.Instance.BeatInputThreshold)
				{
					Fire();
				}
				else
				{
					_source.PlayOneShot(_playerSounds.miss);
					GameManager.current.MultiplierManager.resetMultiplier();
				}
			}

			if (Input.GetButtonDown("Fire2"))
			{
				if (SongManager.Instance.TimeFromBeat() < SongManager.Instance.BeatInputThreshold)
				{
					DoSuper();
				}
				else
				{
					_source.PlayOneShot(_playerSounds.miss);
					GameManager.current.MultiplierManager.resetMultiplier();
				}
			}
		}		
	}

	private void MouseAim()
	{
		Vector3 diff = Camera.main.ScreenToWorldPoint(Input.mousePosition) - _player.transform.position;
		diff.Normalize();

		float rotZ = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
		_player.transform.rotation = Quaternion.Euler(0f, 0f, rotZ - 90);
	}

	private Collider2D[] _superResults = new Collider2D[200];
	private void DoSuper()
	{
		if (_player.HasSuper)
		{
			_source.PlayOneShot(_playerSounds.powerUpSounds.super);
			_player.HasSuper = false;
			var size = _superParticleSystem.sizeOverLifetime;
				size.size = new ParticleSystem.MinMaxCurve(_superRadius, AnimationCurve.Linear(0,0,1,1));
			_superParticleSystem.Play();

			GameManager.current.MultiplierManager.IncreaseHitStreak();
			ContactFilter2D filter = new ContactFilter2D();
			int count = Physics2D.OverlapCircle(transform.position, _superRadius, filter, _superResults);
			for (int i = 0; i < count; i++)
			{
				var r = _superResults[i];
				if (r.CompareTag("Enemy"))
				{
					var enemy = r.GetComponent<Enemy>();
					enemy.KillNoDrops();
				}
			}
		}
	}
	private void Fire()
	{
		GameManager.current.MultiplierManager.IncreaseHitStreak();

		if (_player.PowerUp != null)
		{
			switch (_player.PowerUp.PowerupType)
			{
				case PowerUpTypes.Burst:
					StartCoroutine(burstFire());
					break;
				case PowerUpTypes.Laser:
					FireLaser(transform.up);
					break;
				case PowerUpTypes.Spread:
					_source.PlayOneShot(_playerSounds.powerUpSounds.spread);
					Fire(Quaternion.Euler(0f, 0f, 5f)*transform.up);
					Fire(transform.up);
					Fire(Quaternion.Euler(0f, 0f, -5f) * transform.up);
					break;
				case PowerUpTypes.Super:
				case PowerUpTypes.Shield:
				case PowerUpTypes.TempoUp:
				case PowerUpTypes.TempoDown:
					// Do nothing here for these
					break;
			}
		}
		else
		{
			_source.PlayOneShot(_playerSounds.standardShot);
			Fire(transform.up);
		}
	}

	IEnumerator burstFire()
	{
		_source.PlayOneShot(_playerSounds.standardShot);
		Fire(transform.up);
		yield return _tripletWait;
		_source.PlayOneShot(_playerSounds.standardShot);
		Fire(transform.up);
		yield return _tripletWait;
		_source.PlayOneShot(_playerSounds.standardShot);
		Fire(transform.up);
	}

	public void Fire(Vector2 direction)
	{
		var bullet = _objectPooler.GetObject();
		bullet.transform.position = _projectileStartingPoint.position;
		bullet.gameObject.SetActive(true);

		Rigidbody2D rigid2D = bullet.GetComponent<Rigidbody2D>();
		rigid2D.AddForce(direction * _fireRate);
	}

	public void FireLaser(Vector2 direction)
	{
		_source.PlayOneShot(_playerSounds.powerUpSounds.laser);
		var hits = Physics2D.CircleCastAll(_projectileStartingPoint.position, 1f, direction);
		Vector2 endPoint = direction*1000;
		foreach (var hit in hits)
		{
			if (hit.collider.CompareTag("Obstacle"))
			{
				endPoint = hit.point;
				break;
			}
			if (hit.collider.CompareTag("Enemy"))
			{
				Enemy enemy = hit.collider.GetComponent<Enemy>();
				enemy.ReduceHealth(1 * GameManager.current.MultiplierManager.Multiplier);
			}
		}
		_beamWeapon.gameObject.SetActive(true);
		_beamWeapon.SetPosition(0, _projectileStartingPoint.position);
		_beamWeapon.SetPosition(1, endPoint);
		StartCoroutine(stopBeam());

	}

	IEnumerator stopBeam()
	{
		yield return new WaitForSeconds(0.1f);
		_beamWeapon.gameObject.SetActive(false);
	}

	public void OnBeat(int beatNum)
	{
		if (beatNum >= SongManager.Instance.LastBeatIndex)
		{
			canShoot = false;
		}
		else if (beatNum >= SongManager.Instance.FirstBeatIndex)
		{
			canShoot = true;
		}
	}

	[System.Serializable]
	public class PlayerSounds
	{
		public AudioClip standardShot;
		public PowerUpSounds powerUpSounds;
		public AudioClip miss;
	}

	[System.Serializable]
	public class PowerUpSounds
	{		
		public AudioClip laser;
		public AudioClip spread;
		public AudioClip super;
		public AudioClip shield;
	}
}
