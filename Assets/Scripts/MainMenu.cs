using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject graphicsPanel;
    [SerializeField] private GameObject audioPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject creditsPanel;
    
    [Header("Settings Components")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Dropdown qualityDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Dropdown resolutionDropdown;
    [SerializeField] private Slider sensitivitySlider;
    
    [Header("Game Settings")]
    [SerializeField] private string gameSceneName = "GameScene"; // Название сцены с игрой
    
    private Resolution[] resolutions;
    
    private void Start()
    {
        // Показываем курсор и разблокируем его
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Устанавливаем нормальную скорость времени (на случай, если до этого была пауза)
        Time.timeScale = 1f;
        
        // Инициализация настроек
        InitializeSettings();
    }
    
    // Инициализация всех настроек
    private void InitializeSettings()
    {
        // Инициализация выпадающего списка разрешений
        InitializeResolutions();
        
        // Инициализация настроек качества
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        
        // Инициализация полноэкранного режима
        fullscreenToggle.isOn = Screen.fullScreen;
        
        // Инициализация громкости
        if (audioMixer != null)
        {
            // Загружаем сохраненные значения громкости или используем дефолтные
            float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0f);
            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0f);
            
            if (musicVolumeSlider != null)
                musicVolumeSlider.value = musicVolume;
            
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.value = sfxVolume;
                
            // Устанавливаем значения в миксер
            audioMixer.SetFloat("MusicVolume", musicVolume);
            audioMixer.SetFloat("SFXVolume", sfxVolume);
        }
        
        // Загрузка сохраненной чувствительности
        if (sensitivitySlider != null)
            sensitivitySlider.value = PlayerPrefs.GetFloat("Sensitivity", 1.0f);
    }
    
    // Инициализация списка разрешений
    private void InitializeResolutions()
    {
        resolutions = Screen.resolutions;
        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            
            List<string> options = new List<string>();
            int currentResolutionIndex = 0;
            
            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = resolutions[i].width + " x " + resolutions[i].height;
                options.Add(option);
                
                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }
            
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
        }
    }
    
    #region Menu Navigation
    
    // Новая игра
    public void StartNewGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }
    
    // Продолжить игру (загрузка сохранения)
    public void ContinueGame()
    {
        // Здесь код для загрузки сохраненной игры
        // Для примера просто загружаем игровую сцену
        SceneManager.LoadScene(gameSceneName);
    }
    
    // Выход из игры
    public void QuitGame()
    {
        Debug.Log("Выход из игры");
        Application.Quit();
    }
    
    // Открыть настройки
    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }
    
    // Закрыть настройки
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
    
    // Открыть настройки графики
    public void OpenGraphicsSettings()
    {
        settingsPanel.SetActive(false);
        graphicsPanel.SetActive(true);
    }
    
    // Закрыть настройки графики
    public void CloseGraphicsSettings()
    {
        graphicsPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }
    
    // Открыть настройки звука
    public void OpenAudioSettings()
    {
        settingsPanel.SetActive(false);
        audioPanel.SetActive(true);
    }
    
    // Закрыть настройки звука
    public void CloseAudioSettings()
    {
        audioPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }
    
    // Открыть настройки управления
    public void OpenControlsSettings()
    {
        settingsPanel.SetActive(false);
        controlsPanel.SetActive(true);
    }
    
    // Закрыть настройки управления
    public void CloseControlsSettings()
    {
        controlsPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }
    
    // Открыть титры
    public void OpenCredits()
    {
        mainMenuPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }
    
    // Закрыть титры
    public void CloseCredits()
    {
        creditsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
    
    #endregion
    
    #region Settings Functions
    
    // Установка разрешения экрана
    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
    
    // Установка качества графики
    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("QualityLevel", qualityIndex);
    }
    
    // Установка полноэкранного режима
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }
    
    // Установка громкости музыки
    public void SetMusicVolume(float volume)
    {
        if (audioMixer != null)
            audioMixer.SetFloat("MusicVolume", volume);
        
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }
    
    // Установка громкости эффектов
    public void SetSFXVolume(float volume)
    {
        if (audioMixer != null)
            audioMixer.SetFloat("SFXVolume", volume);
        
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
    
    // Установка чувствительности мыши
    public void SetSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat("Sensitivity", sensitivity);
    }
    
    #endregion
} 