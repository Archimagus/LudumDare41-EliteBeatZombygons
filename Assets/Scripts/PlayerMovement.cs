using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	[SerializeField] float _moveAmount = 2;
	private Transform _player;
	private bool _movementKeyPressed;
	private Vector2 _movementVector;

	private RaycastHit2D hit;
	private float _distanceNeededToMove;
	private float _radius;
	
	public event System.Action Moved;
	public bool canMove = false;

	private AudioSource _source;
	[SerializeField] private AudioClip _miss;
	[SerializeField] private AudioClip _move;
	[SerializeField] private LayerMask _movementBlockingLayers;

	private static float _startingMoveAmount;

	private void Start()
	{
		_player = GameManager.current.Player.transform;
		_radius = GetComponent<CircleCollider2D>().radius;
		_distanceNeededToMove = _moveAmount;
		SongManager.Instance.Beat += OnBeat;
		_source = GetComponent<AudioSource>();

		_startingMoveAmount = _moveAmount;
	}

	void Update ()
	{
		if(canMove)
		{
			MovementDetection();
		}
	}

	private void MovementDetection()
	{

		if(Input.GetKeyDown(KeyCode.A))
		{
			hit = Physics2D.CircleCast(transform.position, _radius, -Vector2.right, _distanceNeededToMove , _movementBlockingLayers);

			_movementVector = new Vector2(_player.position.x - _moveAmount, _player.position.y);
			_movementKeyPressed = true;
		}
		else if(Input.GetKeyDown(KeyCode.D))
		{
			hit = Physics2D.CircleCast(transform.position, _radius, Vector2.right, _distanceNeededToMove, _movementBlockingLayers);

			_movementVector = new Vector2(_player.position.x + _moveAmount, _player.position.y);
			_movementKeyPressed = true;
		}
		else if (Input.GetKeyDown(KeyCode.S))
		{
			hit = Physics2D.CircleCast(transform.position, _radius, -Vector2.up, _distanceNeededToMove, _movementBlockingLayers);

			_movementVector = new Vector2(_player.position.x, _player.position.y - _moveAmount);
			_movementKeyPressed = true;
		}
		else if (Input.GetKeyDown(KeyCode.W))
		{
			hit = Physics2D.CircleCast(transform.position, _radius, Vector2.up, _distanceNeededToMove, _movementBlockingLayers);

			_movementVector = new Vector2(_player.position.x, _player.position.y + _moveAmount);
			_movementKeyPressed = true;
		}

		if (_movementKeyPressed)
		{
			if(SongManager.Instance.TimeFromBeat() < SongManager.Instance.BeatInputThreshold)
			{
				CheckForObstacles(hit);
				_source.PlayOneShot(_move);
				Move();
			}
			else
			{
				_source.PlayOneShot(_miss);
				GameManager.current.MultiplierManager.resetMultiplier();
				_movementKeyPressed = false;				
			}
		}
	}

	private void Move()
	{
		GameManager.current.MultiplierManager.IncreaseHitStreak();
		_player.position = Vector2.MoveTowards(_player.position, _movementVector, _moveAmount);
		_movementKeyPressed = false;

		if (_moveAmount != _startingMoveAmount)
		{
			_moveAmount = _startingMoveAmount;
		}

		Moved?.Invoke();
	}

	private void CheckForObstacles(RaycastHit2D hit)
	{
		if (hit.collider != null && !hit.collider.CompareTag("Enemy") && !hit.collider.CompareTag("PowerUp"))
		{
			if (hit.distance < _distanceNeededToMove)
			{
				_moveAmount = hit.distance - 0.1f;
				hit = new RaycastHit2D();
			}
		}
	}

	public void OnBeat(int beatNum)
	{
		if (beatNum >= SongManager.Instance.LastBeatIndex)
		{
			canMove = false;
		}
		else if (beatNum >= SongManager.Instance.FirstBeatIndex)
		{
			canMove = true;
		}
	}
}
