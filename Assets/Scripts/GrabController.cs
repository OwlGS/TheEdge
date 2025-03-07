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
    }

    private void Update()
    {
        if (mainCamera == null) return;

        // Проверяем наличие поручней рядом с игроком
        Collider[] nearbyHandles = Physics.OverlapSphere(transform.position, grabDistance, handleLayer);
        
        if (showDebugRay && nearbyHandles.Length > 0)
        {
            foreach (Collider handle in nearbyHandles)
            {
                Vector3 closestPoint = handle.ClosestPoint(transform.position);
                Debug.DrawLine(transform.position, closestPoint, Color.yellow);
            }
        }
        
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

    private void StopMovement()
    {
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
        }
    }

    private void TryGrab()
    {
        Collider[] nearbyHandles = Physics.OverlapSphere(transform.position, grabDistance, handleLayer);
        
        if (nearbyHandles.Length > 0)
        {
            float closestDistance = float.MaxValue;
            Collider closestHandle = null;
            Vector3 closestPoint = Vector3.zero;
            
            foreach (Collider handle in nearbyHandles)
            {
                Vector3 point = handle.ClosestPoint(transform.position);
                float distance = Vector3.Distance(transform.position, point);
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestHandle = handle;
                    closestPoint = point;
                }
            }

            if (closestHandle != null)
            {
                grabbedObject = closestHandle.gameObject;
                isGrabbing = true;
                
                // Вычисляем нормаль от ближайшей точки
                RaycastHit hit;
                if (Physics.Raycast(closestPoint + Vector3.up * 0.1f, Vector3.down, out hit, 0.2f, handleLayer))
                {
                    grabNormal = hit.normal;
                }
                else
                {
                    grabNormal = (transform.position - closestPoint).normalized;
                }
                
                grabPoint = closestPoint + (grabNormal * grabOffset);
                
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
            
            // Останавливаем движение перед толчком
            StopMovement();
            playerRb.constraints = RigidbodyConstraints.FreezeRotation;
            
            // Применяем силу
            playerRb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
            
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
            grabbedObject = null;
            isGrabbing = false;
            
            if (playerRb != null)
            {
                StopMovement();
                playerRb.constraints = RigidbodyConstraints.FreezeRotation;
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
