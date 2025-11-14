using Catalyst.GamePlay;
using UnityEngine;

public class Chest : InventoryHolder, IInteractable
{
    public void Interact()
    {
        OnDynamicInventoryDisplayRequested?.Invoke(primaryInventory);
    }
}