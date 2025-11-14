using UnityEngine;
using Catalyst.Player;

public class InventoryDisplayController : MonoBehaviour
{
    [SerializeField] private PlayerInventoryUI playerInventoryUI;
    [SerializeField] private PlayerInventoryUI chestInventoryUI;

    private Catalyst.Player.PlayerController playerController;
    private bool chestOpen = false;
    private float lastChestOpenTime = 0f;
    private Inventory currentChestInventory = null;

    private void Start()
    {
        playerController = GameManager.instance.player.GetComponent<Catalyst.Player.PlayerController>();
        playerInventoryUI.Show(false);
        chestInventoryUI.Show(false);
        chestOpen = false;
        currentChestInventory = null;
    }

    private void OnEnable()
    {
        InventoryHolder.OnDynamicInventoryDisplayRequested += ShowChestInventory;
    }

    private void OnDisable()
    {
        InventoryHolder.OnDynamicInventoryDisplayRequested -= ShowChestInventory;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (!chestOpen)
            {
                bool show = !playerInventoryUI.IsVisible();
                playerInventoryUI.Show(show);
                playerController.isInventoryOpen = show;
                Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = show;
            }
        }

        if (chestOpen && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape)))
        {
            if (Time.time - lastChestOpenTime > 0.15f)
            {
                CloseChest();
            }
        }
    }

    private void ShowChestInventory(Inventory chestInventory)
    {
        if (chestOpen) return;

        if (Time.time - lastChestOpenTime < 0.2f)
        {
            return;
        }
        chestOpen = true;
        lastChestOpenTime = Time.time;
        currentChestInventory = chestInventory;

       // chestInventoryUI.isChestUI = true;
        chestInventoryUI.SetInventory(chestInventory);
        chestInventoryUI.Show(true);
        playerInventoryUI.Show(true);
        playerController.isInventoryOpen = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseChest()
    {
        if (!chestOpen) return;

        chestInventoryUI.Show(false);
        chestInventoryUI.SetInventory(null);
        //chestInventoryUI.isChestUI = true;

        chestOpen = false;
        lastChestOpenTime = Time.time;

        playerInventoryUI.Show(false);
        playerController.isInventoryOpen = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}