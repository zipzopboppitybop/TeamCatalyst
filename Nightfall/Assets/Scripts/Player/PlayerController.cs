using System.Collections;
using System.Runtime.CompilerServices;
using Unity.Cinemachine;
using UnityEngine;

namespace Catalyst.Player
{
    public class PlayerController : MonoBehaviour, IDamage
    {
        [Header("References")]

        [SerializeField] private Animator animator;
        public CinemachineCamera mainCamera;
        public CinemachineCamera thirdPersonCamera;
        public Camera gunCam;
        [SerializeField] private Transform followTarget;
        [SerializeField] GameObject gunModel;
        public AudioSource aud;

        private float _cinemachineTargetPitch;
        private float _cinemachineTargetYaw;

        public InputHandler playerInputHandler;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private LayerMask ignoreLayer;
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
        private int _animDodge;
        private int _animDash;
        private int _animAim;
        private int _animShoot;
        private int _animVelocityX;
        private int _animVelocityZ;

        private int _jumpCount = 0;
        private float _shootTimer = 0f;

        private CharacterController characterController;
        private Vector3 playerDir;
        private int _gunListPos;

        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            SetupAnimator();
            thirdPersonCamera.gameObject.SetActive(false);


        }

        private void LateUpdate()
        {
            //if (ThirdPersonActive())
            ApplyThirdPersonRotation(_mouseYRotation, _mouseXRotation);



        }

        private float CurrentSpeed => playerData.Speed * (playerInputHandler.SprintTriggered ? playerData.SprintSpeed : 1);


        void Update()
        {

            HandleMovement();
            HandleRotation();

            HandleAttack();
            HandleAim();
            HandleDodge();
            HandleDash();
            UpdateInteract();
            ThirdPersonActive();

        }

        private void SetupAnimator()
        {

            _animJump = Animator.StringToHash("Jump");
            _animGrounded = Animator.StringToHash("Grounded");
            _animAttack = Animator.StringToHash("Attack");
            _animDodge = Animator.StringToHash("Dodge");
            _animDash = Animator.StringToHash("Dash");
            _animAim = Animator.StringToHash("Aiming");
            _animShoot = Animator.StringToHash("Shoot");
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

        private void HandleDodge()
        {
            if (playerInputHandler.DodgeTriggered)
            {
                StartCoroutine(Dodge());

            }

        }

        private void HandleDash()
        {             // Dash logic here
            if (playerInputHandler.DiveTriggered)
            {
                StartCoroutine(Dash());
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
            _shootTimer += Time.deltaTime;
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
            HandleDodge();


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

            mainCamera.transform.localRotation = Quaternion.Euler(_verticalRotation, 0, 0);

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
            if (Input.GetKeyDown(KeyCode.E))
            {
                Vector3 origin = mainCamera.transform.position;
                Vector3 direction = mainCamera.transform.forward;

                Debug.DrawRay(origin, direction * playerData.InteractRange, Color.red, 1f);

                if (Physics.Raycast(origin, direction, out RaycastHit hit, playerData.InteractRange, ~ignoreLayer))
                {
                    Chest chest = hit.collider.GetComponent<Chest>();
                    if (chest != null)
                    {
                        chest.OpenChest();
                    }
                }
            }
        }

        private void HandleAim()
        {
            if (playerInputHandler.AimTriggered)
            {
                animator.SetBool(_animAim, true);
                Debug.Log("Aiming");
                HandleShoot();

            }
            else
            {
                animator.SetBool(_animAim, false);

            }

        }

        private void HandleShoot()
        {
            if (playerInputHandler.FireTriggered && _shootTimer > playerData.ShootRate)
            {
                _shootTimer = 0f;
                Shoot();
                animator.SetTrigger("Shoot");
                Debug.Log("Shooting");
                //animator.ResetTrigger("Shoot");

            }
        }

        private void Shoot()
        {
            void shoot()
            {
                _shootTimer = 0;
                playerData.Guns[_gunListPos].ammoCur--;
                aud.PlayOneShot(playerData.Guns[_gunListPos].shootSounds[Random.Range(0, playerData.Guns[_gunListPos].shootSounds.Length)], playerData.Guns[_gunListPos].shootVolume);
                //updatePlayerUI();

                RaycastHit hit;
                if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, playerData.ShootDist, ~ignoreLayer))
                {
                    Instantiate(playerData.Guns[_gunListPos].hitEffect, hit.point, Quaternion.identity);

                    Debug.Log(hit.collider.name);

                    IDamage dmg = hit.collider.GetComponent<IDamage>();

                    if (dmg != null)
                    {

                        dmg.takeDamage(playerData.ShootDamage);
                    }

                }
            }
        }

        private void ToggleGunCam()
        {
            if (gunCam == null)
                return;
            if (thirdPersonCamera.gameObject.activeSelf)
                gunCam.enabled = false;

            else gunCam.enabled = true;

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

        IEnumerator Dash()
        {

            //animator.SetTrigger(animDash);
            yield return new WaitForSeconds(1.0f);
            //playerInputHandler.DashTriggered = false;
        }


        IEnumerator Dodge()
        {
            // move the player quickly in the dodge direction with a burst of speed




            animator.SetTrigger(_animDodge);
            PlayerDodge();
            yield return new WaitForSeconds(1.0f);
            animator.ResetTrigger(_animDodge);
            playerInputHandler.DodgeTriggered = false;
        }
        private void PlayerDodge()
        {
            Vector3 dodgeDirection = CalculateMoveDirection();
            if (dodgeDirection == Vector3.zero)
            {
                dodgeDirection = -transform.forward;
            }
            characterController.Move(dodgeDirection.normalized * playerData.SprintSpeed * 2 * Time.deltaTime);

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
                Debug.Log("You are dead!");
            }
        }

    }
}



