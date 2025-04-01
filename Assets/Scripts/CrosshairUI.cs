using UnityEngine;
using UnityEngine.UI;

public class CrosshairUI : MonoBehaviour
{
    [Header("Crosshair Settings")]
    [SerializeField] private Image crosshairImage;
    [SerializeField] private Color crosshairColor = Color.white;
    [SerializeField] private float crosshairSize = 5f;
    
    [Header("Interaction Settings")]
    [SerializeField] private bool changeOnInteractable = true;
    [SerializeField] private Color interactableColor = Color.green;
    [SerializeField] private float interactableSize = 7f;
    
    private RectTransform crosshairRect;
    
    private void Start()
    {
        if (crosshairImage == null)
        {
            // Если изображение не назначено, пытаемся найти его
            crosshairImage = GetComponent<Image>();
            
            if (crosshairImage == null)
            {
                Debug.LogError("CrosshairUI: No Image component found!");
                enabled = false;
                return;
            }
        }
        
        // Получаем RectTransform для изменения размера
        crosshairRect = crosshairImage.rectTransform;
        
        // Устанавливаем начальные параметры прицела
        SetCrosshairDefault();
    }
    
    // Устанавливаем обычный вид прицела
    public void SetCrosshairDefault()
    {
        if (crosshairImage == null) return;
        
        crosshairImage.color = crosshairColor;
        
        if (crosshairRect != null)
        {
            crosshairRect.sizeDelta = new Vector2(crosshairSize, crosshairSize);
        }
    }
    
    // Устанавливаем вид прицела при наведении на интерактивный объект
    public void SetCrosshairInteractable()
    {
        if (!changeOnInteractable || crosshairImage == null) return;
        
        crosshairImage.color = interactableColor;
        
        if (crosshairRect != null)
        {
            crosshairRect.sizeDelta = new Vector2(interactableSize, interactableSize);
        }
    }
} 