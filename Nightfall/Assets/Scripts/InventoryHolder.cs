using UnityEngine;
using UnityEngine.Events;

public class InventoryHolder : MonoBehaviour
{
    [SerializeField] private int primaryInventorySize;
    [SerializeField] protected Inventory primaryInventory;

    public static UnityAction<Inventory> OnDynamicInventoryDisplayRequested;

    public Inventory PrimaryInventory => primaryInventory;

    public Inventory SecondaryInventory { get; internal set; }

    protected virtual void Awake()
    {
        if (PrimaryInventory != null && primaryInventory.InventorySlots != null && primaryInventory.InventorySlots.Count > 0)
        {
            int missing = primaryInventorySize  - primaryInventory.InventorySlots.Count;

            for (int i = 0; i < missing; i++)
            {
                primaryInventory.InventorySlots.Add(new InventorySlot());
            }
        }
        else
        {
            primaryInventory = new Inventory(primaryInventorySize);
        }
    }
}
