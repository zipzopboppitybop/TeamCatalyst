using UnityEngine;

public class Chest : InventoryHolder
{
    public void OpenChest()
    {
        OnDynamicInventoryDisplayRequested?.Invoke(primaryInventory);
    }
}