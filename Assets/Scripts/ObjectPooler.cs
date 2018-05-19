using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectPooler<T> where T:MonoBehaviour
{
	private readonly T _prefab;
	private readonly List<T> _objectPool = new List<T>();
	
	public ObjectPooler(T prefab)
	{
		_prefab = prefab;
	}
	
	public T GetObject()
	{
		var obj = _objectPool.FirstOrDefault(o => !o.gameObject.activeInHierarchy);
		if (obj == null)
		{
			obj = Object.Instantiate(_prefab);
			_objectPool.Add(obj);
		}
		obj.gameObject.SetActive(true);
		return obj;
	}
}
