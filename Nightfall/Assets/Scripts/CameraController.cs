using UnityEngine;

public class CameraController : MonoBehaviour
{

    [SerializeField] int mouseSens;
    [SerializeField] int vertLockMin, vertLockMax;
    [SerializeField] bool invertY;
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

        transform.localRotation = Quaternion.Euler(rotX, 0, 0);

        if (!thirdPersonEnabled) { 

            transform.parent.Rotate(Vector2.up * mouseX);

        }

    }
}
