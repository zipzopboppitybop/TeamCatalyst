using UnityEngine;
using Catalyst.Player;

public class InventoryDisplayController : MonoBehaviour
{
    [SerializeField] private PlayerInventoryUI playerInventoryUI;
    [SerializeField] private PlayerInventoryUI chestInventoryUI;
    [SerializeField] private GameObject player;

    bool chestOpen;
    private Catalyst.Player.PlayerController playerController;
    private float lastChestOpenTime;

    private void Start()
    {
        chestInventoryUI.Show(false); 
        playerInventoryUI.Show(false);
        playerController = player.GetComponent<Catalyst.Player.PlayerController>();
    }

    void Update()
    {
        if (chestOpen && Input.GetKeyDown(KeyCode.E) && Time.time - lastChestOpenTime > 0.2f ||
            Input.GetKeyDown(KeyCode.Escape)) 
        {
            CloseChest();
        }
    }

    private void OnEnable()
    {
        InventoryHolder.OnDynamicInventoryDisplayRequested += ShowChestInventory;
    }

    private void OnDisable()
    {
        InventoryHolder.OnDynamicInventoryDisplayRequested -= ShowChestInventory;
    }

    private void ShowChestInventory(Inventory chestInventory)
    {
        if (chestOpen)
        {
            return;
        }
        chestOpen = true;
        lastChestOpenTime = Time.time;

        chestInventoryUI.isChestUI = true; 
        chestInventoryUI.SetInventory(chestInventory);
        chestInventoryUI.Show(true);
        playerInventoryUI.Show(true);

        playerController.isInventoryOpen = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseChest()
    {
        if (!chestOpen)
        {
            return;
        }

        chestInventoryUI.Show(false);
        chestInventoryUI.SetInventory(null); 
        chestInventoryUI.isChestUI = true;

        chestOpen = false;

        playerInventoryUI.Show(false);
        playerController.isInventoryOpen = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}