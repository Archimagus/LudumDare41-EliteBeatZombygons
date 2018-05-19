using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class Enemy : MonoBehaviour
{
	[SerializeField] private List<ZombygonData> _zombygonDatas;
	[SerializeField] private ZombygonData _activeZombygon;
	[SerializeField] private ParticleSystem _deathParticleSystem;
	private AIPath _aiPath;

	[SerializeField] private int _health;
	private SpriteRenderer _renderer;

	private Player _player;

	private bool _started;
	private float _chanceToDropLoot;
	public float damageAmount = 100;
	private bool _dataSet;

	private ObjectPooler<Spin> _particlePooler;
	private AudioSource _source;
	[SerializeField] private EnemySounds _enemySounds;

	void Start ()
	{
		_started = true;
		_particlePooler = new ObjectPooler<Spin>(_deathParticleSystem.GetComponent<Spin>());
		_player = GameManager.current.Player;

		_aiPath = GetComponent<AIPath>();

		if(_aiPath == null)
		{
			Debug.LogError(gameObject.name = " is missing required AIPath script. Please add and try again.");
			gameObject.SetActive(false);
		}

		var setter = GetComponent<AIDestinationSetter>();
		setter.target = _player.transform;
		if(!_dataSet)
			SetZombygonData(_activeZombygon);

		_source = GetComponent<AudioSource>();

		OnStartOrEnable();
	}


	protected virtual void OnEnable()
	{
		if (_started) OnStartOrEnable();
	}

	protected virtual void OnStartOrEnable()
	{
		SubscribeToEvents();
		_aiPath.pickNextWaypointDist = _activeZombygon.MovementRate * 2;
		_aiPath.SearchPath();
	}

	public void SetZombygonData(ZombygonData zombygon)
	{
		_dataSet = true;
		_activeZombygon = zombygon;
		if(_aiPath != null)
			_aiPath.pickNextWaypointDist = _activeZombygon.MovementRate * 2;

		_health = _activeZombygon.Sides;
		_renderer = GetComponent<SpriteRenderer>();
		_renderer.sprite = _activeZombygon.ZombygonSprite;
		_chanceToDropLoot = _activeZombygon.ChanceToDropLoot;
	}

	private void onBeat(int beatNum)
	{

		if (beatNum < SongManager.Instance.FirstBeatIndex || beatNum >= SongManager.Instance.LastBeatIndex) return;

		float random = Random.Range(0.0f, 1.0f);

		if (random <= _activeZombygon.ChanceToMove)
		{
			if(_aiPath.isActiveAndEnabled)
			{
				if (_aiPath.hasPath)
				{
					_aiPath.Teleport(_aiPath.GetPointAlongPath(transform.position, _activeZombygon.MovementRate));
					//_source.PlayOneShot(_enemySounds.move);
				}
				else if(!_aiPath.pathPending)
				{
					_aiPath.SearchPath();
				}
			}
		}
	}

	public bool ReduceHealth(int amt, bool canDrop = true)
	{
		_health -= amt;

		if(_health < 1)
		{

			if (PlayerScoreData.Instance != null)
			{
				PlayerScoreData.Instance.Score += _activeZombygon.Score * GameManager.current.MultiplierManager.Multiplier;
				PlayerScoreData.Instance.EnemiesKilled++;
			}

			if (_activeZombygon.ZombygonType == ZombygonTypes.Triangle || _zombygonDatas.Count == 0)
			{
				var part = _particlePooler.GetObject().GetComponent<ParticleSystem>();
				part.transform.position = transform.position;
				part.Play();
				_source.PlayOneShot(_enemySounds.death);
				UnsubscribeFromEvents();
				gameObject.SetActive(false);
				if (canDrop)
					DropLoot();
				return false;
			}
			else
			{
				_deathParticleSystem.gameObject.SetActive(true);
				_deathParticleSystem.Emit(1);
				NextZombygon();
				return true;
			}
		}

		return true;
	}


	private void NextZombygon()
	{
		if(_zombygonDatas.Count > 0)
		{
			_activeZombygon = _zombygonDatas[_activeZombygon.Sides - 4];

			_renderer.sprite = _activeZombygon.ZombygonSprite;
			_health = _activeZombygon.Sides;
			_source.PlayOneShot(_enemySounds.damaged);
		}		
	}

	private void Player_Moved()
	{
		if (!_aiPath.pathPending)
		{
			_aiPath.SearchPath();
		}
	}

	private void SubscribeToEvents()
	{
		SongManager.Instance.Beat += onBeat;
		_player.GetComponent<PlayerMovement>().Moved += Player_Moved;
	}

	private void UnsubscribeFromEvents()
	{
		SongManager.Instance.Beat -= onBeat;
		_player.GetComponent<PlayerMovement>().Moved -= Player_Moved;
	}

	public void EndPathfinding()
	{
		_aiPath.enabled = false;
	}

	private void DropLoot()
	{
		float drop = Random.value;
		if (drop < _chanceToDropLoot)
		{
			GameManager.current.SpawnPowerup(transform.position);
		}
	}
	public void KillNoDrops()
	{
		while (ReduceHealth(6, false))
		{
			// killing the enemy
		}
	}

	[System.Serializable]
	public class EnemySounds
	{
		public AudioClip damaged;
		public AudioClip death;
		public AudioClip move;
	}
}
