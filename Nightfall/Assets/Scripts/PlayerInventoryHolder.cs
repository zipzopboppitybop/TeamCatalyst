using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventoryHolder : InventoryHolder, IPickup
{
    [SerializeField] protected int secondaryInventorySize = 16;
    [SerializeField] protected Inventory secondaryInventory;
    [SerializeField] private PlayerInventoryUI hotbarUI;

    public new Inventory SecondaryInventory => secondaryInventory;

    protected override void Awake()
    {
        base.Awake();
        secondaryInventory = new Inventory(secondaryInventorySize);
    }

    void Update()
    {
        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            OnDynamicInventoryDisplayRequested?.Invoke(secondaryInventory);
        }
    }

    public bool AddToInventory(ItemData item, int amountToAdd)
    {
        if (TryAddToSpecificInventory(PrimaryInventory, item, amountToAdd))
        {
            return true;
        }

        if (TryAddToSpecificInventory(SecondaryInventory, item, amountToAdd))
        {
            return true;
        }

        return false;
    }


    private bool TryAddToSpecificInventory(Inventory targetInventory, ItemData item, int amountToAdd)
    {
        List<InventorySlot> slots = targetInventory.InventorySlots;

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlot slot = slots[i];
            if (slot.ItemData == item && slot.RoomLeftInStack(amountToAdd))
            {
                slot.AddToStack(amountToAdd);
                targetInventory.NotifySlotChanged(slot);
                return true;
            }
        }

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlot slot = slots[i];
            if (slot.ItemData == null)
            {
                slot.UpdateInventorySlot(item, amountToAdd);
                targetInventory.NotifySlotChanged(slot);
                return true;
            }
        }

        return false;
    }

    public ItemData GetHeldItem()
    {
        return hotbarUI != null ? hotbarUI.GetSelectedItem() : null;
    }

    public InventorySlot GetHeldSlot()
    {
        return hotbarUI != null ? hotbarUI.GetSelectedSlot() : null;
    }
}