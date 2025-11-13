using Unity.Cinemachine;
using UnityEngine;

public class MousePosition3D : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask ignoreLayer;

    // Update is called once per frame
    void Update()
    {
        FollowMousePosition();
    }

    public Vector3 GetMouseWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, mainCamera.farClipPlane, ~ignoreLayer, QueryTriggerInteraction.Ignore))
        {
            return hit.point;
        }
        return Vector3.zero; // Return a default value if no hit

    }
    public void FollowMousePosition()
    {
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        transform.position = mouseWorldPos;
    }
}
