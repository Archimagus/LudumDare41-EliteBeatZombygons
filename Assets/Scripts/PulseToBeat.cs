using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PulseToBeat : MonoBehaviour
{
	[SerializeField]private RuntimeAnimatorController _defaultController;
	private Animator _animator;

	private void Start()
	{
		SongManager.Instance.Beat += PulseToBeat_Beat;
		_animator = GetComponent<Animator>();
	}

	private void OnValidate()
	{
		if (GetComponent<Animator>().runtimeAnimatorController == null)
			GetComponent<Animator>().runtimeAnimatorController = _defaultController;
	}

	private void OnDestroy()
	{
		SongManager.Instance.Beat -= PulseToBeat_Beat;
	}
	
	private void PulseToBeat_Beat(int obj)
	{
		if (gameObject.activeInHierarchy)
		{
			_animator.SetTrigger("Beat");
		}
	}
}
