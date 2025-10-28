using UnityEngine;

public class CameraController : MonoBehaviour
{

    [SerializeField] LayerMask ignoreLayer;

    [SerializeField] int mouseSens;
    [SerializeField] int vertLockMin, vertLockMax;
    [SerializeField] bool invertY;

    // Toggle for third-person camera, this could most likely be moved to the actual player in case they need changes between first and third person (VERY LIKELY).
    [SerializeField] bool thirdPersonEnabled;

    // Camera variables.
    [SerializeField] int camDist;

    float rotX;
    float rotY;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        ChangeCameraView();

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

        rotY += mouseX;

        // Handles camera rotation.

        transform.parent.localRotation = Quaternion.Euler(rotX, rotY, 0);

        // If camera would go through object, adjust position to last available spot.
        if (thirdPersonEnabled)
        {    
            RaycastHit distToObject;
            if (Physics.Raycast(transform.parent.position, -transform.parent.forward, out distToObject, camDist, ~ignoreLayer))
            {

                transform.position = distToObject.point;

            }
            
            //Debug.DrawRay(transform.parent.position, -transform.parent.forward * camDist, Color.red);
        }

    }
    
    public void ChangeCameraView()
    {

        if (thirdPersonEnabled) { 
        
            transform.localPosition = new Vector3(0, 0, -camDist);

        }
        else
        {

            transform.localPosition = Vector3.zero;

        }

    }

}
