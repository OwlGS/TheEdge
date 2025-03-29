using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class GameSaveManager : MonoBehaviour
{
    public static GameSaveManager Instance { get; private set; }

    [SerializeField] private int maxSaveSlots = 3;
    
    private string saveFileTemplate = "save_{0}.dat"; // Шаблон имени файла сохранения
    
    private void Awake()
    {
        // Синглтон паттерн
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    // Сохранение игры
    public void SaveGame(int saveSlot)
    {
        if (saveSlot < 0 || saveSlot >= maxSaveSlots)
        {
            Debug.LogError($"Неверный слот сохранения: {saveSlot}. Доступные слоты: 0-{maxSaveSlots-1}");
            return;
        }
        
        // Создаем объект GameData с текущим состоянием игры
        GameData gameData = new GameData();
        
        // Заполняем данные
        gameData.playerPosition = new SerializableVector3(
            GameObject.FindGameObjectWithTag("Player").transform.position);
        gameData.currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        gameData.saveDate = DateTime.Now.ToString();
        
        // Добавляем инвентарь и другие важные данные
        // gameData.inventory = InventoryManager.Instance.GetSerializableInventory();
        
        // Сохраняем данные в файл
        string savePath = Path.Combine(Application.persistentDataPath, 
            string.Format(saveFileTemplate, saveSlot));
        
        try
        {
            using (FileStream stream = new FileStream(savePath, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, gameData);
            }
            
            // Запоминаем последний использованный слот
            PlayerPrefs.SetInt("LastSaveSlot", saveSlot);
            PlayerPrefs.SetString("SavedGameScene", gameData.currentScene);
            PlayerPrefs.Save();
            
            Debug.Log($"Игра успешно сохранена в слот {saveSlot}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка при сохранении игры: {e.Message}");
        }
    }
    
    // Загрузка игры из указанного слота
    public void LoadGame(int saveSlot)
    {
        if (saveSlot < 0 || saveSlot >= maxSaveSlots)
        {
            Debug.LogError($"Неверный слот сохранения: {saveSlot}. Доступные слоты: 0-{maxSaveSlots-1}");
            return;
        }
        
        string savePath = Path.Combine(Application.persistentDataPath, 
            string.Format(saveFileTemplate, saveSlot));
        
        if (File.Exists(savePath))
        {
            try
            {
                using (FileStream stream = new FileStream(savePath, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    GameData gameData = (GameData)formatter.Deserialize(stream);
                    
                    // Загружаем сцену
                    UnityEngine.SceneManagement.SceneManager.LoadScene(gameData.currentScene);
                    
                    // Обновляем позицию игрока и другие данные после загрузки сцены
                    StartCoroutine(SetPlayerDataAfterSceneLoad(gameData));
                    
                    Debug.Log($"Игра успешно загружена из слота {saveSlot}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Ошибка при загрузке игры: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"Сохранение не найдено в слоте {saveSlot}");
        }
    }
    
    // Корутина для установки данных игрока после загрузки сцены
    private System.Collections.IEnumerator SetPlayerDataAfterSceneLoad(GameData gameData)
    {
        // Ждем, пока сцена полностью загрузится
        yield return new WaitForEndOfFrame();
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = gameData.playerPosition.ToVector3();
            
            // Устанавливаем другие данные игрока
            // if (InventoryManager.Instance != null && gameData.inventory != null)
            //     InventoryManager.Instance.LoadInventory(gameData.inventory);
        }
        else
        {
            Debug.LogError("Игрок не найден после загрузки сцены");
        }
    }
    
    // Проверка наличия сохранения в указанном слоте
    public bool HasSaveInSlot(int saveSlot)
    {
        if (saveSlot < 0 || saveSlot >= maxSaveSlots)
            return false;
            
        string savePath = Path.Combine(Application.persistentDataPath, 
            string.Format(saveFileTemplate, saveSlot));
            
        return File.Exists(savePath);
    }
    
    // Получение информации о сохранении
    public SaveInfo GetSaveInfo(int saveSlot)
    {
        if (!HasSaveInSlot(saveSlot))
            return null;
            
        string savePath = Path.Combine(Application.persistentDataPath, 
            string.Format(saveFileTemplate, saveSlot));
            
        try
        {
            using (FileStream stream = new FileStream(savePath, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                GameData gameData = (GameData)formatter.Deserialize(stream);
                
                return new SaveInfo
                {
                    saveDate = gameData.saveDate,
                    sceneName = gameData.currentScene,
                    saveSlot = saveSlot
                };
            }
        }
        catch
        {
            return null;
        }
    }
    
    // Получение информации обо всех сохранениях
    public SaveInfo[] GetAllSavesInfo()
    {
        List<SaveInfo> savesInfo = new List<SaveInfo>();
        
        for (int i = 0; i < maxSaveSlots; i++)
        {
            SaveInfo info = GetSaveInfo(i);
            if (info != null)
                savesInfo.Add(info);
        }
        
        return savesInfo.ToArray();
    }
    
    // Удаление сохранения
    public void DeleteSave(int saveSlot)
    {
        if (saveSlot < 0 || saveSlot >= maxSaveSlots)
            return;
            
        string savePath = Path.Combine(Application.persistentDataPath, 
            string.Format(saveFileTemplate, saveSlot));
            
        if (File.Exists(savePath))
        {
            try
            {
                File.Delete(savePath);
                Debug.Log($"Сохранение в слоте {saveSlot} удалено");
            }
            catch (Exception e)
            {
                Debug.LogError($"Ошибка при удалении сохранения: {e.Message}");
            }
        }
    }
}

// Классы для сериализации данных

[Serializable]
public class GameData
{
    public SerializableVector3 playerPosition;
    public string currentScene;
    public string saveDate;
    // Здесь можно добавить другие данные, такие как:
    // public SerializableInventory inventory;
    // public int playerHealth;
    // public float gameTime;
    // и т.д.
}

[Serializable]
public class SerializableVector3
{
    public float x;
    public float y;
    public float z;
    
    public SerializableVector3(Vector3 vector)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }
    
    public SerializableVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    
    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}

// Класс для отображения информации о сохранении в UI
public class SaveInfo
{
    public int saveSlot;
    public string saveDate;
    public string sceneName;
} 