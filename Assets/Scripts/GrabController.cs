using UnityEngine;

public class GrabController : MonoBehaviour
{
    [SerializeField] private float grabDistance = 3f;
    [SerializeField] private LayerMask handleLayer;
    
    private GameObject grabbedObject;
    private Rigidbody playerRb;
    private bool isGrabbing;

    private void Start()
    {
        playerRb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
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
    }

    private void TryGrab()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, grabDistance, handleLayer))
        {
            grabbedObject = hit.collider.gameObject;
            isGrabbing = true;
            
            // Freeze player position
            playerRb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
        }
    }

    private void Release()
    {
        if (grabbedObject != null)
        {
            grabbedObject = null;
            isGrabbing = false;
            
            // Unfreeze player position
            playerRb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }
}
