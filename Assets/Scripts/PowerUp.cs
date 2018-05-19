using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PowerUp : MonoBehaviour
{
	[SerializeField] private PowerupData[] _possiblePowerups;

	public PowerupData Data { get; set; }
	private void OnEnable()
	{
		Data = _possiblePowerups[Random.Range(0, _possiblePowerups.Length)];

		GetComponent<SpriteRenderer>().sprite = Data.Icon;
	}
}
[System.Serializable]
public class PowerupData
{
	public Sprite Icon;
	public PowerUpTypes PowerupType;
	public int DurationInBeats;
}
public enum PowerUpTypes
{
	Burst,
	Laser,
	Spread,
	Super,
	Shield,
	TempoUp,
	TempoDown
}