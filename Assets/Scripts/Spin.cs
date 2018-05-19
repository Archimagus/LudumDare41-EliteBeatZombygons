using UnityEngine;

public class Spin : MonoBehaviour
{
	[SerializeField]private float _speed = 10;
	void Update()
	{
		transform.Rotate(0,0,_speed*Time.deltaTime);
	}
}
