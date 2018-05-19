using UnityEditor;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
	public float Radius=1;

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		Handles.color = Color.yellow;
		Handles.DrawWireDisc(transform.position, Vector3.forward, Radius);
	}
#endif
}
