using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IPickup
{
    [SerializeField] CharacterController controller;

    [SerializeField] int health;
    [SerializeField] int speed;
    [SerializeField] int sprintMod;
    [SerializeField] int jumpSpeed;
    [SerializeField] int jumpCountMax;
    [SerializeField] int gravity;

    private Vector3 moveDir;
    private Vector3 playerVel;

    bool isSprinting;
    bool isJumping;
    int maxHealth;
    int jumpCount;

    InventoryHolder inventoryHolder;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        maxHealth = health;
        inventoryHolder = GetComponent<InventoryHolder>();
    }

    // Update is called once per frame
    void Update()
    {
        Movement();

        Sprint();
    }

    void Movement()
    {
        if (controller.isGrounded)
        {
            playerVel = Vector3.zero;
            jumpCount = 0;
        }
        else
        {
            playerVel.y -= gravity * Time.deltaTime;
        }

        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        controller.Move(moveDir* speed * Time.deltaTime);

        Jump();
        controller.Move(playerVel * Time.deltaTime);
    }

    void Sprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            speed *= sprintMod;
            isSprinting = true;
        }
        else if (Input.GetButtonUp("Sprint"))
        {
            speed /= sprintMod;
            isSprinting = false;
        }
    }
    void Jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpCountMax)
        {
            playerVel.y = jumpSpeed;
            jumpCount++;
        }
    }

    public bool AddToInventory(ItemData item, int amountToAdd)
    {
        Inventory playerInventory = inventoryHolder.PrimaryInventory;
        List<InventorySlot> slots = playerInventory.InventorySlots;

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlot slot = slots[i];
            if (slot.ItemData == item)
            {
                if (slot.RoomLeftInStack(amountToAdd))
                {
                    slot.AddToStack(amountToAdd);
                    playerInventory.OnInventorySlotChanged?.Invoke(slot);
                    return true;
                }
            }
        }

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlot slot = slots[i];
            if (slot.ItemData == null)
            {
                slot.UpdateInventorySlot(item, amountToAdd);
                playerInventory.OnInventorySlotChanged?.Invoke(slot);
                return true;
            }
        }

        return false;
    }
}

