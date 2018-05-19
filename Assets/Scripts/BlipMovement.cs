using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlipMovement : MonoBehaviour
{
	public float Speed { get; set; }
	public float LifeTime { get; set; }

	private float _deathTime;
	private RectTransform _rt;
	private bool _started;

	protected virtual void Start()
	{
		_started = true;
		OnStartOrEnable();
	}

	protected virtual void OnEnable()
	{
		if (_started) OnStartOrEnable();
	}

	protected virtual void OnStartOrEnable()
	{
		_deathTime = Time.time + LifeTime;
		_rt = transform as RectTransform;

	}

	void Update()
	{
		_rt.anchoredPosition += Vector2.right * Time.deltaTime * Speed;
		if(Time.time > _deathTime)
			gameObject.SetActive(false);
	}
}
