using UnityEngine;

public class GrabController : MonoBehaviour
{
    [Header("Grab Settings")]
    [SerializeField] private float grabDistance = 3f;
    [SerializeField] private float grabRadius = 0.5f;
    [SerializeField] private LayerMask handleLayer;
    [SerializeField] private float grabOffset = 0.5f;
    
    [Header("Push Settings")]
    [SerializeField] private float pushForce = 10f;
    [SerializeField] private float maxPushDistance = 10f;
    
    [Header("UI References")]
    [SerializeField] private CrosshairUI crosshair;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugRay = true;
    
    private GameObject grabbedObject;
    private Rigidbody playerRb;
    private bool isGrabbing;
    private Camera mainCamera;
    private PlayerMovement playerMovement;
    private Vector3 grabPoint;
    private Vector3 grabNormal;

    private void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        playerMovement = GetComponent<PlayerMovement>();
        
        if (handleLayer == 0)
        {
            int handleLayerIndex = LayerMask.NameToLayer("Handle");
            if (handleLayerIndex != -1)
            {
                handleLayer = 1 << handleLayerIndex;
            }
            else
            {
                Debug.LogError("Layer 'Handle' not found!");
            }
        }
        
        // Найти компонент CrosshairUI, если он не назначен
        if (crosshair == null)
        {
            crosshair = FindObjectOfType<CrosshairUI>();
        }
    }

    private void Update()
    {
        if (mainCamera == null) return;

        // Проверка интерактивных объектов перед игроком для прицела
        CheckInteractiveObjects();
        
        if (Input.GetMouseButtonDown(0))
        {
            if (!isGrabbing)
            {
                TryGrab();
            }
            else
            {
                Release();
            }
        }
        
        if (Input.GetMouseButtonDown(1) && isGrabbing)
        {
            TryPushOff();
        }

        if (isGrabbing && grabbedObject != null)
        {
            UpdateGrabPosition();
        }
    }

    private void CheckInteractiveObjects()
    {
        // В режиме от первого лица используем луч из центра экрана
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, grabDistance, handleLayer))
        {
            // Есть интерактивный объект под прицелом
            if (crosshair != null)
            {
                crosshair.SetCrosshairInteractable();
            }
            
            if (showDebugRay)
            {
                Debug.DrawLine(ray.origin, hit.point, Color.green);
            }
        }
        else
        {
            // Нет интерактивного объекта под прицелом
            if (crosshair != null)
            {
                crosshair.SetCrosshairDefault();
            }
            
            if (showDebugRay)
            {
                Debug.DrawRay(ray.origin, ray.direction * grabDistance, Color.yellow);
            }
        }
    }

    private void StopMovement()
    {
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
            
            // Обновляем сохраненную скорость в PlayerMovement
            PlayerMovement playerMov = GetComponent<PlayerMovement>();
            if (playerMov != null)
            {
                playerMov.ResetSavedVelocity();
            }
        }
    }

    private void TryGrab()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        
        // Сначала пробуем точный луч из центра экрана
        if (Physics.Raycast(ray, out hit, grabDistance, handleLayer))
        {
            HandleGrab(hit.collider.gameObject, hit.point, hit.normal);
            return;
        }
        
        // Проверяем наличие поручней рядом с игроком как запасной вариант
        Collider[] nearbyHandles = Physics.OverlapSphere(transform.position, grabDistance, handleLayer);
        
        if (nearbyHandles.Length > 0)
        {
            float closestDistance = float.MaxValue;
            Collider closestHandle = null;
            Vector3 closestPoint = Vector3.zero;
            Vector3 closestNormal = Vector3.zero;
            
            foreach (Collider handle in nearbyHandles)
            {
                Vector3 point = handle.ClosestPoint(transform.position);
                float distance = Vector3.Distance(transform.position, point);
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestHandle = handle;
                    closestPoint = point;
                    
                    // Пытаемся получить нормаль
                    RaycastHit handleHit;
                    if (Physics.Raycast(transform.position, point - transform.position, out handleHit, grabDistance, handleLayer))
                    {
                        closestNormal = handleHit.normal;
                    }
                    else
                    {
                        closestNormal = (transform.position - point).normalized;
                    }
                }
            }

            if (closestHandle != null)
            {
                HandleGrab(closestHandle.gameObject, closestPoint, closestNormal);
            }
        }
    }
    
    private void HandleGrab(GameObject handle, Vector3 point, Vector3 normal)
    {
        grabbedObject = handle;
        isGrabbing = true;
        
        grabNormal = normal;
        grabPoint = point + (grabNormal * grabOffset);
        
        // Поворачиваем игрока лицом к поверхности захвата
        Vector3 lookDirection = -grabNormal;
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        if (playerRb != null)
        {
            StopMovement();
            playerRb.constraints = RigidbodyConstraints.FreezeAll;
        }
        
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
    }

    private void UpdateGrabPosition()
    {
        if (grabbedObject != null)
        {
            transform.position = Vector3.Lerp(transform.position, grabPoint, Time.deltaTime * 15f);
        }
    }

    private void TryPushOff()
    {
        Vector3 pushDirection = mainCamera.transform.forward;
        
        if (playerRb != null)
        {
            // Сначала освобождаем от поручня
            grabbedObject = null;
            isGrabbing = false;
            
            // Полностью останавливаем движение перед толчком
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
            playerRb.constraints = RigidbodyConstraints.FreezeRotation;
            
            // Принудительно устанавливаем скорость вместо добавления силы
            // Это гарантирует мгновенный эффект
            playerRb.linearVelocity = pushDirection * pushForce / playerRb.mass;
            
            Debug.Log($"Pushed off with velocity: {playerRb.linearVelocity}, magnitude: {playerRb.linearVelocity.magnitude}");
            
            // Обновляем сохраненную скорость для невесомости
            PlayerMovement playerMov = GetComponent<PlayerMovement>();
            if (playerMov != null)
            {
                playerMov.UpdateSavedVelocity(playerRb.linearVelocity);
            }
            
            if (playerMovement != null)
            {
                playerMovement.enabled = true;
            }
        }
    }

    private void Release()
    {
        if (grabbedObject != null)
        {
            Debug.Log("Releasing handle");
            grabbedObject = null;
            isGrabbing = false;
            
            if (playerRb != null)
            {
                // Полностью останавливаем движение при отпускании
                playerRb.linearVelocity = Vector3.zero;
                playerRb.angularVelocity = Vector3.zero;
                playerRb.constraints = RigidbodyConstraints.FreezeRotation;
                
                // Сбрасываем сохраненную скорость
                PlayerMovement playerMov = GetComponent<PlayerMovement>();
                if (playerMov != null)
                {
                    playerMov.ResetSavedVelocity();
                }
            }
            
            if (playerMovement != null)
            {
                playerMovement.enabled = true;
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Отображаем радиус захвата
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, grabDistance);
        
        if (isGrabbing && grabbedObject != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(grabPoint, 0.1f);
            Gizmos.DrawRay(grabPoint, grabNormal);
        }
    }
}
