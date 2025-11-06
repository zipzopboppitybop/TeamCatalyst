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

        private float _cinemachineTargetPitch;
        private float _cinemachineTargetYaw;

        public InputHandler playerInputHandler;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private LayerMask ignoreLayer;
        private Vector3 _currentMovement;
        private float _verticalRotation;
        public bool isInverted;

        private float mouseXRotation;
        private float mouseYRotation;

        [Header("Animation")]
        private int animMoveSpeed;
        private int animJump;
        private int animGrounded;
        private int animSprinting;
        private int animAttack;
        private int animDodge;
        private int animDash;

        private int jumpCount = 0;

        private CharacterController characterController;
        private Vector3 playerDir;

        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            SetupAnimator();
            thirdPersonCamera.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            //if (ThirdPersonActive())
            ApplyThirdPersonRotation(mouseYRotation, mouseXRotation);



        }

        //private float CurrentSpeed => playerData.Speed * (playerInputHandler.SprintTriggered ? playerData.SprintMultiplier : 1);

        void Update()
        {

            HandleMovement();
            HandleRotation();

            HandleAttack();
            HandleDodge();
            HandleDash();
            UpdateInteract();
            ThirdPersonActive();

        }

        private void SetupAnimator()
        {
            animMoveSpeed = Animator.StringToHash("Speed");
            animJump = Animator.StringToHash("Jump");
            animGrounded = Animator.StringToHash("Grounded");
            animSprinting = Animator.StringToHash("Sprinting");
            animAttack = Animator.StringToHash("Attack");
            animDodge = Animator.StringToHash("Dodge");
            animDash = Animator.StringToHash("Dash");




        }

        private void HandleAttack()
        {
            if (playerInputHandler.AttackTriggered)
            {
                // Attack logic here
                animator.SetTrigger(animAttack);

            }
            else
            {
                animator.ResetTrigger(animAttack);
            }

        }
        // Rewrite this to use playerData.Speed and playerData.SprintMultiplier with isSprinting bool
        private float CurrentSpeed()
        {
            if (!characterController.isGrounded)
            {
                animator.SetBool(animSprinting, false);
                return playerData.Speed;
            }
            if (playerInputHandler.SprintTriggered)
            {
                animator.SetBool(animSprinting, true);
                return playerData.SprintSpeed;

            }
            else
            {
                animator.SetBool(animSprinting, false);
                return playerData.Speed;

            }
        }
        private Vector3 CalculatePlayerDirection()
        {
            Vector3 inputDirection = new Vector3(playerInputHandler.MoveInput.x, 0f, playerInputHandler.MoveInput.y);
            Vector3 playerDirection = transform.TransformDirection(inputDirection);
            return playerDirection.normalized;
        }

        private void HandleJumping()
        {
            if (characterController.isGrounded)
            {
                jumpCount = 0;

                _currentMovement.y = -1f; // Small downward force to keep the player grounded

                animator.SetBool(animGrounded, true);

                if (playerInputHandler.JumpTriggered && jumpCount < playerData.JumpMax)
                {

                    _currentMovement.y = playerData.JumpForce;
                    animator.SetTrigger(animJump);
                    jumpCount++;
                    animator.ResetTrigger(animJump);
                }


            }

            else if (!characterController.isGrounded)
            {

                _currentMovement.y += Physics.gravity.y * playerData.GravityMultiplier * Time.deltaTime;
                animator.SetBool(animGrounded, false);
                animator.ResetTrigger(animJump);

            }
        }

        private void HandleDodge()
        {
            if (playerInputHandler.DodgeTriggered)
            {
                // Dodge logic here
                animator.SetTrigger(animDodge);

            }
            else
            {
                animator.ResetTrigger(animDodge);
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

            playerDir = CalculatePlayerDirection();

            _currentMovement.x = playerDir.x * CurrentSpeed();
            _currentMovement.z = playerDir.z * CurrentSpeed();
            HandleJumping();


            characterController.Move(_currentMovement * Time.deltaTime);

            animator.SetFloat(animMoveSpeed, Mathf.Max(Mathf.Max(Mathf.Abs(_currentMovement.z), Mathf.Abs(_currentMovement.x)), Mathf.Abs(mouseXRotation)));
            //ThirdPersonActive();
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
            mouseXRotation = playerInputHandler.RotationInput.x * playerData.MouseSensitivity * playerData.RotationSpeed;
            mouseYRotation = playerInputHandler.RotationInput.y * playerData.MouseSensitivity;



            ApplyHorizontalRotation(mouseXRotation);
            ApplyVerticalRotation(mouseYRotation);

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
                if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out RaycastHit hit, playerData.InteractRange, ~ignoreLayer))
                {
                    // logging the collider the raycast hit //
                    Debug.Log(hit.collider.name);

                    // if the collider has the IDamage interface, we store it in 'target'
                    //IInteractable target = hit.collider.GetComponent<IInteractable>();

                    // null check on the target. if target is not null, we call 'TakeDamage'
                    //target?.Interact();

                }
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


    }
}


