using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI saveNameText;
    [SerializeField] private TextMeshProUGUI saveDateText;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button deleteButton;
    
    private int saveSlotIndex;
    private MainMenu mainMenu;
    
    // Инициализация слота сохранения
    public void Initialize(SaveInfo saveInfo, MainMenu menuReference)
    {
        mainMenu = menuReference;
        saveSlotIndex = saveInfo.saveSlot;
        
        // Устанавливаем информацию о сохранении
        saveNameText.text = $"Сохранение {saveSlotIndex + 1}";
        saveDateText.text = $"{saveInfo.sceneName}\n{saveInfo.saveDate}";
        
        // Настраиваем кнопки
        loadButton.onClick.RemoveAllListeners();
        loadButton.onClick.AddListener(() => LoadSaveSlot());
        
        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() => DeleteSaveSlot());
        
        // Активируем слот
        gameObject.SetActive(true);
    }
    
    // Инициализация пустого слота
    public void InitializeEmpty(int slotIndex, MainMenu menuReference)
    {
        mainMenu = menuReference;
        saveSlotIndex = slotIndex;
        
        // Устанавливаем информацию для пустого слота
        saveNameText.text = $"Пустой слот {slotIndex + 1}";
        saveDateText.text = "Нет сохранения";
        
        // Деактивируем кнопку загрузки
        loadButton.interactable = false;
        deleteButton.interactable = false;
        
        // Активируем слот
        gameObject.SetActive(true);
    }
    
    // Загрузить сохранение
    private void LoadSaveSlot()
    {
        // Проверяем наличие менеджера сохранений
        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.LoadGame(saveSlotIndex);
        }
        else
        {
            Debug.LogError("GameSaveManager не найден в сцене!");
        }
    }
    
    // Удалить сохранение
    private void DeleteSaveSlot()
    {
        // Проверяем наличие менеджера сохранений
        if (GameSaveManager.Instance != null)
        {
            // Показываем диалог подтверждения (можно реализовать отдельно)
            // Если пользователь подтвердил удаление:
            GameSaveManager.Instance.DeleteSave(saveSlotIndex);
            
            // Обновляем UI после удаления
            if (mainMenu != null)
            {
                mainMenu.RefreshSaveSlots();
            }
        }
        else
        {
            Debug.LogError("GameSaveManager не найден в сцене!");
        }
    }
} 