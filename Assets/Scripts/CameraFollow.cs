using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	private Transform _player;
	private Vector3 _offset;

	private void Start()
	{
		_player = GameManager.current.Player.transform;
		_offset = transform.position - _player.position;
	}

	void LateUpdate()
	{
		transform.position = _player.position + _offset;
	}
}
