using Unity.Mathematics;
using UnityEngine;
using System.Collections;


public class DoorController : MonoBehaviour
{
    public GameObject doorHinge;
    public float rotationSpeed = 2f;

    private bool isOpen = false;


    public void OpenDoor(float openAngle)
    {
        if (!isOpen)
        {
            StopAllCoroutines();
            StartCoroutine(RotateDoor(openAngle));
            isOpen = true;
        }

    }

    public void CloseDoor(float closeAngle)
    {
        if (isOpen)
        {
            StopAllCoroutines();
            StartCoroutine(RotateDoor(closeAngle));
            isOpen = false;
        }

    }

    IEnumerator RotateDoor(float targetAngle)
    {

        Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);

        while (Quaternion.Angle(doorHinge.transform.localRotation, targetRotation) > 0.01f)
        {
            doorHinge.transform.localRotation = Quaternion.Lerp(doorHinge.transform.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return null;
        }
        doorHinge.transform.localRotation = targetRotation;

    }

}


