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
    [SerializeField] private GameObject settingsMenuPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject loadGamePanel;

    [Header("Settings Category Panels")]
    [SerializeField] private GameObject settingsCategoryPanel; // Панель с кнопками категорий настроек
    [SerializeField] private GameObject graphicsSettingsPanel; // Панель настроек графики
    [SerializeField] private GameObject audioSettingsPanel; // Панель настроек звука
    [SerializeField] private GameObject brightnessSettingsPanel; // Панель настроек яркости
    [SerializeField] private GameObject controlsSettingsPanel; // Панель настроек управления

    [Header("Settings Components")]
    // Звук
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    
    // Графика
    [SerializeField] private Dropdown qualityDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Dropdown resolutionDropdown;
    
    // Яркость
    [SerializeField] private Slider brightnessSlider;
    
    // Управление
    [SerializeField] private Slider sensitivitySlider;

    [Header("Main Menu Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton; 
    [SerializeField] private Button exitButton;

    [Header("Save Slots")]
    [SerializeField] private Transform saveSlotContainer; // Контейнер для слотов сохранения
    [SerializeField] private GameObject saveSlotPrefab; // Префаб слота сохранения
    [SerializeField] private int maxSaveSlots = 3; // Максимальное количество слотов сохранения

    [Header("Game Settings")]
    [SerializeField] private string newGameSceneName = "Level1"; // Имя сцены для новой игры
    [SerializeField] private string continueGameSceneName = "Level1"; // Временно такое же, потом заменим на сохраненную сцену

    private Resolution[] resolutions;
    private bool hasSavedGame = false;
    private List<SaveSlotUI> saveSlotUIList = new List<SaveSlotUI>();

    private void Awake()
    {
        // Проверяем наличие сохранений через GameSaveManager
        if (GameSaveManager.Instance != null)
        {
            // Проверяем наличие последнего использованного слота
            if (PlayerPrefs.HasKey("LastSaveSlot"))
            {
                int lastSlot = PlayerPrefs.GetInt("LastSaveSlot", 0);
                hasSavedGame = GameSaveManager.Instance.HasSaveInSlot(lastSlot);
            }
            else
            {
                // Если нет информации о последнем слоте, проверяем все слоты
                hasSavedGame = false;
                for (int i = 0; i < maxSaveSlots; i++)
                {
                    if (GameSaveManager.Instance.HasSaveInSlot(i))
                    {
                        hasSavedGame = true;
                        break;
                    }
                }
            }
        }
        else
        {
            // Если GameSaveManager не найден, используем простую проверку
            hasSavedGame = PlayerPrefs.HasKey("SavedGameScene");
        }
        
        // Настраиваем кнопку Continue в зависимости от наличия сохранения
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(hasSavedGame);
            continueButton.interactable = hasSavedGame;
            Debug.Log($"Continue button active: {hasSavedGame}");
        }
    }

    private void Start()
    {
        // Открываем только главное меню при запуске
        OpenMainMenu();
        
        // Инициализируем настройки
        InitializeSettings();
        
        // Убеждаемся, что курсор видим и не заблокирован
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Проверяем наличие GameSaveManager и инициализируем его, если необходимо
        if (GameSaveManager.Instance == null)
        {
            GameObject saveManager = new GameObject("GameSaveManager");
            saveManager.AddComponent<GameSaveManager>();
            Debug.Log("GameSaveManager был автоматически создан");
        }
        
        // Перепроверяем состояние кнопки Continue (на случай, если GameSaveManager был создан после Awake)
        if (continueButton != null && !hasSavedGame)
        {
            // Проверяем наличие сохранений еще раз
            if (GameSaveManager.Instance != null)
            {
                for (int i = 0; i < maxSaveSlots; i++)
                {
                    if (GameSaveManager.Instance.HasSaveInSlot(i))
                    {
                        hasSavedGame = true;
                        continueButton.gameObject.SetActive(true);
                        continueButton.interactable = true;
                        Debug.Log("Found saves after GameSaveManager initialization");
                        break;
                    }
                }
            }
        }
    }

    public void OpenMainMenu()
    {
        // Активируем только главную панель
        mainMenuPanel.SetActive(true);
        settingsMenuPanel.SetActive(false);
        creditsPanel.SetActive(false);
        loadGamePanel.SetActive(false);
    }

    public void ContinueGame()
    {
        if (hasSavedGame)
        {
            if (GameSaveManager.Instance != null)
            {
                // Если есть информация о последнем использованном слоте, загружаем его
                if (PlayerPrefs.HasKey("LastSaveSlot"))
                {
                    int lastSlot = PlayerPrefs.GetInt("LastSaveSlot", 0);
                    if (GameSaveManager.Instance.HasSaveInSlot(lastSlot))
                    {
                        GameSaveManager.Instance.LoadGame(lastSlot);
                        return;
                    }
                }
                
                // Если нет информации о последнем слоте или он пуст, ищем любое сохранение
                for (int i = 0; i < maxSaveSlots; i++)
                {
                    if (GameSaveManager.Instance.HasSaveInSlot(i))
                    {
                        GameSaveManager.Instance.LoadGame(i);
                        return;
                    }
                }
            }
            else
            {
                // Если GameSaveManager не найден, используем простой подход
                string savedScene = PlayerPrefs.GetString("SavedGameScene", continueGameSceneName);
                SceneManager.LoadScene(savedScene);
            }
        }
        else
        {
            Debug.LogWarning("Попытка продолжить игру, но нет доступных сохранений.");
        }
    }

    public void NewGame()
    {
        // Загружаем сцену новой игры
        SceneManager.LoadScene(newGameSceneName);
    }

    public void OpenLoadGame()
    {
        mainMenuPanel.SetActive(false);
        loadGamePanel.SetActive(true);
        
        // Обновляем информацию о слотах сохранения
        RefreshSaveSlots();
    }

    public void RefreshSaveSlots()
    {
        // Очищаем старые слоты
        foreach (SaveSlotUI slot in saveSlotUIList)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        saveSlotUIList.Clear();
        
        // Если нет контейнера или префаба для слотов - выходим
        if (saveSlotContainer == null || saveSlotPrefab == null)
        {
            Debug.LogError("Не назначен контейнер или префаб для слотов сохранения!");
            return;
        }

        // Проверяем наличие менеджера сохранений
        GameSaveManager saveManager = GameSaveManager.Instance;
        if (saveManager == null)
        {
            Debug.LogError("GameSaveManager не найден в сцене!");
            return;
        }
        
        // Сбрасываем флаг наличия сохранений
        hasSavedGame = false;
        
        // Создаем слоты сохранения
        for (int i = 0; i < maxSaveSlots; i++)
        {
            // Создаем объект слота
            GameObject slotObj = Instantiate(saveSlotPrefab, saveSlotContainer);
            SaveSlotUI slotUI = slotObj.GetComponent<SaveSlotUI>();
            
            if (slotUI != null)
            {
                // Получаем информацию о сохранении
                SaveInfo saveInfo = saveManager.GetSaveInfo(i);
                
                // Инициализируем слот
                if (saveInfo != null)
                {
                    slotUI.Initialize(saveInfo, this);
                    // Обнаружено сохранение
                    hasSavedGame = true;
                }
                else
                {
                    slotUI.InitializeEmpty(i, this);
                }
                
                saveSlotUIList.Add(slotUI);
            }
        }
        
        // Обновляем состояние кнопки Continue на основе наличия сохранений
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(hasSavedGame);
            continueButton.interactable = hasSavedGame;
            Debug.Log($"Continue button updated, active: {hasSavedGame}");
        }
    }

    public void LoadGame(int saveSlot)
    {
        // Здесь будет логика загрузки игры из определенного слота
        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.LoadGame(saveSlot);
        }
        else
        {
            Debug.LogError("GameSaveManager не найден в сцене!");
        }
    }

    #region Settings Menu

    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsMenuPanel.SetActive(true);
        
        // Открываем только панель категорий настроек
        settingsCategoryPanel.SetActive(true);
        CloseAllSettingSubpanels();
    }
    
    public void CloseSettings()
    {
        settingsMenuPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
    
    private void CloseAllSettingSubpanels()
    {
        if (graphicsSettingsPanel != null) graphicsSettingsPanel.SetActive(false);
        if (audioSettingsPanel != null) audioSettingsPanel.SetActive(false);
        if (brightnessSettingsPanel != null) brightnessSettingsPanel.SetActive(false);
        if (controlsSettingsPanel != null) controlsSettingsPanel.SetActive(false);
    }
    
    public void OpenGraphicsSettings()
    {
        settingsCategoryPanel.SetActive(false);
        CloseAllSettingSubpanels();
        graphicsSettingsPanel.SetActive(true);
    }
    
    public void OpenAudioSettings()
    {
        settingsCategoryPanel.SetActive(false);
        CloseAllSettingSubpanels();
        audioSettingsPanel.SetActive(true);
    }
    
    public void OpenBrightnessSettings()
    {
        settingsCategoryPanel.SetActive(false);
        CloseAllSettingSubpanels();
        brightnessSettingsPanel.SetActive(true);
    }
    
    public void OpenControlsSettings()
    {
        settingsCategoryPanel.SetActive(false);
        CloseAllSettingSubpanels();
        controlsSettingsPanel.SetActive(true);
    }
    
    public void BackToSettingsCategories()
    {
        CloseAllSettingSubpanels();
        settingsCategoryPanel.SetActive(true);
    }

    #endregion

    public void OpenCredits()
    {
        mainMenuPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void CloseCredits()
    {
        creditsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void ExitGame()
    {
        Debug.Log("Выход из игры...");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void InitializeSettings()
    {
        // Инициализация качества графики
        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            List<string> qualityOptions = new List<string>(QualitySettings.names);
            qualityDropdown.AddOptions(qualityOptions);
            qualityDropdown.value = QualitySettings.GetQualityLevel();
            qualityDropdown.RefreshShownValue();
        }

        // Инициализация полноэкранного режима
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
        }

        // Инициализация разрешений
        InitializeResolutions();

        // Инициализация настроек звука
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        if (musicVolumeSlider != null) musicVolumeSlider.value = musicVolume;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfxVolume;

        // Инициализация чувствительности мыши
        float sensitivity = PlayerPrefs.GetFloat("Sensitivity", 1.0f);
        if (sensitivitySlider != null) sensitivitySlider.value = sensitivity;
        
        // Инициализация яркости
        float brightness = PlayerPrefs.GetFloat("Brightness", 1.0f);
        if (brightnessSlider != null) brightnessSlider.value = brightness;
    }

    private void InitializeResolutions()
    {
        if (resolutionDropdown == null) return;
        
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

    public void SetResolution(int resolutionIndex)
    {
        if (resolutionIndex < 0 || resolutionIndex >= resolutions.Length) return;
        
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
        // Настройка громкости музыки
        PlayerPrefs.SetFloat("MusicVolume", volume);
        
        // Если у вас есть AudioMixer:
        if (audioMixer != null)
        {
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
        }
    }

    public void SetSFXVolume(float volume)
    {
        // Настройка громкости звуковых эффектов
        PlayerPrefs.SetFloat("SFXVolume", volume);
        
        // Если у вас есть AudioMixer:
        if (audioMixer != null)
        {
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        }
    }

    public void SetSensitivity(float sensitivity)
    {
        // Сохраняем значение чувствительности
        PlayerPrefs.SetFloat("Sensitivity", sensitivity);
        
        // Применяем к камере, если она есть
        FirstPersonCamera cameraController = FindObjectOfType<FirstPersonCamera>();
        if (cameraController != null)
        {
            cameraController.SetSensitivity(sensitivity);
        }
    }
    
    public void SetBrightness(float brightness)
    {
        // Сохраняем значение яркости
        PlayerPrefs.SetFloat("Brightness", brightness);
        
        // Здесь можно добавить код для настройки яркости в игре
        // Например, через пост-обработку или настройку экспозиции камеры
    }
} 