using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
	private Camera _mainCamera;
	private Vector2 _screenPosition;
	private MultiplierManager _multiplierManager;

	private void Awake()
	{
		_mainCamera = Camera.main;
	}

	private void Start()
	{
		_multiplierManager = GameManager.current.MultiplierManager;
	}

	private void Update()
	{
		_screenPosition = _mainCamera.WorldToViewportPoint(transform.position);

		// Disables bullet when it goes offscreen without hitting anything
		if ((_screenPosition.x > 1 | _screenPosition.x < 0) || (_screenPosition.y > 1 | _screenPosition.y < 0))
		{
			DisableBullet();
		}
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.collider.CompareTag("Enemy"))
		{
			Enemy enemy = collision.gameObject.GetComponent<Enemy>();

			if (enemy != null)
			{
				enemy.ReduceHealth(1 * _multiplierManager.Multiplier);
			}

			DisableBullet();
		}
		else if (!collision.collider.CompareTag("Bullet"))
		{
			DisableBullet();
		}
	}

	private void DisableBullet()
	{
		//gameObject.transform.parent.gameObject.SetActive(false);
		gameObject.SetActive(false);
	}
}
