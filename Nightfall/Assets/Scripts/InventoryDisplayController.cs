using UnityEngine;
using Catalyst.Player;

public class InventoryDisplayController : MonoBehaviour
{
    [SerializeField] private PlayerInventoryUI playerInventoryUI;
    [SerializeField] private PlayerInventoryUI chestInventoryUI;
    [SerializeField] private GameObject player;

    private Catalyst.Player.PlayerController playerController;
    private bool chestOpen = false;
    private float lastChestOpenTime = 0f;
    private Inventory currentChestInventory = null;

    private void Start()
    {
        playerController = player.GetComponent<Catalyst.Player.PlayerController>();
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
        // Player inventory toggle via I — ONLY if NO chest is open.
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log($"[IDC] Key I pressed. chestOpen={chestOpen}");
            if (!chestOpen)
            {
                bool show = !playerInventoryUI.IsVisible();
                Debug.Log($"[IDC] Toggling player inventory -> {show}");
                playerInventoryUI.Show(show);
                playerController.isInventoryOpen = show;
                Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = show;
            }
            else
            {
                Debug.Log("[IDC] I pressed but chestOpen==true -> ignoring.");
            }
        }

        // Close chest with E or Escape (only if chest open)
        if (chestOpen && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape)))
        {
            if (Time.time - lastChestOpenTime > 0.15f)
            {
                Debug.Log("[IDC] E/Escape pressed -> CloseChest()");
                CloseChest();
            }
            else
            {
                Debug.Log("[IDC] E/Escape pressed too soon after open -> ignored.");
            }
        }
    }

    // This is called by InventoryHolder.OnDynamicInventoryDisplayRequested
    private void ShowChestInventory(Inventory chestInventory)
    {
        Debug.Log($"[IDC] ShowChestInventory called. chestOpen={chestOpen} chestInventory==null? {chestInventory == null}");

        if (chestOpen)
        {
            Debug.Log("[IDC] ShowChestInventory: chest already open -> returning.");
            return;
        }

        if (chestInventory == null)
        {
            Debug.LogWarning("[IDC] ShowChestInventory: passed null chestInventory -> ignoring.");
            return;
        }

        chestOpen = true;
        lastChestOpenTime = Time.time;
        currentChestInventory = chestInventory;

        // set up chest UI
        chestInventoryUI.isChestUI = true;
        chestInventoryUI.SetInventory(chestInventory);
        chestInventoryUI.Show(true);

        // ensure player inventory shows too
        playerInventoryUI.Show(true);
        playerController.isInventoryOpen = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("[IDC] ShowChestInventory: chest and player inventory shown.");
    }

    public void CloseChest()
    {
        if (!chestOpen) return;

        Debug.Log("[IDC] Closing chest: clearing chest UI and inventory reference.");

        chestInventoryUI.Show(false);
        chestInventoryUI.SetInventory(null); // <--- clear the chest inventory
        chestInventoryUI.isChestUI = true;

        chestOpen = false;

        playerInventoryUI.Show(false);
        playerController.isInventoryOpen = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("[IDC] CloseChest complete.");
    }
}