using System.Collections;
using Catalyst.Player;
using Unity.Cinemachine;
using UnityEngine;

namespace Catalyst.CameraController
{
    public class CamController : MonoBehaviour
    {

        public Camera sceneCam;
        public Camera gunCam;
        public CinemachineCamera FPSCamera;
        public CinemachineCamera thirdPersonCamera;
        public CinemachineCamera AimCamera;
        [SerializeField] private Transform followTarget;
        [SerializeField] private Transform aimTarget;
        [SerializeField, Range(1, 20)] private int aimFollowSpeed;
        [SerializeField] private LayerMask gunCamLayers;
        [SerializeField] private LayerMask thirdPersonLayers;
        [SerializeField] private LayerMask ignoreLayer;
        [SerializeField] private GameObject[] hideInFPS;
        [SerializeField] private InputHandler playerInputHandler;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private PlayerController playerController;

        public bool isInverted;

        private float _cinemachineTargetPitch;
        private float _cinemachineTargetYaw;

        private float _mouseXRotation;
        private float _mouseYRotation;


        private float _verticalRotation;

        private void Start()
        {
            thirdPersonCamera.gameObject.SetActive(false);
            AimCamera.gameObject.SetActive(false);
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
                ToggleAimCamera(playerInputHandler.AimHeld);
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
            if (playerController.isInventoryOpen)
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

        public void ToggleAimCamera(bool isAiming)
        {
            if (AimCamera == null)
                return;
            if (ThirdPersonActive())
            {
                AimCamera.gameObject.SetActive(isAiming);
                if (isAiming)
                    FollowMousePosition();

            }
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

        public Vector3 GetMouseWorldPosition()
        {

            Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray ray = sceneCam.ScreenPointToRay(screenCenterPoint);
            if (Physics.Raycast(ray, out RaycastHit hit, sceneCam.farClipPlane, ~ignoreLayer, QueryTriggerInteraction.Ignore))
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
