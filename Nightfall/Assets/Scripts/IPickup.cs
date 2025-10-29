using UnityEngine;

public interface IPickup
{
    public bool AddToInventory(ItemData item, int amountToAdd);
}
