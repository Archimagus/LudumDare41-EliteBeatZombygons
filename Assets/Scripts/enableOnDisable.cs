using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enableOnDisable : MonoBehaviour
{
	[SerializeField] private List<GameObject> _objectsToEnable;
	[SerializeField] private List<GameObject> _objectsToDisable;
	private void OnDisable()
	{
		foreach (var o in _objectsToDisable)
		{
			o.SetActive(false);
		}

		foreach (var o in _objectsToEnable)
		{
			o.SetActive(true);
		}
	}
}
