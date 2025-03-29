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

    [Header("Main Menu Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton; 
    [SerializeField] private Button exitButton;

    [Header("Settings Components")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Dropdown qualityDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Dropdown resolutionDropdown;
    [SerializeField] private Slider sensitivitySlider;

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
        // Проверяем, есть ли сохраненная игра
        hasSavedGame = PlayerPrefs.HasKey("SavedGameScene");
        
        // Настраиваем кнопку Continue в зависимости от наличия сохранения
        if (continueButton != null)
        {
            continueButton.interactable = hasSavedGame;
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
            // Загружаем последнюю сохраненную сцену
            string savedScene = PlayerPrefs.GetString("SavedGameScene", continueGameSceneName);
            SceneManager.LoadScene(savedScene);
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
                }
                else
                {
                    slotUI.InitializeEmpty(i, this);
                }
                
                saveSlotUIList.Add(slotUI);
            }
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

    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsMenuPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsMenuPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

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
        qualityDropdown.ClearOptions();
        List<string> qualityOptions = new List<string>(QualitySettings.names);
        qualityDropdown.AddOptions(qualityOptions);
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.RefreshShownValue();

        // Инициализация полноэкранного режима
        fullscreenToggle.isOn = Screen.fullScreen;

        // Инициализация разрешений
        InitializeResolutions();

        // Инициализация настроек звука
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        musicVolumeSlider.value = musicVolume;
        sfxVolumeSlider.value = sfxVolume;

        // Инициализация чувствительности мыши
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
        // Настройка громкости музыки
        PlayerPrefs.SetFloat("MusicVolume", volume);
        
        // Если у вас есть AudioMixer:
        // audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
    }

    public void SetSFXVolume(float volume)
    {
        // Настройка громкости звуковых эффектов
        PlayerPrefs.SetFloat("SFXVolume", volume);
        
        // Если у вас есть AudioMixer:
        // audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
    }

    public void SetSensitivity(float sensitivity)
    {
        // Сохраняем значение чувствительности
        PlayerPrefs.SetFloat("Sensitivity", sensitivity);
    }
} 