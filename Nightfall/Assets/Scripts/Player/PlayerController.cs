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




        [SerializeField] public PlayerInventoryUI playerInventory;
        public AudioSource aud;
        [SerializeField] private TilePainter tilePainter;
        public InputHandler playerInputHandler;
        [SerializeField] private PlayerData farmerData;
        [SerializeField] private LayerMask ignoreLayer;

        private Vector3 _currentMovement;
        private CamController camController;

        private AnimationHandler anim;

        public bool isInventoryOpen;





        private int _jumpCount = 0;

        private CharacterController characterController;
        private Vector3 playerDir;

        private void Awake()
        {
            aud = GetComponent<AudioSource>();
            //GameManager.instance.playerController = this;
        }

        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            camController = GetComponent<CamController>();
            anim = GetComponent<AnimationHandler>();
            playerInputHandler = GetComponent<InputHandler>();

        }

        private float CurrentSpeed => farmerData.Speed * (playerInputHandler.SprintHeld ? farmerData.SprintSpeed : 1);


        void Update()
        {
            if (!GameManager.instance.isPaused && !isInventoryOpen)
            {
                HandleMovement();
                HandleAttack();
                HandleDash();
                UpdateInteract();
            }
            else
            {
                StopAllPlayerMovement();
            }
        }



        public void EnablePlayerInput(bool enabled)
        {
            playerInputHandler.enabled = enabled;
        }
        public void StopAllPlayerMovement()
        {
            _currentMovement = Vector3.zero;
            anim.UpdateMoveAnimations(Vector3.zero);
        }
        private void HandleAttack()
        {

            if (isInventoryOpen)
            {
                return;
            }

            if (playerInputHandler.AttackTriggered)
            {
                if (farmerData.isAttacking || farmerData.isAiming)
                    return;

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

                if (playerInputHandler.JumpTriggered && _jumpCount < farmerData.JumpMax)
                {

                    _currentMovement.y = farmerData.JumpForce;
                    anim.SetGrounded(false);
                    anim.TriggerJump();
                    _jumpCount++;

                }


            }

            else if (!characterController.isGrounded)
            {

                _currentMovement.y += Physics.gravity.y * farmerData.GravityMultiplier * Time.deltaTime;
                anim.SetGrounded(false);
                //anim.ResetTrigger(_animJump);

            }
        }

        private void HandleDash()
        {             // Dash logic here
            if (!characterController.isGrounded)
                return;

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
            if (isInventoryOpen || (playerInventory != null && playerInventory.isChestOpen) || GameManager.instance.isPaused)
            {
                return;
            }

            if (playerInputHandler.InteractTriggered)
            {
                Vector3 origin = camController.MainCamera.transform.position;
                Vector3 direction = camController.MainCamera.transform.forward;

                Debug.DrawRay(origin, direction * farmerData.InteractRange, Color.green, 2f);
                // Raycast to check for interactable objects in a half circle range
                if (Physics.SphereCast(origin, .25f, direction, out RaycastHit hit, farmerData.InteractRange, ~ignoreLayer))
                {
                    IInteractable target = hit.collider.GetComponent<IInteractable>();
                    if (target != null)
                    {
                        target.Interact();
                        return;
                    }
                }



                //if (Physics.Raycast(origin, direction, out RaycastHit hit, playerData.InteractRange, ~ignoreLayer))
                //{
                //    Chest chest = hit.collider.GetComponent<Chest>();
                //    chest?.OpenChest();
                //    IInteractable target = hit.collider.GetComponent<IInteractable>();
                //    target?.Interact();
                //}

                if (tilePainter != null)
                {
                    ItemData heldItem = playerInventory?.GetSelectedItem();
                    if (heldItem != null)
                        tilePainter.TryPlaceTile(heldItem.dropPrefab);

                    tilePainter.TryHarvestCrop();
                }
            }
        }

        public PlayerInventoryUI GetHotBar()
        {
            return playerInventory;
        }

        public void PlayerDash()
        {
            Vector3 dodgeDirection = CalculateMoveDirection();

            if (dodgeDirection == Vector3.zero)
            {
                dodgeDirection = -transform.forward;
            }
            // Dont include forwward movement in dash




            if (dodgeDirection.y < 0)
                dodgeDirection.y = -dodgeDirection.y;

            playerInputHandler.enabled = false;
            // Disable player input for the dash duration

            StopAllPlayerMovement();
            characterController.Move(dodgeDirection.normalized * farmerData.DashSpeed * Time.deltaTime);
            StartCoroutine(EnableInputAfterDelay(0.2f));
        }


        IEnumerator EnableInputAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            playerInputHandler.enabled = true;
        }



        public void takeDamage(int amount)
        {
            farmerData.Health -= amount;

            /*if (isLowHealth && !InfoManager.instance.IsInfoShowing())
            {

                InfoManager.instance.ShowMessage("WARNING!", "Health Critical!", Color.red, 2);
            }

            UpdatePlayerHealthBarUI();*/

            //StartCoroutine(FlashDamageScreen());

            if (farmerData.Health <= 0)
            {
                GameManager.instance.YouLose();
                farmerData.Health = farmerData.HealthMax;
            }
        }
        public void PlayHurtSound()
        {
            // Play hurt sound with pitch variation
            if (aud != null && farmerData.HurtSound.Length > 0)
            {
                int index = Random.Range(0, farmerData.HurtSound.Length);
                aud.pitch = 2f + Random.Range(-farmerData.PitchVariation, farmerData.PitchVariation);
                aud.PlayOneShot(farmerData.HurtSound[index], farmerData.MovementVolume);
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
            float healRate = farmerData.HealthRegen;

            while (healAmount > 0)
            {
                farmerData.Health += healRate * Time.deltaTime;
                healAmount -= healRate * Time.deltaTime;

                if (farmerData.Health > farmerData.HealthMax)
                {
                    farmerData.Health = farmerData.HealthMax;
                    break;
                }
                yield return null;
            }
        }
        public void PlayRandomFootstep()
        {
            if (aud != null && farmerData.FootstepSounds.Length > 0)
            {
                int index = Random.Range(0, farmerData.FootstepSounds.Length);
                aud.pitch = 1f + Random.Range(-farmerData.PitchVariation, farmerData.PitchVariation);
                aud.PlayOneShot(farmerData.FootstepSounds[index], farmerData.MovementVolume);
            }
        }

        public void PlayJumpSound()
        {
            // Play jump sound with pitch variation
            if (aud != null && farmerData.JumpSound.Length > 0)
            {
                int index = Random.Range(0, farmerData.JumpSound.Length);
                aud.pitch = 1f + Random.Range(-farmerData.PitchVariation, farmerData.PitchVariation);
                aud.PlayOneShot(farmerData.JumpSound[index], farmerData.MovementVolume);
            }

        }

        public void PlayLandSound()
        {
            // Play land sound with pitch variation
            if (aud != null && farmerData.LandSound.Length > 0)
            {
                int index = Random.Range(0, farmerData.LandSound.Length);
                aud.pitch = 1f + Random.Range(-farmerData.PitchVariation, farmerData.PitchVariation);
                aud.PlayOneShot(farmerData.LandSound[index], farmerData.MovementVolume);
            }
        }
        public void PlayDashSOund()
        {
            // Play dash sound with pitch variation
            if (aud != null && farmerData.DashSound.Length > 0)
            {
                int index = Random.Range(0, farmerData.DashSound.Length);
                aud.pitch = 1f + Random.Range(-farmerData.PitchVariation, farmerData.PitchVariation);
                aud.PlayOneShot(farmerData.DashSound[index], farmerData.MovementVolume);
            }
        }

        public void PlayJumpTrailSound()
        {
            // Play jump trail sound with pitch variation
            if (aud != null && farmerData.JumpTrailSound.Length > 0)
            {
                int index = Random.Range(0, farmerData.JumpTrailSound.Length);
                aud.pitch = 1f + Random.Range(-farmerData.PitchVariation, farmerData.PitchVariation);
                aud.PlayOneShot(farmerData.JumpTrailSound[index], farmerData.MovementVolume);
            }
        }

        public void Save(ref PlayerSaveData data)
        {
            data.Position = transform.position;
            data.playerData = farmerData;
        }

        public void Load(ref PlayerSaveData data)
        {
            characterController.enabled = false;
            transform.position = data.Position;
            farmerData = data.playerData;
            characterController.enabled = true;
        }

        public PlayerData GetPlayerData()
        {
            return farmerData;
        }

    }

    [System.Serializable]
    public struct PlayerSaveData
    {
        public Vector3 Position;
        public PlayerData playerData;
    }


}
