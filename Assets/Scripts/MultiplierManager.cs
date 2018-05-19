using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

public class MultiplierManager : MonoBehaviour, INotifyPropertyChanged
{
	[SerializeField] private int _hitsToIncrease = 3;                               // Factor of hitstreak to increase multiplier
	[SerializeField] private int _hitsForCurrentMultiplier; // Number of consecutive hits to reach next multiplier
	[SerializeField] private int _maxMuliplier = 6;
	private bool _successfulHit;
	private int _hitStreak;
	private int _multiplier = 1;


	public int HitStreak
	{
		get { return _hitStreak; }
		private set
		{
			_hitStreak = value;
			if (PlayerScoreData.Instance != null)
				PlayerScoreData.Instance.CurrentStreak = HitStreak;
		}
	}

	public int Multiplier
	{
		get { return _multiplier; }
		private set
		{
			if (value == _multiplier) return;
			_multiplier = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(Progress));
		}
	}

	public float Progress
	{
		get { return Mathf.Clamp(Multiplier-1 + (float) _hitsForCurrentMultiplier / _hitsToIncrease, 0, 5); }
	}

	void Start()
	{
		SongManager.Instance.Beat += onBeat;
		Multiplier = Mathf.Clamp(Multiplier, 1, _maxMuliplier);
	}

	public void IncreaseHitStreak()
	{
		_successfulHit = true;
		HitStreak++;
		_hitsForCurrentMultiplier++;

		if (_hitsForCurrentMultiplier == _hitsToIncrease)
		{
			int next = Multiplier + 1;
			// Clamp multipler to highest sided polygon (currently hexagon?)
			Multiplier = Mathf.Clamp(next, 1, _maxMuliplier);
			_hitsForCurrentMultiplier = 0;
		}
	}

	// Increases the hit streak factor to increase multiplier
	// Call this as player progresses through level to increase difficulty
	public void IncreaseRequirement()
	{
		_hitsToIncrease++;
		_hitsForCurrentMultiplier = 0;
	}

	private void onBeat(int beatNum)
	{
		if (beatNum < SongManager.Instance.FirstBeatIndex || beatNum >= SongManager.Instance.LastBeatIndex) return;

		if (_successfulHit)
		{
			_successfulHit = false;
		}
		else
		{
			StartCoroutine(postBeatRoutine());
		}
	}

	public void resetMultiplier()
	{
		HitStreak = 0;
		_hitsForCurrentMultiplier = 0;
		Multiplier = 1;
	}

	IEnumerator postBeatRoutine()
	{
		yield return new WaitForSeconds((float) SongManager.Instance.BeatInputThreshold);
		if (_successfulHit == false)
		{
			// Missed beat - play SFX, screen effect?
			resetMultiplier();
		}

		_successfulHit = false;
	}

	public event PropertyChangedEventHandler PropertyChanged;

	[NotifyPropertyChangedInvocator]
	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
