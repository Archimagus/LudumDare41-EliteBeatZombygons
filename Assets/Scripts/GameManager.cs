using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public static GameManager current;

	public Settings SettingsPanel;
	public DeathPanel DeathPanel;
	public Enemy EnemyPrefab;
	public PowerUp PowerUpPrefab;

	public Player Player;
	public StartGameCountdown Countdown;
	public MultiplierManager MultiplierManager;
	public HashSet<Enemy> Enemies;
	public Player ThePlayer;
	public SpawnPoint []SpawnPoints;

	private ObjectPooler<Enemy> _enemyPooler;
	private ObjectPooler<PowerUp> _powerUpPool;
	private void Awake()
	{
		current = this;
		_enemyPooler = new ObjectPooler<Enemy>(EnemyPrefab);
		_powerUpPool = new ObjectPooler<PowerUp>(PowerUpPrefab);
		Player = FindObjectOfType<Player>();
		Countdown = FindObjectOfType<StartGameCountdown>();
		MultiplierManager = GetComponent<MultiplierManager>();
		Enemies = new HashSet<Enemy>();
		Enemies.Clear();
		ThePlayer = FindObjectOfType<Player>();
		SpawnPoints = FindObjectsOfType<SpawnPoint>();
	}

	public bool SpawnEnemy(ZombygonData data, string spawnPointName)
	{
		foreach (var spawnPoint in SpawnPoints)
		{
			if (spawnPoint.name == spawnPointName)
			{
				var enemy = _enemyPooler.GetObject();
				Enemies.Add(enemy);
				var offset = Random.insideUnitCircle * spawnPoint.Radius;
				var point = spawnPoint.transform.position + (Vector3)offset;
				if (Vector3.Distance(ThePlayer.transform.position, point) < 8)
					return false;
				enemy.transform.position = point;
				enemy.SetZombygonData(data);
				return true;
			}
		}

		Debug.LogWarning($"No spawn point named {spawnPointName} skipping spawning of object.");
		return true;
	}

	public void SpawnPowerup(Vector3 location)
	{
		var powerup = _powerUpPool.GetObject();
		powerup.transform.position = location;
	}

	public void LoadScene(string name)
	{
		SceneManager.LoadScene(name);
	}

	public void ShowSettings()
	{
		SettingsPanel.gameObject.SetActive(true);
	}

	public void QuitGame()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			ShowSettings();
		}
	}

	public void DisableEnemyPathfinding()
	{
		foreach (var enemy in Enemies)
		{
			enemy.EndPathfinding();
		}
	}

	public void ShowGameOverPanel(bool win)
	{
		DeathPanel.Show(win);
	}
}
