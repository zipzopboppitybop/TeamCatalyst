using UnityEngine;

public class Pickup : MonoBehaviour
{

    // We can use this to easily make more item types with one pickup script.
    [SerializeField] enum pickupType { Item }

    [SerializeField] int price;
    [SerializeField] int sellValue;

    private void OnTriggerEnter(Collider other)
    {
        
        IPickup pickup = other.GetComponent<IPickup>();

        if (pickup != null)
        {

            // Do whatever we need to do for each type of item, if we need to do anything.

        }

    }
}
