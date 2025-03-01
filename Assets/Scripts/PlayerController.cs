using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Basic Movement")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float normalHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    private bool isCrouching = false;
    private CapsuleCollider playerCollider;

    [Header("Zero Gravity Movement")]
    [SerializeField] private LayerMask handleLayer;
    [SerializeField] private float grabRadius = 1.5f;
    [SerializeField] private float pushForce = 10f;

    private bool gravityEnabled = true;
    private bool isGrabbing = false;
    private Transform currentHandle;
    private Rigidbody rb;
    private Camera mainCamera;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        playerCollider = GetComponent<CapsuleCollider>();
    }

    private void Update()
    {
        if (!isGrabbing)
        {
            // Базовое перемещение
            HandleMovement();
            
            // Прыжок
            if (Input.GetKeyDown(KeyCode.Space) && IsGrounded() && gravityEnabled)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }

            // Присед
            HandleCrouch();

            // Проверка захвата поручня
            if (Input.GetMouseButtonDown(0))
            {
                CheckHandleGrab();
            }
        }
        else
        {
            // Отталкивание при нажатии ПКМ
            if (Input.GetMouseButtonDown(1))
            {
                Push();
            }
            // Отпускание при повторном нажатии ЛКМ
            else if (Input.GetMouseButtonDown(0))
            {
                ReleaseHandle();
            }
        }

        // Переключение гравитации (только когда не держимся за поручень)
        if (Input.GetKeyDown(KeyCode.G) && !isGrabbing)
        {
            gravityEnabled = !gravityEnabled;
            rb.useGravity = gravityEnabled;
        }
    }

    private void HandleMovement()
    {
        float currentSpeed = isCrouching ? crouchSpeed : moveSpeed;
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput);
        movement = transform.TransformDirection(movement);
        movement *= currentSpeed;
        
        // Сохраняем текущую Y-скорость для правильной работы прыжка
        movement.y = rb.linearVelocity.y;
        rb.linearVelocity = movement;
    }

    private void HandleCrouch()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (!isCrouching)
            {
                // Приседание
                playerCollider.height = crouchHeight;
                transform.localScale = new Vector3(1f, 0.5f, 1f);
                isCrouching = true;
            }
        }
        else if (isCrouching)
        {
            // Проверка возможности встать (нет ли препятствий сверху)
            if (!Physics.Raycast(transform.position, Vector3.up, normalHeight - crouchHeight))
            {
                // Вставание
                playerCollider.height = normalHeight;
                transform.localScale = new Vector3(1f, 1f, 1f);
                isCrouching = false;
            }
        }
    }

    private void CheckHandleGrab()
    {
        Collider[] nearbyHandles = Physics.OverlapSphere(transform.position, grabRadius, handleLayer);
        
        if (nearbyHandles.Length > 0)
        {
            Transform nearestHandle = nearbyHandles[0].transform;
            float nearestDistance = Vector3.Distance(transform.position, nearestHandle.position);
            
            for (int i = 1; i < nearbyHandles.Length; i++)
            {
                float distance = Vector3.Distance(transform.position, nearbyHandles[i].transform.position);
                if (distance < nearestDistance)
                {
                    nearestHandle = nearbyHandles[i].transform;
                    nearestDistance = distance;
                }
            }

            GrabHandle(nearestHandle);
        }
    }

    private void GrabHandle(Transform handle)
    {
        isGrabbing = true;
        currentHandle = handle;
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true; // Фиксируем позицию
        
        Vector3 handlePos = handle.position;
        Vector3 directionToPlayer = (transform.position - handlePos).normalized;
        transform.position = handlePos + directionToPlayer * 1f;
    }

    private void Push()
    {
        if (!isGrabbing) return;

        ReleaseHandle(); // Сначала освобождаем от поручня
        Vector3 pushDirection = mainCamera.transform.forward;
        rb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
    }

    private void ReleaseHandle()
    {
        isGrabbing = false;
        currentHandle = null;
        rb.isKinematic = false; // Возвращаем физику
    }

    private bool IsGrounded()
    {
        float rayLength = 1.1f;
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        Debug.DrawRay(rayStart, Vector3.down * rayLength, Color.red);
        return Physics.Raycast(rayStart, Vector3.down, rayLength, groundLayer);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 0, 0.2f);
        Gizmos.DrawWireSphere(transform.position, grabRadius);
    }
} 