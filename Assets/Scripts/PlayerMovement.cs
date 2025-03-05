using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private Transform cameraTransform;
    
    private Rigidbody rb;
    private Vector3 moveDirection;
    private bool isGrounded;
    private float originalHeight;
    private bool isCrouching;
    private CapsuleCollider capsuleCollider;
    private Vector3 currentVelocity;
    private GravityController gravityController;
    private bool wasGroundedBeforeGravityToggle = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponentInChildren<CapsuleCollider>();
        originalHeight = capsuleCollider.height;
        gravityController = GetComponent<GravityController>();
        
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        bool wasGrounded = isGrounded;
        isGrounded = rb.useGravity && Physics.Raycast(transform.position, Vector3.down, 1.1f);

        // Сохраняем полную скорость при изменении состояния гравитации
        if (rb.useGravity != gravityController.IsGravityEnabled)
        {
            currentVelocity = rb.linearVelocity;
        }
        
        // Управление движением только на земле
        if (isGrounded)
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");

            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            moveDirection = (forward * verticalInput + right * horizontalInput).normalized;
        }

        // Jump только при включенной гравитации
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            currentVelocity = rb.linearVelocity;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // Crouch
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (!isCrouching)
            {
                capsuleCollider.height = crouchHeight;
                isCrouching = true;
            }
            else
            {
                capsuleCollider.height = originalHeight;
                isCrouching = false;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            float angle = Vector3.Angle(contact.normal, Vector3.up);
            if (angle > 45f)
            {
                Vector3 normalVelocity = Vector3.Project(currentVelocity, contact.normal);
                currentVelocity -= normalVelocity;
                break;
            }
        }
    }

    private void FixedUpdate()
    {
        if (isGrounded)
        {
            // На земле с гравитацией
            Vector3 movement = moveDirection * moveSpeed;
            rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);
            currentVelocity = rb.linearVelocity;
        }
        else if (!rb.useGravity)
        {
            // В невесомости сохраняем полную скорость
            rb.linearVelocity = currentVelocity;
        }
        else
        {
            // В воздухе с гравитацией сохраняем горизонтальную скорость
            rb.linearVelocity = new Vector3(currentVelocity.x, rb.linearVelocity.y, currentVelocity.z);
        }
    }
}
