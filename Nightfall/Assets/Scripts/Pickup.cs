using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField] ItemData item;

    private void OnTriggerEnter(Collider other)
    {
        IPickup pickup = other.GetComponent<IPickup>();

        if (pickup != null)
        {
            if (pickup.AddToInventory(item, 1))
            {
                if (item.pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(item.pickupSound, Camera.main.transform.position, item.audVol);
                }

                Destroy(gameObject);
            }
        }

    }
}
