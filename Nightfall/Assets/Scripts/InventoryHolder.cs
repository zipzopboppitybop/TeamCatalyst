using UnityEngine;
using UnityEngine.Events;

public class InventoryHolder : MonoBehaviour
{
    [SerializeField] private int primaryInventorySize;
    [SerializeField] protected Inventory primaryInventory;

    public static UnityAction<Inventory> OnDynamicInventoryDisplayRequested;

    public Inventory PrimaryInventory => primaryInventory;

    protected virtual void Awake()
    {
        primaryInventory = new Inventory(primaryInventorySize);
    }
}
