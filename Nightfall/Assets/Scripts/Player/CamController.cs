using System.Collections;
using Catalyst.Player;
using Unity.Cinemachine;
using UnityEngine;

namespace Catalyst.CameraController
{
    public class CamController : MonoBehaviour
    {

        [SerializeField] private PlayerData playerData;
        [SerializeField] private Transform followTarget;
        [SerializeField] private Transform aimTarget;
        [SerializeField, Range(1, 20)] private int aimFollowSpeed;
        [SerializeField] private LayerMask gunCamLayers;
        [SerializeField] private LayerMask thirdPersonLayers;
        [SerializeField] private LayerMask ignoreLayer;
        [SerializeField] private GameObject[] hideInFPS;

        private Camera _mainCamera;
        private Camera _gunCamera;
        private CinemachineCamera _fpsCamera;
        private CinemachineCamera _thirdPersonCamera;
        private CinemachineCamera _aimCamera;
        private InputHandler _playerInput;

        private PlayerController _playerController;

        public bool isInverted;

        private float _cinemachineTargetPitch;
        private float _cinemachineTargetYaw;

        private float _mouseXRotation;
        private float _mouseYRotation;


        private float _verticalRotation;

        public Camera MainCamera => _mainCamera;
        public CinemachineCamera FPSCamera => _fpsCamera;
        public CinemachineCamera ThirdPersonCamera => _thirdPersonCamera;
        public CinemachineCamera AimCamera => _aimCamera;
        public Transform FollowTarget => followTarget;
        public Transform AimTarget => aimTarget;



        private void Awake()
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            _gunCamera = GameObject.FindGameObjectWithTag("GunCamera").GetComponent<Camera>();
            _fpsCamera = GameObject.FindGameObjectWithTag("FPSCamera").GetComponent<CinemachineCamera>();
            _thirdPersonCamera = GameObject.FindGameObjectWithTag("ThirdPersonCamera").GetComponent<CinemachineCamera>();
            _aimCamera = GameObject.FindGameObjectWithTag("AimCamera").GetComponent<CinemachineCamera>();

            _playerInput = GetComponent<InputHandler>();

            _playerController = GetComponent<PlayerController>();
            aimTarget.gameObject.SetActive(false);

        }

        private void Start()
        {
            _thirdPersonCamera.gameObject.SetActive(false);
            _aimCamera.gameObject.SetActive(false);


            StartCoroutine(ToggleCullingLayer(0.5f));
        }

        private void Update()
        {
            if (!GameManager.instance.isPaused)
            {
                ThirdPersonActive();
                HandleRotation();
            }

        }
        private void LateUpdate()
        {
            if (!GameManager.instance.isPaused)

            {
                ApplyThirdPersonRotation(_mouseYRotation, _mouseXRotation);
                ToggleAimCamera(_playerInput.AimHeld);
            }

        }

        private bool ThirdPersonActive()
        {

            if (_playerInput.ToggleCameraTriggered)
            {
                StartCoroutine(ToggleCamera(_thirdPersonCamera));
            }
            return _thirdPersonCamera.gameObject.activeSelf;
        }

        private void ApplyHorizontalRotation(float rotationAmount)
        {
            if (ThirdPersonActive() && _playerInput.MoveInput.magnitude < 0.1)
            {
                return;
            }
            else if (ThirdPersonActive() && _playerInput.MoveInput.magnitude >= 0.1)
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

            _fpsCamera.transform.localRotation = Quaternion.Euler(_verticalRotation, 0, 0);
        }



        private void HandleRotation()
        {
            if (_playerController.isInventoryOpen)
            {
                return;
            }

            _mouseXRotation = _playerInput.RotationInput.x * playerData.MouseSensitivity * playerData.CameraRotationSpeed;
            _mouseYRotation = _playerInput.RotationInput.y * playerData.MouseSensitivity;

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
        private void ToggleGunCam()
        {
            if (_gunCamera == null)
                return;
            if (_thirdPersonCamera.gameObject.activeSelf)
            {
                _gunCamera.enabled = false;


            }

            else
            {
                _gunCamera.enabled = true;
                Debug.Log("Gun cam enabled");
                // enable ignore layers on main camera 


            }
            StartCoroutine(ToggleCullingLayer(1f));
        }

        IEnumerator ToggleCullingLayer(float time)
        {
            if (_fpsCamera == null)
                yield break;

            if (_thirdPersonCamera.gameObject.activeSelf)
            {
                yield return new WaitForEndOfFrame();
                _mainCamera.cullingMask |= thirdPersonLayers;
                Debug.Log("Adding culling layers to main camera");

                foreach (GameObject go in hideInFPS)
                {
                    go.SetActive(true);
                }

                if (playerData.AmmoCount > 0)
                {
                    _mainCamera.cullingMask |= ~gunCamLayers;
                }
            }
            else if (_gunCamera.enabled)
            {
                // Wait until camera blend is done for sure
                yield return new WaitForSeconds(time);

                _mainCamera.cullingMask &= ~thirdPersonLayers;
                _mainCamera.cullingMask &= ~gunCamLayers;

                foreach (GameObject go in hideInFPS)
                {
                    go.SetActive(false);
                }

            }
        }

        public void ToggleAimCamera(bool isAiming)
        {
            if (_aimCamera == null)
                return;
            if (ThirdPersonActive())
            {

                _aimCamera.gameObject.SetActive(isAiming);
                aimTarget.gameObject.SetActive(isAiming);


                if (isAiming)
                    FollowMousePosition();

            }
        }

        IEnumerator ToggleCamera(CinemachineCamera cam)
        {
            // Stop everything a bit to avoid multiple toggles      
            _playerInput.enabled = false;
            cam.gameObject.SetActive(!cam.gameObject.activeSelf);
            ToggleGunCam();
            yield return new WaitForSeconds(1.0f);
            _playerInput.enabled = true;
        }

        public Vector3 GetMouseWorldPosition()
        {

            Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray ray = _mainCamera.ScreenPointToRay(screenCenterPoint);

            if (Physics.Raycast(ray, out RaycastHit hit, _mainCamera.farClipPlane, ~ignoreLayer, QueryTriggerInteraction.Ignore))
            {
                return hit.point;
            }
            return Vector3.zero; // Return a default value if no hit

        }
        public void FollowMousePosition()
        {
            Vector3 worldAimTarget
                = GetMouseWorldPosition();

            worldAimTarget.y = transform.position.y;

            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;
            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * aimFollowSpeed);

            aimTarget.position = worldAimTarget;
        }
    }
}
