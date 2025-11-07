using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [SerializeField] DoorController door;
    [SerializeField] float openAngle = 90;
    [SerializeField] float closeAngle = 0;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (door != null)
            {
                //Debug.Log("OpeningDoor!"); 
                door.OpenDoor(openAngle);
            }
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (door != null)
            {
                //Debug.Log("ClosingDoor!");
                door.CloseDoor(closeAngle);

            }

        }

    }

}
