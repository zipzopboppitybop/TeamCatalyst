using System.Collections;
using System.Runtime.CompilerServices;
using Unity.Cinemachine;
using UnityEngine;

namespace Catalyst.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]

        [SerializeField] private Animator animator;
        public CinemachineCamera mainCamera;
        public CinemachineCamera thirdPersonCamera;
        [SerializeField] private Transform followTarget;



        public InputHandler playerInputHandler;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private LayerMask ignoreLayer;
        private Vector3 _currentMovement;
        private float _verticalRotation;
        public bool isInverted;
        public bool isInventoryOpen;



        [Header("Animation")]
        private int _animJump;
        private int _animGrounded;
        private int _animSprinting;
        private int _animAttack;
        private int _animDodge;
        private int _animDash;
        private int _animVelocityX;
        private int _animVelocityZ;

        private int _jumpCount = 0;

        private CharacterController _characterController;
        private Vector3 _playerDir;
        private float _cinemachineTargetPitch;
        private float _cinemachineTargetYaw;
        private float _mouseXRotation;
        private float _mouseYRotation;
        private float _velocityX;
        private float _velocityZ;

        public Vector3 MovementDirection => _playerDir;

        private void Start()
        {
            _characterController = GetComponent<CharacterController>();
            SetupAnimator();
            thirdPersonCamera.gameObject.SetActive(false);

        }

        private void LateUpdate()
        {
            //if (IsThirdPersonActive())
            ApplyThirdPersonRotation(_mouseYRotation, _mouseXRotation);
        }

        //private float CurrentSpeed => playerData.Speed * (playerInputHandler.SprintTriggered ? playerData.SprintMultiplier : 1);

        void Update()
        {
            ThirdPersonActive();
            HandleMovement();
            HandleRotation();

            HandleAttack();
            HandleDodge();
            HandleDash();
            UpdateInteract();


        }

        private void SetupAnimator()
        {

            _animJump = Animator.StringToHash("Jump");
            _animGrounded = Animator.StringToHash("Grounded");
            _animSprinting = Animator.StringToHash("Sprinting");
            _animAttack = Animator.StringToHash("Attack");
            _animDodge = Animator.StringToHash("Dodge");
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
                // Attack logic here
                StartCoroutine(Attack());


            }
            else
            {
                animator.ResetTrigger(_animAttack);
            }

        }

        private float CurrentSpeed()
        {
            if (!_characterController.isGrounded)
            {
                animator.SetBool(_animSprinting, false);
                return playerData.Speed;
            }
            if (playerInputHandler.SprintTriggered)
            {
                animator.SetBool(_animSprinting, true);
                return playerData.SprintSpeed;

            }
            else
            {
                animator.SetBool(_animSprinting, false);
                return playerData.Speed;

            }
        }
        private Vector3 CalculatePlayerDirection()
        {
            Vector3 inputDirection = new Vector3(playerInputHandler.MoveInput.x, 0f, playerInputHandler.MoveInput.y);

            Vector3 playerDirection = transform.TransformDirection(inputDirection);
            return playerDirection.normalized;


        }


        private bool IsThirdPersonActive()
        {
            return thirdPersonCamera.isActiveAndEnabled && playerInputHandler.isActiveAndEnabled;
        }

        private void HandleJumping()
        {
            if (_characterController.isGrounded)
            {
                _jumpCount = 0;

                _currentMovement.y = -1f; // Small downward force to keep the player grounded

                animator.SetBool(_animGrounded, true);

                if (playerInputHandler.JumpTriggered && _jumpCount < playerData.JumpMax)
                {

                    _currentMovement.y = playerData.JumpForce;
                    animator.SetTrigger(_animJump);
                    _jumpCount++;
                    animator.ResetTrigger(_animJump);
                }


            }

            else if (!_characterController.isGrounded)
            {

                _currentMovement.y += Physics.gravity.y * playerData.GravityMultiplier * Time.deltaTime;
                animator.SetBool(_animGrounded, false);
                animator.ResetTrigger(_animJump);

            }
        }

        private void HandleDodge()
        {
            if (playerInputHandler.DodgeTriggered)
            {
                // Dodge logic here
                animator.SetTrigger(_animDodge);

            }
            else
            {
                animator.ResetTrigger(_animDodge);
            }
        }

        private void HandleDash()
        {             // Dash logic here
        }
        private void ThirdPersonActive()
        {

            if (playerInputHandler.ToggleCameraTriggered)
            {
                StartCoroutine(ToggleCamera(thirdPersonCamera));
            }
        }

        private void HandleMovement()
        {
            if (isInventoryOpen)
            {
                return;
            }

            _playerDir = CalculatePlayerDirection();
            Debug.DrawRay(transform.position, _playerDir * 5f, Color.green);
            _currentMovement.x = _playerDir.x * CurrentSpeed();
            _currentMovement.z = _playerDir.z * CurrentSpeed();
            HandleJumping();


            _characterController.Move(_currentMovement * Time.deltaTime);

            animator.SetFloat(_animVelocityX, Mathf.SmoothDamp(animator.GetFloat(_animVelocityX), _currentMovement.x, ref _velocityX, 0.1f));
            animator.SetFloat(_animVelocityZ, Mathf.SmoothDamp(animator.GetFloat(_animVelocityZ), _currentMovement.z, ref _velocityZ, 0.1f));



        }

        private void ApplyHorizontalRotation(float rotationAmount)
        {
            transform.Rotate(0, rotationAmount, 0);

        }


        private void ApplyVerticalRotation(float rotationAmount)
        {


            if (isInverted)
            {
                _verticalRotation = Mathf.Clamp(_verticalRotation + rotationAmount, -playerData.UpLookRange, playerData.DownLookRange);

            }
            else if (!isInverted)
            {

                _verticalRotation = Mathf.Clamp(_verticalRotation - rotationAmount, -playerData.UpLookRange, playerData.DownLookRange);

            }

            mainCamera.transform.localRotation = Quaternion.Euler(_verticalRotation, 0, 0);

        }



        private void HandleRotation()
        {
            if (isInventoryOpen)
            {
                return;
            }

            _mouseXRotation = playerInputHandler.RotationInput.x * playerData.MouseSensitivity * playerData.RotationSpeed;
            _mouseYRotation = playerInputHandler.RotationInput.y * playerData.MouseSensitivity;




            ApplyHorizontalRotation(_mouseXRotation);
            ApplyVerticalRotation(_mouseYRotation);





        }

        private void ApplyThirdPersonRotation(float pitch, float yaw)

        {

            if (isInverted)
            {
                Debug.Log("Applying INVERTED third person rotation");
                _cinemachineTargetPitch = UpdateRotation(_cinemachineTargetPitch, -pitch, -playerData.DownLookRange, playerData.UpLookRange, true);

            }
            else if (!isInverted)
            {

                Debug.Log("Applying third person rotation");
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
                Debug.DrawRay(mainCamera.transform.position, mainCamera.transform.forward * playerData.InteractRange, Color.red, 1f);

                if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out RaycastHit hit, playerData.InteractRange, ~ignoreLayer))
                {
                    TryOpenChest(hit);
                    // logging the collider the raycast hit //
                    Debug.Log("Hit " + hit.collider.name);

                    // if the collider has the IDamage interface, we store it in 'target'
                    //IInteractable target = hit.collider.GetComponent<IInteractable>();

                    // null check on the target. if target is not null, we call 'interact'
                    //target?.Interact();

                }
            }
        }

        private void TryOpenChest(RaycastHit hit)
        {
            Chest chest = hit.collider.GetComponent<Chest>();
            if (chest != null)
            {
                chest.OpenChest();
            }
        }

        public void TakeDamage(int amount)
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
                // GameManager.Instance.YouLose();
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
            yield return new WaitForSeconds(1.0f);
            playerInputHandler.enabled = true;
        }

        IEnumerator Attack()
        {
            playerInputHandler.enabled = false;
            animator.SetTrigger(_animAttack);
            yield return new WaitForSeconds(1.0f);
            playerInputHandler.enabled = true;
        }
    }
}


