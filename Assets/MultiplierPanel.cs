using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MultiplierPanel : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI _multiplierText;

	private TextMeshProUGUI[] _childRenderers;
	// Use this for initialization
	void Start()
	{
		_childRenderers = GetComponentsInChildren<TextMeshProUGUI>();

		foreach (var c in _childRenderers)
		{
			c.enabled = false;
		}

		GameManager.current.MultiplierManager.PropertyChanged += MultiplierManager_PropertyChanged;
	}

	private void MultiplierManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		var mult = GameManager.current.MultiplierManager.Multiplier;
		if (mult > 1)
		{
			foreach (var c in _childRenderers)
			{
				c.enabled = true;
			}
		}
		else
		{
			foreach (var c in _childRenderers)
			{
				c.enabled = false;
			}
		}

		_multiplierText.text = $"{mult}X";
	}
}
