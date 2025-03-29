using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InGameMenuManager : MonoBehaviour
{
    [Header("Save Menu")]
    [SerializeField] private GameObject saveMenuUI;
    [SerializeField] private Transform saveSlotContainer;
    [SerializeField] private GameObject saveSlotPrefab;
    [SerializeField] private int maxSaveSlots = 3;
    
    private PauseMenu pauseMenu;
    private List<SaveSlotUI> saveSlotUIList = new List<SaveSlotUI>();
    
    private void Awake()
    {
        pauseMenu = GetComponent<PauseMenu>();
        if (pauseMenu == null)
        {
            Debug.LogError("PauseMenu компонент не найден на том же объекте!");
        }
    }
    
    private void Start()
    {
        if (saveMenuUI != null)
        {
            saveMenuUI.SetActive(false);
        }
    }
    
    public void OpenSaveMenu()
    {
        // Закрываем меню паузы и открываем меню сохранения
        pauseMenu.CloseAllSubmenus();
        saveMenuUI.SetActive(true);
        
        // Обновляем информацию о слотах сохранения
        RefreshSaveSlots();
    }
    
    public void CloseSaveMenu()
    {
        saveMenuUI.SetActive(false);
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
                int slotIndex = i; // Создаем локальную копию для замыкания
                
                // Получаем информацию о сохранении
                SaveInfo saveInfo = saveManager.GetSaveInfo(i);
                
                // Для UI сохранения в игре переопределяем поведение слота
                Button saveButton = slotUI.GetComponent<Button>();
                if (saveButton != null)
                {
                    saveButton.onClick.RemoveAllListeners();
                    saveButton.onClick.AddListener(() => SaveGame(slotIndex));
                }
                
                // Инициализируем слот
                if (saveInfo != null)
                {
                    slotUI.Initialize(saveInfo, null);
                }
                else
                {
                    slotUI.InitializeEmpty(i, null);
                }
                
                saveSlotUIList.Add(slotUI);
            }
        }
    }
    
    private void SaveGame(int saveSlot)
    {
        // Проверяем наличие менеджера сохранений
        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.SaveGame(saveSlot);
            
            // Обновляем UI после сохранения
            RefreshSaveSlots();
        }
        else
        {
            Debug.LogError("GameSaveManager не найден в сцене!");
        }
    }
} 