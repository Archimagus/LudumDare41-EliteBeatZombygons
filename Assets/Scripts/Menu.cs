using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class Menu : MonoBehaviour
{
	[SerializeField] private MenuPanels menuPanels;
	[SerializeField] private Audio audioSettings;
	[SerializeField] private Video videoSettings;
	[SerializeField] private Button startButton;
	[SerializeField] private Button exitGameButton;
	[SerializeField] private List<Levels> levels;
	[SerializeField] private Sprite _blankStar;

	private GameObject _currentPanel;
	private GameObject _previousPanel;

	[SerializeField] private LevelElement _selectedLevelElement;
	private string _levelToLoad;

	private UnityEngine.Resolution[] supportedResolutions;
	public List<ResolutionOptions> resolutionOptionsList;
	private int _currentResolutionIndex = 0;

	[SerializeField] private AudioMixer _mixer;
	private float _masterVolume;
	private float _musicVolume;
	private float _sfxVolume;

	[SerializeField] private AudioSource _musicSource;
	[SerializeField] private AudioSource _previewSource;
	private AudioSource _sfxSource;

	public UISounds uiSounds;
	private bool _isPreviewing = false;
	public GameplaySettings gameplaySettings;
	private int _currentDifficultyIndex = 0;

	public float MasterVolume
	{
		get
		{
			return _masterVolume;
		}

		set
		{
			if (Mathf.Abs(_masterVolume - value) > 0.01f)
				_mixer.SetFloat("MasterVolume", LinearToDecibel(value));
			_masterVolume = value;
		}
	}

	public float MusicVolume
	{
		get
		{
			return _musicVolume;
		}

		set
		{
			if (Mathf.Abs(_musicVolume - value) > 0.01f)
				_mixer.SetFloat("MusicVolume", LinearToDecibel(value));
			_musicVolume = value;
		}
	}

	public float SfxVolume
	{
		get
		{
			return _sfxVolume;
		}

		set
		{
			if (Mathf.Abs(_sfxVolume - value) > 0.01f)
			{
				_mixer.SetFloat("SfxVolume", LinearToDecibel(value));
				if (menuPanels.audioPanel.activeInHierarchy)
					_sfxSource.PlayOneShot(uiSounds.hover);
			}
			_sfxVolume = value;
		}
	}

	private static float LinearToDecibel(float lin)
	{
		if (lin <= float.Epsilon)
			return -80;
		return Mathf.Log(lin, 3) * 20;
	}

	private static float DecibelToLinear(float db)
	{
		return Mathf.Pow(3, db / 20);
	}

	private void Awake()
	{
		_sfxSource = GetComponent<AudioSource>();
		if(Application.platform == RuntimePlatform.WebGLPlayer || Application.isMobilePlatform)
		exitGameButton.gameObject.SetActive(false);
	}

	void Start()
	{
		_currentPanel = menuPanels.mainPanel;
		resolutionOptionsList = new List<ResolutionOptions>();

		CreateSupportedResolutionOptions();
		LoadStuff();

		if(PlayerScoreData.Instance != null)
		{
			if(PlayerScoreData.Instance.FromScoreScreen)
			{
				menuPanels.mainPanel.SetActive(false);
				SetupLevelScreen();
				_currentPanel = menuPanels.levelSelectPanel;
				_currentPanel.SetActive(true);
			}
			else if(PlayerScoreData.Instance.FromCalibrationScreen)
			{
				menuPanels.mainPanel.SetActive(false);
				_currentPanel = menuPanels.optionsPanel;
				_currentPanel.SetActive(true);
				PlayerScoreData.Instance.FromCalibrationScreen = false;
			}
		}

		audioSettings.master.onValueChanged.AddListener(delegate { MasterChangeCheck(); });
		audioSettings.music.onValueChanged.AddListener(delegate { MusicChangeCheck(); });
		audioSettings.soundFx.onValueChanged.AddListener(delegate { SoundFxChangeCheck(); });
	}

	private void Update()
	{
		if(menuPanels.levelSelectPanel.activeInHierarchy)
		{
			if(_isPreviewing)
			{
				if(_previewSource.isPlaying && _previewSource.time > 10)
				{
					StopPreview();
				}
			}
		}
	}

	public void StartLevel()
	{
		_sfxSource.PlayOneShot(uiSounds.play);
		_currentPanel.SetActive(false);
		StartCoroutine(Play());
	}

	public void ExitGame()
	{
		_sfxSource.PlayOneShot(uiSounds.exit);
		_currentPanel.SetActive(false);
		StartCoroutine(Exit());
	}

	public void ToMainPanel()
	{
		_sfxSource.PlayOneShot(uiSounds.clickSounds[0]);

		_previousPanel = _currentPanel;
		_previousPanel.SetActive(false);

		if(_previousPanel == menuPanels.levelSelectPanel)
		{
			if (_selectedLevelElement != null)
			{
				DeselectLevel();
			}
		}

		_currentPanel = menuPanels.mainPanel;
		_currentPanel.SetActive(true);
	}

	public void ToCalibration()
	{
		if(PlayerScoreData.Instance != null)
		{
			PlayerScoreData.Instance.FromCalibrationScreen = true;
		}

		SceneManager.LoadScene("Calibration");
	}

	public void BackToPanel(GameObject nextPanel)
	{
		_sfxSource.PlayOneShot(uiSounds.clickSounds[0]);

		_previousPanel = _currentPanel;
		_previousPanel.SetActive(false);

		_currentPanel = nextPanel;
		_currentPanel.SetActive(true);

		if (_currentPanel == menuPanels.levelSelectPanel)
		{
			SetupLevelScreen();
		}
		else if (_previousPanel == menuPanels.audioPanel)
		{
			SaveAudio();
		}
	}

	public void ToPanel(GameObject nextPanel)
	{
		_sfxSource.PlayOneShot(uiSounds.clickSounds[1]);

		_previousPanel = _currentPanel;
		_previousPanel.SetActive(false);

		_currentPanel = nextPanel;
		_currentPanel.SetActive(true);

		if (_currentPanel == menuPanels.levelSelectPanel)
		{
			SetupLevelScreen();
		}
		else if (_previousPanel == menuPanels.audioPanel)
		{
			SaveAudio();
		}
	}

	// Renders level select screen elements based on data collected
	// when scene started.
	private void SetupLevelScreen()
	{
		for(int i = 0; i < levels.Count; i++)
		{
			LevelElement element = levels[i].levelElement;

			if (levels[i].levelUnlocked)
			{
				UnlockAndSetupLevelElement(element);
			}
			else
			{
				LockLevelElement(element);
			}
		}
	}

	// Unlocks a level element and renders appropriately.
	private void UnlockAndSetupLevelElement(LevelElement element)
	{
		element.levelSelectButton.interactable = true;
		if (element.lockedImage != null)
		{
			element.lockedImage.gameObject.SetActive(false);
		}

		int rating = GetRating(element);
		for (int j = 0; j < rating; j++)
		{
			element.ratingImages[j].sprite = element.filledStar;
		}

		element.ratingPanel.SetActive(true);
	}

	// Locks a level element and renders appropriately.
	private void LockLevelElement(LevelElement element)
	{
		element.levelSelectButton.interactable = false;
		element.ratingPanel.SetActive(false);
		element.lockedImage.gameObject.SetActive(true);
	}

	public void SelectLevel(LevelElement element)
	{
		if(element != _selectedLevelElement)
		{
			if (_selectedLevelElement != null)
			{
				DeselectLevel();
			}

			EventSystem.current.SetSelectedGameObject(element.gameObject);
			_sfxSource.PlayOneShot(uiSounds.clickSounds[1]);
			
			_selectedLevelElement = element;
			_selectedLevelElement.songName.text = GetLevelLabel();
			Song song = GetSongAsset();
			LevelSelectPersistence.Instance.SelectedSong = song;
			_selectedLevelElement.songName.enabled = true;

			if (!startButton.interactable)
			{
				startButton.interactable = true;
			}

			PreviewSong(song);
		}
	}

	// Deselect selected level element and reset level select UI.
	private void DeselectLevel()
	{
		if(_isPreviewing)
		{
			StopPreview();
		}

		EventSystem.current.SetSelectedGameObject(null);

		if(_selectedLevelElement != null)
		{
			_selectedLevelElement.songName.enabled = false;
			_selectedLevelElement = null;
		}
		
		startButton.interactable = false;
	}

	// Preview song for selected level element.
	// If song is actively being previewed, stop it and start preview for new selected level element.
	private void PreviewSong(Song song)
	{
		if(_isPreviewing)
		{
			_previewSource.Stop();
		}
		else
		{
			_musicSource.Pause();
			_previewSource.clip = song.MusicClip;
			_previewSource.Play();
		}

		_isPreviewing = true;
	}

	// Stop the preview for selected level element and resume menu music.
	private void StopPreview()
	{
		_previewSource.Stop();
		_musicSource.Play();

		_isPreviewing = false;
	}

	private void CreateSupportedResolutionOptions()
	{
		supportedResolutions = Screen.resolutions;
		for (int i = 0; i < supportedResolutions.Length; i++)
		{
			if (supportedResolutions[i].refreshRate >= 29)
			{
				ResolutionOptions resolutionOption = new ResolutionOptions();
				resolutionOption.width = supportedResolutions[i].width;
				resolutionOption.height = supportedResolutions[i].height;
				resolutionOption.refreshRate = supportedResolutions[i].refreshRate;

				resolutionOptionsList.Add(resolutionOption);
			}
		}
	}

	private void LoadStuff()
	{
		MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
		MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
		SfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);

		audioSettings.master.value = MasterVolume;
		audioSettings.music.value = MusicVolume;
		audioSettings.soundFx.value = SfxVolume;

		_currentResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", resolutionOptionsList.Count-1);

		videoSettings.currentResolution.text = resolutionOptionsList[_currentResolutionIndex].width + " x " +
					resolutionOptionsList[_currentResolutionIndex].height + " " + resolutionOptionsList[_currentResolutionIndex].refreshRate + "Hz";

		for(int i = 0; i < levels.Count; i++)
		{
			string key2 = levels[i].song.MusicClip.name + "Rating";

			levels[i].rating = PlayerPrefs.GetInt(key2, 0);

			// Skips tutorial since tutorial is always unlocked
			if(i > 0)
			{
				string key = levels[i-1].song.MusicClip.name + "Played";

				if (PlayerPrefs.GetString(key, "N") == "Y")
				{
					levels[i].levelUnlocked = true;
				}
				else
				{
					levels[i].levelUnlocked = false;
				}

				/*
				if (levels[i - 1].rating > 0)
				{
					levels[i].levelUnlocked = true;
				}
				else
				{
					levels[i].levelUnlocked = false;
				}
				*/
			}		
		}

		// Difficulty (0 = easy, 1 = normal, 2 = hard)
		_currentDifficultyIndex = PlayerPrefs.GetInt("Difficulty", 1);
		gameplaySettings.difficultyUI.text = gameplaySettings.difficulty[_currentDifficultyIndex].uiText;
	}

	private void SaveAudio()
	{
		PlayerPrefs.SetFloat("MasterVolume", MasterVolume);
		PlayerPrefs.SetFloat("MusicVolume", MusicVolume);
		PlayerPrefs.SetFloat("SFXVolume", SfxVolume);
	}

	public void ChangeResolution(int direction)
	{
		if (direction > 0)
		{
			_sfxSource.PlayOneShot(uiSounds.clickSounds[1]);
			if (_currentResolutionIndex < resolutionOptionsList.Count - 1)
			{
				_currentResolutionIndex++;
			}
			else
			{
				_currentResolutionIndex = 0;
			}
		}
		else
		{
			_sfxSource.PlayOneShot(uiSounds.clickSounds[0]);
			if (_currentResolutionIndex > 0)
			{
				_currentResolutionIndex--;
			}
			else
			{
				_currentResolutionIndex = resolutionOptionsList.Count - 1;
			}
		}

		videoSettings.currentResolution.text = resolutionOptionsList[_currentResolutionIndex].width + " x " + 
			resolutionOptionsList[_currentResolutionIndex].height + " " + resolutionOptionsList[_currentResolutionIndex].refreshRate + "Hz";

		PlayerPrefs.SetInt("ResolutionIndex", _currentResolutionIndex);
		Screen.SetResolution(resolutionOptionsList[_currentResolutionIndex].width, resolutionOptionsList[_currentResolutionIndex].width, true);
		EventSystem.current.SetSelectedGameObject(null);
	}

	public void ChangeDifficulty(int direction)
	{
		if (direction > 0)
		{
			_sfxSource.PlayOneShot(uiSounds.clickSounds[1]);
			if (_currentDifficultyIndex < gameplaySettings.difficulty.Count - 1)
			{
				_currentDifficultyIndex++;
			}
			else
			{
				_currentDifficultyIndex = 0;
			}
		}
		else
		{
			_sfxSource.PlayOneShot(uiSounds.clickSounds[0]);
			if (_currentDifficultyIndex > 0)
			{
				_currentDifficultyIndex--;
			}
			else
			{
				_currentDifficultyIndex = gameplaySettings.difficulty.Count - 1;
			}
		}

		gameplaySettings.difficultyUI.text = gameplaySettings.difficulty[_currentDifficultyIndex].uiText;
		PlayerPrefs.SetInt("Difficulty", _currentDifficultyIndex);
		EventSystem.current.SetSelectedGameObject(null);
	}

	public void PointerUp()
	{
		EventSystem.current.SetSelectedGameObject(null);
		_sfxSource.PlayOneShot(uiSounds.clickSounds[1]);
	}

	private string GetLevelLabel()
	{
		string label = "";

		for (int i = 0; i < levels.Count; i++)
		{
			if (levels[i].levelElement == _selectedLevelElement)
			{
				label = levels[i].levelName;
			}
		}

		return label;
	}

	private string GetSongName()
	{
		string songName = "";

		for(int i = 0; i < levels.Count; i++)
		{
			if(levels[i].levelElement == _selectedLevelElement)
			{
				songName = levels[i].song.MusicClip.name;
			}
		}

		return songName;
	}

	private Song GetSongAsset()
	{
		// Defaults to tutorial for initialization
		Song song = levels[0].song;

		for (int i = 0; i < levels.Count; i++)
		{
			if (levels[i].levelElement == _selectedLevelElement)
			{
				song = levels[i].song;
			}
		}

		return song;
	}

	private int GetRating(LevelElement element)
	{
		int rating = 0;

		for (int i = 0; i < levels.Count; i++)
		{
			if (levels[i].levelElement == element)
			{
				rating = levels[i].rating;
			}
		}

		return rating;
	}
	
	public void ShowConfirmation()
	{
		_sfxSource.PlayOneShot(uiSounds.clickSounds[1]);
		menuPanels.confirmationPanel.SetActive(true);
		DeselectLevel();
	}

	public void CancelConfirmation()
	{
		_sfxSource.PlayOneShot(uiSounds.clickSounds[0]);
		menuPanels.confirmationPanel.SetActive(false);
	}

	public void ApplyConfirmation()
	{
		_sfxSource.PlayOneShot(uiSounds.clickSounds[1]);
		ResetLevelData();
	}

	private void ResetLevelData()
	{
		menuPanels.confirmationPanel.SetActive(false);
		PlayerPrefs.SetString("DidSetupScreen", "N");
		for (int i = 0; i < levels.Count; i++)
		{
			string key = levels[i].song.MusicClip.name + "Played";
			string key2 = levels[i].song.MusicClip.name + "Rating";

			PlayerPrefs.SetString(key, "N");
			PlayerPrefs.SetInt(key2, 0);

			ResetStars(levels[i].levelElement);
		}

		LoadStuff();
		SetupLevelScreen();

		EventSystem.current.SetSelectedGameObject(null);
	}

	private void ResetStars(LevelElement element)
	{
		for(int i = 0; i < element.ratingImages.Length; i++)
		{
			element.ratingImages[i].sprite = _blankStar;
		}
	}

	private void MasterChangeCheck()
	{
		MasterVolume = audioSettings.master.value;	
	}

	private void MusicChangeCheck()
	{
		MusicVolume = audioSettings.music.value;
	}

	private void SoundFxChangeCheck()
	{
		SfxVolume = audioSettings.soundFx.value;
	}

	IEnumerator Play()
	{
		PlayerPrefs.SetString("SongName", GetSongName());

		yield return new WaitForSeconds(1);

		if (PlayerPrefs.GetString("DidSetupScreen", "N") == "N")
		{
			SceneManager.LoadScene("SetupScene");
		}
		else
		{
			SceneManager.LoadScene("Game");
		}
	}

	IEnumerator Exit()
	{
		yield return new WaitForSeconds(1);
		Application.Quit();
	}

	/**
	 * All of the panels in menu
	 **/
	[System.Serializable]
	public class MenuPanels
	{
		public GameObject mainPanel;
		public GameObject levelSelectPanel;
		public GameObject optionsPanel;
		public GameObject audioPanel;
		public GameObject videoPanel;
		public GameObject controlPanel;
		public GameObject gameplayPanel;
		public GameObject confirmationPanel;
	}

	/**
	 * Holds data for each of the songs for the game as well as player's
	 * rating for that level
	 **/ 
	[System.Serializable]
	public class Levels
	{
		public string levelName;
		public LevelElement levelElement;
		public bool levelUnlocked;
		public Song song;
		public int rating;
	}

	/**
	 * Audio settings
	 **/
	[System.Serializable]
	public class Audio
	{
		public Slider master;
		public Slider music;
		public Slider soundFx;
	}

	/**
	 * Video settings
	 **/
	[System.Serializable]
	public class Video
	{
		public TextMeshProUGUI currentResolution;
	}

	/**
	 * Resolution data
	 **/
	[System.Serializable]
	public class ResolutionOptions
	{
		public int width;
		public int height;
		public int refreshRate;
	}

	[System.Serializable]
	public class UISounds
	{
		public AudioClip play;
		public AudioClip exit;
		public AudioClip hover;
		public List<AudioClip> clickSounds;
	}

	[System.Serializable]
	public class GameplaySettings
	{
		public TextMeshProUGUI difficultyUI;
		public List<Difficulty> difficulty;
	}

	[System.Serializable]
	public class Difficulty
	{
		public string uiText;
	}
}
