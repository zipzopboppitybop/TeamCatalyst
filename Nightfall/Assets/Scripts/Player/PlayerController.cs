using System.Collections;
using Catalyst.CameraController;
using Catalyst.GamePlay;
using Catalyst.Player.Handlers;
using UnityEngine;

namespace Catalyst.Player
{
    public class PlayerController : MonoBehaviour, IDamage
    {
        [Header("References")]




        [SerializeField] public PlayerInventoryUI hotbar;
        public AudioSource aud;

        public InputHandler playerInputHandler;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private LayerMask ignoreLayer;

        private Vector3 _currentMovement;
        private CamController camController;

        private AnimationHandler anim;

        public bool isInventoryOpen;





        private int _jumpCount = 0;

        private CharacterController characterController;
        private Vector3 playerDir;

        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            camController = GetComponent<CamController>();
            anim = GetComponent<AnimationHandler>();
            playerInputHandler = GetComponent<InputHandler>();

        }

        private float CurrentSpeed => playerData.Speed * (playerInputHandler.SprintHeld ? playerData.SprintSpeed : 1);


        void Update()
        {
            if (!GameManager.instance.isPaused)
            {

                HandleMovement();
                HandleAttack();
                HandleDash();
                UpdateInteract();

            }
        }



        public void EnablePlayerInput(bool enabled)
        {
            playerInputHandler.enabled = enabled;
        }

        private void HandleAttack()
        {

            if (isInventoryOpen)
            {
                return;
            }

            if (playerInputHandler.AttackTriggered)
            {
                anim.TriggerAttack();
            }

        }

        private Vector3 CalculateMoveDirection()
        {
            Vector3 inputDirection = new Vector3(playerInputHandler.MoveInput.x, 0f, playerInputHandler.MoveInput.y);

            Vector3 moveDirection = transform.TransformDirection(inputDirection);

            return moveDirection.normalized;
        }

        private void HandleJumping()
        {
            if (characterController.isGrounded)
            {
                _jumpCount = 0;

                _currentMovement.y = -1f; // Small downward force to keep the player grounded

                anim.SetGrounded(true);

                if (playerInputHandler.JumpTriggered && _jumpCount < playerData.JumpMax)
                {

                    _currentMovement.y = playerData.JumpForce;

                    anim.TriggerJump();
                    _jumpCount++;

                }


            }

            else if (!characterController.isGrounded)
            {

                _currentMovement.y += Physics.gravity.y * playerData.GravityMultiplier * Time.deltaTime;
                anim.SetGrounded(false);
                //animator.ResetTrigger(_animJump);

            }
        }

        private void HandleDash()
        {             // Dash logic here
            if (playerInputHandler.DashTriggered)
            {
                anim.TriggerDash();
                PlayerDash();
            }
        }


        private void HandleMovement()
        {
            if (isInventoryOpen)
            {
                return;
            }


            playerDir = CalculateMoveDirection();

            if (playerInputHandler.AimHeld)
                _currentMovement.x = playerDir.x * (CurrentSpeed * 0.5f);

            else
                _currentMovement.x = playerDir.x * CurrentSpeed;


            _currentMovement.z = playerDir.z * CurrentSpeed;
            HandleJumping();

            characterController.Move(_currentMovement * Time.deltaTime);

            anim.UpdateMoveAnimations(_currentMovement);
        }

        public void UpdateInteract()
        {
            if (playerInputHandler.InteractTriggered)
            {
                Vector3 origin = camController.FPSCamera.transform.position;
                Vector3 direction = camController.FPSCamera.transform.forward;

                Debug.DrawRay(origin, direction * playerData.InteractRange, Color.green, 2f);
                // Raycast to check for interactable objects in a half circle range
                if (Physics.SphereCast(origin, playerData.InteractRange, direction, out RaycastHit hit, playerData.InteractRange, ~ignoreLayer))
                {
                    Chest chest = hit.collider.GetComponent<Chest>();
                    chest?.OpenChest();
                    IInteractable target = hit.collider.GetComponent<IInteractable>();
                    target?.Interact();
                }



                //if (Physics.Raycast(origin, direction, out RaycastHit hit, playerData.InteractRange, ~ignoreLayer))
                //{
                //    Chest chest = hit.collider.GetComponent<Chest>();
                //    chest?.OpenChest();

                //    IInteractable target = hit.collider.GetComponent<IInteractable>();

                //    target?.Interact();
                //}

                TilePainter painter = FindAnyObjectByType<TilePainter>();
                if (painter != null)
                {
                    ItemData heldItem = hotbar?.GetSelectedItem();
                    if (heldItem != null && heldItem.dropPrefab != null)
                    {
                        painter.TryPlaceTile(heldItem.dropPrefab);
                    }
                }
            }
        }

        public PlayerInventoryUI GetHotBar()
        {
            return hotbar;
        }

        public void PlayerDash()
        {
            Vector3 dodgeDirection = CalculateMoveDirection();
            if (dodgeDirection == Vector3.zero)
            {
                dodgeDirection = -transform.forward;
            }
            characterController.Move(dodgeDirection.normalized * playerData.DashSpeed * Time.deltaTime);
        }






        public void takeDamage(int amount)
        {
            playerData.Health -= amount;

            /*if (isLowHealth && !InfoManager.instance.IsInfoShowing())
            {

                InfoManager.instance.ShowMessage("WARNING!", "Health Critical!", Color.red, 2);
            }

            UpdatePlayerHealthBarUI();*/

            //StartCoroutine(FlashDamageScreen());

            //if (playerData.Health <= 0)
            //{
            //    GameManager.instance.YouLose();
            //    playerData.Health = playerData.HealthMax;
            //}
        }

        public void Heal(int amount)
        {
            StartCoroutine(HealPlayer(amount));

        }

        IEnumerator HealPlayer(int amount)
        {
            // Lerp the health increase over time base off playerdata health regen rate
            float healAmount = amount;
            float healRate = playerData.HealthRegen;

            while (healAmount > 0)
            {
                playerData.Health += healRate * Time.deltaTime;
                healAmount -= healRate * Time.deltaTime;

                if (playerData.Health > playerData.HealthMax)
                {
                    playerData.Health = playerData.HealthMax;
                    break;
                }
                yield return null;
            }
        }


        public void PlayRandomFootstep()
        {
            if (aud != null && playerData.FootstepSounds.Length > 0)
            {
                int index = Random.Range(0, playerData.FootstepSounds.Length);
                aud.PlayOneShot(playerData.FootstepSounds[index], playerData.FootstepVolume);
            }
        }

    }
}



