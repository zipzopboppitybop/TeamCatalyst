using UnityEngine;

public class CameraController : MonoBehaviour
{

    [SerializeField] int mouseSens;
    [SerializeField] int vertLockMin, vertLockMax;
    [SerializeField] bool invertY;

    // Toggle for third-person camera, this could most likely be moved to the actual player in case they need changes between first and third person (VERY LIKELY).
    [SerializeField] bool thirdPersonEnabled;

    float rotX;

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

    }

    // Update is called once per frame
    void Update()
    {
        
        // Most of this was covered before, but I'll comment on things that are new/important.

        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSens * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSens * Time.deltaTime;

        if (invertY)
        {
            rotX += mouseY;
        }
        else
        {
            rotX -= mouseY;
        }

        rotX = Mathf.Clamp(rotX, vertLockMin, vertLockMax);

        // Handles vertical mouse rotation.
        transform.localRotation = Quaternion.Euler(rotX, 0, 0);

        // Handles horizontal mouse rotation.
        if (!thirdPersonEnabled) { 

            transform.parent.Rotate(Vector2.up * mouseX);

        }

    }
}
