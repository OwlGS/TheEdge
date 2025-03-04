using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityController : MonoBehaviour
{
    private Rigidbody rb;
    private bool isGravityEnabled = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            ToggleGravity();
        }
    }

    private void ToggleGravity()
    {
        isGravityEnabled = !isGravityEnabled;
        rb.useGravity = isGravityEnabled;
        
        if (!isGravityEnabled)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }
}
