using System.Collections;
using System.IO;
using Catalyst.GamePlay;
using Unity.Cinemachine;
using UnityEngine;

namespace Catalyst.Player
{
    public class PlayerController : MonoBehaviour, IDamage
    {
        [Header("References")]

        [SerializeField] private Animator animator;
        public Camera sceneCam;
        public CinemachineCamera FPSCamera;
        public CinemachineCamera thirdPersonCamera;
        public CinemachineCamera AimCamera;

        public Camera gunCam;
        [SerializeField] private Transform followTarget;
        [SerializeField] private LayerMask ignoreLayer;
        [SerializeField] private LayerMask gunCamLayers;
        [SerializeField] private LayerMask thirdPersonLayers;
        [SerializeField] private GameObject[] hideInFPS;
        [SerializeField] public PlayerInventoryUI hotbar;
        public AudioSource aud;

        private float _cinemachineTargetPitch;
        private float _cinemachineTargetYaw;

        public InputHandler playerInputHandler;
        [SerializeField] private PlayerData playerData;

        private Vector3 _currentMovement;
        private float _verticalRotation;
        public bool isInverted;
        public bool isInventoryOpen;

        private float _mouseXRotation;
        private float _mouseYRotation;
        private float _velocityX;
        private float _velocityZ;

        [Header("Animation")]
        private int _animJump;
        private int _animGrounded;
        private int _animAttack;
        private int _animDash;

        private int _animVelocityX;
        private int _animVelocityZ;

        private int _jumpCount = 0;

        private CharacterController characterController;
        private Vector3 playerDir;


        private void Start()
        {

            characterController = GetComponent<CharacterController>();

            SetupAnimator();
            thirdPersonCamera.gameObject.SetActive(false);
            AimCamera.gameObject.SetActive(false);
            StartCoroutine(ToggleCullingLayer(0.5f));


        }

        private void LateUpdate()
        {
            //if (ThirdPersonActive())
            ApplyThirdPersonRotation(_mouseYRotation, _mouseXRotation);
            ToggleAimCamera(playerInputHandler.AimHeld);



        }

        private float CurrentSpeed => playerData.Speed * (playerInputHandler.SprintHeld ? playerData.SprintSpeed : 1);


        void Update()
        {

            HandleMovement();
            HandleRotation();

            HandleAttack();
            HandleDash();
            UpdateInteract();
            ThirdPersonActive();

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
        private bool ThirdPersonActive()
        {

            if (playerInputHandler.ToggleCameraTriggered)
            {
                StartCoroutine(ToggleCamera(thirdPersonCamera));
            }
            return thirdPersonCamera.gameObject.activeSelf;
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

        private void ApplyHorizontalRotation(float rotationAmount)
        {
            if (ThirdPersonActive() && playerInputHandler.MoveInput.magnitude < 0.1)
            {
                return;
            }
            else if (ThirdPersonActive() && playerInputHandler.MoveInput.magnitude >= 0.1)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, followTarget.eulerAngles.y, 0), Time.deltaTime * playerData.CameraRotationSpeed * playerData.RotationSpeed);

            }

            else
                transform.Rotate(0, rotationAmount, 0);
        }


        private void ApplyVerticalRotation(float rotationAmount)
        {


            if (isInverted)
            {
                _verticalRotation = Mathf.Clamp(_verticalRotation + rotationAmount, -playerData.FPSVerticalRange, playerData.FPSVerticalRange);
            }
            else if (!isInverted)
            {
                _verticalRotation = Mathf.Clamp(_verticalRotation - rotationAmount, -playerData.FPSVerticalRange, playerData.FPSVerticalRange);
            }

            FPSCamera.transform.localRotation = Quaternion.Euler(_verticalRotation, 0, 0);
        }



        private void HandleRotation()
        {
            if (isInventoryOpen)
            {
                return;
            }

            _mouseXRotation = playerInputHandler.RotationInput.x * playerData.MouseSensitivity * playerData.CameraRotationSpeed;
            _mouseYRotation = playerInputHandler.RotationInput.y * playerData.MouseSensitivity;

            ApplyHorizontalRotation(_mouseXRotation);
            ApplyVerticalRotation(_mouseYRotation);
        }

        private void ApplyThirdPersonRotation(float pitch, float yaw)

        {

            if (isInverted)
            {
                //Debug.Log("Applying INVERTED third person rotation");
                _cinemachineTargetPitch = UpdateRotation(_cinemachineTargetPitch, -pitch, -playerData.DownLookRange, playerData.UpLookRange, true);

            }
            else if (!isInverted)
            {

                //Debug.Log("Applying third person rotation");
                _cinemachineTargetPitch = UpdateRotation(_cinemachineTargetPitch, pitch, -playerData.DownLookRange, playerData.UpLookRange, true);
            }

            _cinemachineTargetYaw = UpdateRotation(_cinemachineTargetYaw, yaw, float.MinValue, float.MaxValue, false);
            followTarget.rotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, followTarget.eulerAngles.z);

        }

        private float UpdateRotation(float currentRotation, float input, float min, float max, bool isXAxis)
        {
            currentRotation += isXAxis ? -input : input;
            return Mathf.Clamp(currentRotation, min, max);
        }

        public void UpdateInteract()
        {
            if (playerInputHandler.InteractTriggered)
            {
                Vector3 origin = FPSCamera.transform.position;
                Vector3 direction = FPSCamera.transform.forward;

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

        private void ToggleGunCam()
        {
            if (gunCam == null)
                return;
            if (thirdPersonCamera.gameObject.activeSelf)
            {
                gunCam.enabled = false;

            }

            else
            {
                gunCam.enabled = true;
                Debug.Log("Gun cam enabled");
                // enable ignore layers on main camera 

            }
            StartCoroutine(ToggleCullingLayer(1f));
        }

        IEnumerator ToggleCullingLayer(float time)
        {
            if (FPSCamera == null)
                yield break;

            if (thirdPersonCamera.gameObject.activeSelf)
            {
                yield return new WaitForEndOfFrame();
                sceneCam.cullingMask |= thirdPersonLayers;
                Debug.Log("Adding culling layers to main camera");

                foreach (GameObject go in hideInFPS)
                {
                    go.SetActive(true);
                }

                if (playerData.AmmoCount > 0)
                {
                    sceneCam.cullingMask |= ~gunCamLayers;
                }
            }
            else if (gunCam.enabled)
            {
                // Wait until camera blend is done for sure
                yield return new WaitForSeconds(time);

                sceneCam.cullingMask &= ~thirdPersonLayers;
                sceneCam.cullingMask &= ~gunCamLayers;

                foreach (GameObject go in hideInFPS)
                {
                    go.SetActive(false);
                }

            }
        }


        IEnumerator FlashDamageScreen()
        {
            //HUDManager.instance.playerDamageScreen.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            //HUDManager.instance.playerDamageScreen.SetActive(false);
        }

        IEnumerator ToggleCamera(CinemachineCamera cam)
        {
            // Stop everything a bit to avoid multiple toggles      
            playerInputHandler.enabled = false;
            cam.gameObject.SetActive(!cam.gameObject.activeSelf);
            ToggleGunCam();
            yield return new WaitForSeconds(1.0f);
            playerInputHandler.enabled = true;
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
        public void ToggleAimCamera(bool isAiming)
        {
            if (AimCamera == null)
                return;
            if (ThirdPersonActive())
                AimCamera.gameObject.SetActive(isAiming);
        }

    }
}



