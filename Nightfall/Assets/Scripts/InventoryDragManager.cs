using UnityEngine.UIElements;

public static class InventoryDragManager
{
    public static InventorySlot draggedSlot;
    public static VisualElement draggedIcon;
    public static Inventory draggedFromInventory;
    public static VisualElement rootPanel;

    public static bool IsDragging => draggedSlot != null && draggedIcon != null;

    public static void BeginDrag(InventorySlot slot, VisualElement icon, Inventory sourceInventory, VisualElement root)
    {
        draggedSlot = slot;
        draggedIcon = icon;
        draggedFromInventory = sourceInventory;
        rootPanel = root;
        root.Add(icon);
    }

    public static void EndDrag()
    {
        if (draggedIcon != null && rootPanel != null)
        {
            rootPanel.Remove(draggedIcon);
        }

        draggedSlot = null;
        draggedIcon = null;
        draggedFromInventory = null;
        rootPanel = null;
    }
}