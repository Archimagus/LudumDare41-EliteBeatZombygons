using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ZombygonData : ScriptableObject
{
	[SerializeField] private ZombygonTypes _zombygonType;
	[SerializeField] private Sprite _zombygonSprite;
	[SerializeField] [Range(1, 6)] private int _sides;
	[SerializeField] [Range(1, 6)] private int _movementRate;
	[SerializeField] [Range(0.0f, 1.0f)] private float _chanceToMove;
	[SerializeField] int _health = 100;
	[SerializeField] int _damage = 100;
	[SerializeField] int _score = 100;
	[SerializeField] [Range(0f, 1f)] private float _chanceToDropLoot;

	public ZombygonTypes ZombygonType { get { return _zombygonType; } }
	public Sprite ZombygonSprite { get { return _zombygonSprite; } }
	public int Sides { get { return _sides; } }
	public int MovementRate { get { return _movementRate; } }
	public float ChanceToMove { get { return _chanceToMove; } }
	public int Health { get { return _health; } }
	public int Damage { get { return _damage; } }
	public int Score { get { return _score; } }
	public float ChanceToDropLoot { get { return _chanceToDropLoot; } }

}
