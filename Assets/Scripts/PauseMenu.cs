using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PauseMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject pauseMenuUI; // Основная панель меню паузы
    [SerializeField] private GameObject menuText; // Текст "menu"
    [SerializeField] private GameObject menuBackground; // Фон текста "menu"
    [SerializeField] private GameObject pauseButtonsContainer; // Контейнер для всех кнопок меню паузы
    [SerializeField] private GameObject settingsMenuUI;
    [SerializeField] private GameObject controlsMenuUI;
    [SerializeField] private GameObject graphicsMenuUI;
    [SerializeField] private GameObject audioMenuUI;

    [Header("Settings Components")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Dropdown qualityDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Dropdown resolutionDropdown;
    [SerializeField] private Slider sensitivitySlider;

    private Resolution[] resolutions;
    private bool isPaused = false;
    private float defaultTimeScale = 1.0f;

    private void Start()
    {
        // Ensure menu is closed on start
        pauseMenuUI.SetActive(false);
        
        // Ensure menu text and background are hidden on start
        if (menuText != null) menuText.SetActive(false);
        if (menuBackground != null) menuBackground.SetActive(false);
        if (pauseButtonsContainer != null) pauseButtonsContainer.SetActive(false);
        
        CloseAllSubmenus();
        
        // Initialize settings
        InitializeSettings();
        
        // Debug log to verify initialization
        Debug.Log("PauseMenu initialized. UI elements deactivated.");
    }

    void Update()
    {
        // Toggle pause menu with ESC key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
                Debug.Log("Game resumed");
            }
            else
            {
                Pause();
                Debug.Log("Game paused");
            }
        }
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

        // Initialize resolutions
        InitializeResolutions();

        // Initialize audio settings
        // Assume we have PlayerPrefs for these values or default values
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        musicVolumeSlider.value = musicVolume;
        sfxVolumeSlider.value = sfxVolume;

        // Initialize sensitivity
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

    public void Resume()
    {
        // Hide pause menu
        pauseMenuUI.SetActive(false);
        
        // Hide menu text and background
        if (menuText != null) menuText.SetActive(false);
        if (menuBackground != null) menuBackground.SetActive(false);
        if (pauseButtonsContainer != null) pauseButtonsContainer.SetActive(false);
        
        CloseAllSubmenus();
        
        // Unpause the game
        Time.timeScale = defaultTimeScale;
        isPaused = false;
        
        // Re-enable player controls
        EnablePlayerControls(true);
        
        // Show cursor and lock it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Debug.Log("Resume called. UI elements hidden.");
    }

    void Pause()
    {
        // Show pause menu
        pauseMenuUI.SetActive(true);
        
        // Show menu text and background
        if (menuText != null) 
        {
            menuText.SetActive(true);
            Debug.Log("Menu text activated");
        }
        else
        {
            Debug.LogWarning("Menu text not assigned in inspector");
        }
        
        if (menuBackground != null) 
        {
            menuBackground.SetActive(true);
            Debug.Log("Menu background activated");
        }
        else
        {
            Debug.LogWarning("Menu background not assigned in inspector");
        }
        
        if (pauseButtonsContainer != null) 
        {
            pauseButtonsContainer.SetActive(true);
            Debug.Log("Pause buttons container activated");
        }
        else
        {
            Debug.LogWarning("Pause buttons container not assigned in inspector");
        }
        
        // Pause the game
        defaultTimeScale = Time.timeScale; // Save current time scale
        Time.timeScale = 0f;
        isPaused = true;
        
        // Disable player input during pause
        EnablePlayerControls(false);
        
        // Show cursor and unlock it
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log("Pause called. UI elements shown.");
    }

    void EnablePlayerControls(bool enable)
    {
        // Disable/enable player movement components
        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
            playerMovement.enabled = enable;
            
        GrabController grabController = FindObjectOfType<GrabController>();
        if (grabController != null)
            grabController.enabled = enable;
            
        // Disable camera controls when paused
        ThirdPersonCamera thirdPersonCamera = FindObjectOfType<ThirdPersonCamera>();
        if (thirdPersonCamera != null)
            thirdPersonCamera.enabled = enable;
    }

    void CloseAllSubmenus()
    {
        settingsMenuUI.SetActive(false);
        controlsMenuUI.SetActive(false);
        graphicsMenuUI.SetActive(false);
        audioMenuUI.SetActive(false);
    }

    public void OpenSettings()
    {
        CloseAllSubmenus();
        settingsMenuUI.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsMenuUI.SetActive(false);
    }

    public void OpenControls()
    {
        CloseAllSubmenus();
        controlsMenuUI.SetActive(true);
    }

    public void CloseControls()
    {
        controlsMenuUI.SetActive(false);
    }

    public void OpenGraphics()
    {
        CloseAllSubmenus();
        graphicsMenuUI.SetActive(true);
    }

    public void CloseGraphics()
    {
        graphicsMenuUI.SetActive(false);
    }

    public void OpenAudio()
    {
        CloseAllSubmenus();
        audioMenuUI.SetActive(true);
    }

    public void CloseAudio()
    {
        audioMenuUI.SetActive(false);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Reset time scale before loading main menu
        SceneManager.LoadScene("MainMenu"); // Assuming you have a scene named "MainMenu"
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
        // Save sensitivity value
        PlayerPrefs.SetFloat("Sensitivity", sensitivity);
        
        // Apply to camera controller if it has a sensitivity property
        ThirdPersonCamera cameraController = FindObjectOfType<ThirdPersonCamera>();
        if (cameraController != null)
        {
            // Assuming the camera has a public property to set sensitivity
            // cameraController.sensitivity = sensitivity;
        }
    }
}
