using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsMenuPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject controlsPanel;
    
    [Header("Settings Components")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Dropdown qualityDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Dropdown resolutionDropdown;
    [SerializeField] private Slider sensitivitySlider;
    
    private Resolution[] resolutions;
    
    private void Start()
    {
        // Make sure only main menu is showing at start
        ShowMainMenu();
        
        // Initialize settings
        InitializeSettings();
        
        // Set up cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    
    private void InitializeSettings()
    {
        // Initialize quality settings
        qualityDropdown.ClearOptions();
        List<string> qualityOptions = new List<string>(QualitySettings.names);
        qualityDropdown.AddOptions(qualityOptions);
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.RefreshShownValue();

        // Initialize fullscreen setting
        fullscreenToggle.isOn = Screen.fullScreen;

        // Initialize resolution settings
        InitializeResolutions();

        // Load and apply saved volume settings
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        musicVolumeSlider.value = musicVolume;
        sfxVolumeSlider.value = sfxVolume;

        // Load sensitivity setting
        float sensitivity = PlayerPrefs.GetFloat("Sensitivity", 1.0f);
        sensitivitySlider.value = sensitivity;
    }
    
    private void InitializeResolutions()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRate + "Hz";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height &&
                resolutions[i].refreshRate == Screen.currentResolution.refreshRate)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }
    
    // Navigation methods
    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        settingsMenuPanel.SetActive(false);
        creditsPanel.SetActive(false);
        controlsPanel.SetActive(false);
    }
    
    public void ShowSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsMenuPanel.SetActive(true);
        creditsPanel.SetActive(false);
        controlsPanel.SetActive(false);
    }
    
    public void ShowCredits()
    {
        mainMenuPanel.SetActive(false);
        settingsMenuPanel.SetActive(false);
        creditsPanel.SetActive(true);
        controlsPanel.SetActive(false);
    }
    
    public void ShowControls()
    {
        mainMenuPanel.SetActive(false);
        settingsMenuPanel.SetActive(false);
        creditsPanel.SetActive(false);
        controlsPanel.SetActive(true);
    }
    
    // Game actions
    public void StartGame()
    {
        // Load the first level
        SceneManager.LoadScene("Level1"); // Change to your first level scene name
    }
    
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    // Settings methods
    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen, resolution.refreshRate);
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("QualityLevel", qualityIndex);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    public void SetMusicVolume(float volume)
    {
        // Implement audio mixer control for music volume
        PlayerPrefs.SetFloat("MusicVolume", volume);
        
        // If you have an audio mixer:
        // audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
    }

    public void SetSFXVolume(float volume)
    {
        // Implement audio mixer control for SFX volume
        PlayerPrefs.SetFloat("SFXVolume", volume);
        
        // If you have an audio mixer:
        // audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
    }

    public void SetSensitivity(float sensitivity)
    {
        // Save sensitivity setting
        PlayerPrefs.SetFloat("Sensitivity", sensitivity);
    }
} 