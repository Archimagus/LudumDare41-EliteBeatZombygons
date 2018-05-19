using UnityEngine;


public class LevelSelectPersistence : MonoBehaviour
{
	public Song SelectedSong;

	public static LevelSelectPersistence Instance { get; private set; }
	private void Awake()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);
	}
}