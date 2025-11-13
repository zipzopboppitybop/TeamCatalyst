using System.Collections;
using System.IO;
using Catalyst.CameraController;
using Catalyst.GamePlay;
using Unity.Cinemachine;
using UnityEngine;

namespace Catalyst.Player
{
    public class PlayerController : MonoBehaviour, IDamage
    {
        [Header("References")]

        [SerializeField] private Animator animator;
        [SerializeField] CamController camController;

        [SerializeField] public PlayerInventoryUI hotbar;
        public AudioSource aud;



        public InputHandler playerInputHandler;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private LayerMask ignoreLayer;

        private Vector3 _currentMovement;


        public bool isInventoryOpen;



        [Header("Animation")]
        private int _animJump;
        private int _animGrounded;
        private int _animAttack;
        private int _animDash;

        private float _velocityX;
        private float _velocityZ;
        private int _animVelocityX;
        private int _animVelocityZ;

        private int _jumpCount = 0;

        private CharacterController characterController;
        private Vector3 playerDir;


        private void Start()
        {

            characterController = GetComponent<CharacterController>();

            SetupAnimator();



        }


        private float CurrentSpeed => playerData.Speed * (playerInputHandler.SprintHeld ? playerData.SprintSpeed : 1);


        void Update()
        {

            HandleMovement();


            HandleAttack();
            HandleDash();
            UpdateInteract();


        }

        private void SetupAnimator()
        {

            _animJump = Animator.StringToHash("Jump");
            _animGrounded = Animator.StringToHash("Grounded");
            _animAttack = Animator.StringToHash("Attack");
            _animDash = Animator.StringToHash("Dash");
            _animVelocityX = Animator.StringToHash("Velocity X");
            _animVelocityZ = Animator.StringToHash("Velocity Z");




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
                StartCoroutine(Attack());
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

                animator.SetBool(_animGrounded, true);

                if (playerInputHandler.JumpTriggered && _jumpCount < playerData.JumpMax)
                {

                    _currentMovement.y = playerData.JumpForce;

                    StartCoroutine(Jump());
                    _jumpCount++;

                }


            }

            else if (!characterController.isGrounded)
            {

                _currentMovement.y += Physics.gravity.y * playerData.GravityMultiplier * Time.deltaTime;
                animator.SetBool(_animGrounded, false);
                animator.ResetTrigger(_animJump);

            }
        }

        private void HandleDash()
        {             // Dash logic here
            if (playerInputHandler.DashTriggered)
            {
                StartCoroutine(Dash());
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

            _currentMovement.x = playerDir.x * CurrentSpeed;
            _currentMovement.z = playerDir.z * CurrentSpeed;
            HandleJumping();

            characterController.Move(_currentMovement * Time.deltaTime);


            animator.SetFloat(_animVelocityX, Mathf.SmoothDamp(animator.GetFloat(_animVelocityX), playerInputHandler.MoveInput.x * _currentMovement.magnitude, ref _velocityX, 0.1f));
            animator.SetFloat(_animVelocityZ, Mathf.SmoothDamp(animator.GetFloat(_animVelocityZ), playerInputHandler.MoveInput.y * _currentMovement.magnitude, ref _velocityZ, 0.1f));
        }












        public void UpdateInteract()
        {
            if (playerInputHandler.InteractTriggered)
            {
                Vector3 origin = camController.FPSCamera.transform.position;
                Vector3 direction = camController.FPSCamera.transform.forward;

                Debug.DrawRay(origin, direction * playerData.InteractRange, Color.red, 1f);

                if (Physics.Raycast(origin, direction, out RaycastHit hit, playerData.InteractRange, ~ignoreLayer))
                {
                    Chest chest = hit.collider.GetComponent<Chest>();
                    chest?.OpenChest();

                    IInteractable target = hit.collider.GetComponent<IInteractable>();

                    target?.Interact();
                }

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




        IEnumerator FlashDamageScreen()
        {
            //HUDManager.instance.playerDamageScreen.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            //HUDManager.instance.playerDamageScreen.SetActive(false);
        }




        IEnumerator Attack()
        {

            animator.SetTrigger(_animAttack);
            yield return new WaitForSeconds(1.0f);
            animator.ResetTrigger(_animAttack);
            playerInputHandler.AttackTriggered = false;
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

        IEnumerator Dash()
        {
            animator.SetTrigger(_animDash);

            yield return new WaitForSeconds(1.0f);
            animator.ResetTrigger(_animDash);
            playerInputHandler.DashTriggered = false;
        }


        IEnumerator Jump()
        {
            animator.SetTrigger(_animJump);
            yield return new WaitForSeconds(0.5f);
            animator.ResetTrigger(_animJump);
            playerInputHandler.JumpTriggered = false;
        }

        public void takeDamage(int amount)
        {
            playerData.Health -= amount;

            /*if (isLowHealth && !InfoManager.instance.IsInfoShowing())
            {

                InfoManager.instance.ShowMessage("WARNING!", "Health Critical!", Color.red, 2);
            }

            UpdatePlayerHealthBarUI();*/

            StartCoroutine(FlashDamageScreen());

            if (playerData.Health <= 0)
            {
                GameManager.instance.YouLose();
                playerData.Health = playerData.HealthMax;
            }
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


    }
}



