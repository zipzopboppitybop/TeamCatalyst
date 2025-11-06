using UnityEngine;
using UnityEngine.Events;

public class InventoryHolder : MonoBehaviour
{
    [SerializeField] private int inventorySize;
    [SerializeField] protected Inventory inventory;

    public Inventory Inventory => inventory;

    private void Awake()
    {
        inventory = new Inventory(inventorySize);
    }
}
