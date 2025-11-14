using UnityEngine;
using Catalyst.Player;

public class InventoryDisplayController : MonoBehaviour
{
    //[SerializeField] private PlayerInventoryUI playerInventoryUI;
    //[SerializeField] private PlayerInventoryUI chestInventoryUI;

    //private Catalyst.Player.PlayerController playerController;
    //private bool chestOpen = false;
    //private float lastChestOpenTime = 0f;
    //private Inventory currentChestInventory = null;

    //private void Start()
    //{
    //    playerController = GameManager.instance.player.GetComponent<Catalyst.Player.PlayerController>();
    //    playerInventoryUI.Show(false);
    //    chestInventoryUI.Show(false);
    //    chestOpen = false;
    //    currentChestInventory = null;
    //}

    //private void OnEnable()
    //{
    //    InventoryHolder.OnDynamicInventoryDisplayRequested += ShowChestInventory;
    //}

    //private void OnDisable()
    //{
    //    InventoryHolder.OnDynamicInventoryDisplayRequested -= ShowChestInventory;
    //}

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.I))
    //    {
    //        if (!chestOpen)
    //        {
    //            bool show = !playerInventoryUI.IsVisible();
    //            playerInventoryUI.Show(show);
    //            playerController.isInventoryOpen = show;
    //            Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
    //            Cursor.visible = show;
    //        }
    //    }

    //    if (chestOpen && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape)))
    //    {
    //        if (Time.time - lastChestOpenTime > 0.15f)
    //        {
    //            CloseChest();
    //        }
    //    }
    //}
}