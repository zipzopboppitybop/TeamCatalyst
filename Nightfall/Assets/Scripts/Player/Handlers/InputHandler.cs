using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;



namespace Catalyst.Player
{
    public class InputHandler : MonoBehaviour
    {
        [Header("Input Action Asset")]
        [SerializeField] private InputActionAsset playerControls;

        [Header("Action Map Name Reference")]
        [SerializeField] private string actionMapName = "FPS";

        [Header("Directional Input Actions")]
        [SerializeField] private string movement = "Move";
        [SerializeField] private string rotation = "Look";
        [SerializeField] private string sprint = "Sprint";

        private InputAction _movementAction;
        private InputAction _rotationAction;
        private InputAction _sprintAction;

        [Header("In-Game Input Actions")]
        [SerializeField] private string attack = "Attack";
        [SerializeField] private string dive = "Dive";

        private InputAction _attackAction;
        private InputAction _diveAction;

        [SerializeField] private string interact = "Interact";
        [SerializeField] private string crouch = "Crouch";
        [SerializeField] private string dodge = "Dodge";
        [SerializeField] private string jump = "Jump";

        private InputAction _interactAction;
        private InputAction _crouchAction;
        private InputAction _dodgeAction;
        private InputAction _jumpAction;

        [Header("UI Input & Triggers Actions")]
        [SerializeField] private string previous = "Previous";
        [SerializeField] private string next = "Next";
        [SerializeField] private string aim = "Aim";
        [SerializeField] private string fire = "Fire";
        [SerializeField] private string pause = "Pause";
        [SerializeField] private string toggleCamera = "ToggleCamera";

        private InputAction _pauseAction;
        private InputAction _previousAction;
        private InputAction _nextAction;
        private InputAction _aimAction;
        private InputAction _fireAction;
        private InputAction _toggleCameraAction;


        private PlayerController _playerController;
        public Vector2 MoveInput { get; private set; }
        public Vector2 RotationInput { get; private set; }
        public bool SprintTriggered { get; private set; }


        public bool AttackTriggered { get; private set; }
        public bool DiveTriggered { get; private set; }

        public bool InteractTriggered { get; private set; }
        public bool CrouchTriggered { get; private set; }
        public bool DodgeTriggered { get; private set; }
        public bool JumpTriggered { get; private set; }
        public bool PrevTriggered { get; private set; }
        public bool NextTriggered { get; private set; }
        public bool AimTriggered { get; private set; }
        public bool FireTriggered { get; private set; }
        public bool PauseTriggered { get; private set; }
        public bool ToggleCameraTriggered { get; private set; }


        public PlayerController PlayerController => _playerController;

        private void Awake()
        {
            InputActionMap mapReference = playerControls.FindActionMap(actionMapName);

            // Directionals
            _movementAction = mapReference.FindAction(movement);
            _rotationAction = mapReference.FindAction(rotation);
            _sprintAction = mapReference.FindAction(sprint);

            //  D-Pad
            _attackAction = mapReference.FindAction(attack);
            _diveAction = mapReference.FindAction(dive);
            _interactAction = mapReference.FindAction(interact);
            _crouchAction = mapReference.FindAction(crouch);
            _dodgeAction = mapReference.FindAction(dodge);
            _jumpAction = mapReference.FindAction(jump);

            // Top Buttons
            _previousAction = mapReference.FindAction(previous);
            _nextAction = mapReference.FindAction(next);
            _aimAction = mapReference.FindAction(aim);
            _fireAction = mapReference.FindAction(fire);
            _pauseAction = mapReference.FindAction(pause);
            _toggleCameraAction = mapReference.FindAction(toggleCamera);
            SubscribeActionValuesToInputEvents();
        }

        private void SubscribeActionValuesToInputEvents()
        {
            _movementAction.performed += inputInfo => MoveInput = inputInfo.ReadValue<Vector2>();
            _movementAction.canceled += inputInfo => MoveInput = Vector2.zero;
            _rotationAction.performed += inputInfo => RotationInput = inputInfo.ReadValue<Vector2>();
            _rotationAction.canceled += inputInfo => RotationInput = Vector2.zero;

            _jumpAction.performed += inputInfo => JumpTriggered = true;
            _jumpAction.canceled += inputInfo => JumpTriggered = false;
            _sprintAction.performed += inputInfo => SprintTriggered = true;
            _sprintAction.canceled += inputInfo => SprintTriggered = false;

            _attackAction.performed += inputInfo => AttackTriggered = true;
            _attackAction.canceled += inputInfo => AttackTriggered = false;
            _diveAction.performed += inputInfo => DiveTriggered = true;
            _diveAction.canceled += inputInfo => DiveTriggered = false;

            _interactAction.performed += inputInfo => InteractTriggered = true;
            _interactAction.canceled += inputInfo => InteractTriggered = false;

            _crouchAction.performed += inputInfo => CrouchTriggered = true;
            _crouchAction.canceled += inputInfo => CrouchTriggered = false;
            _dodgeAction.performed += inputInfo => DodgeTriggered = true;
            _dodgeAction.canceled += inputInfo => DodgeTriggered = false;

            _previousAction.performed += inputInfo => PrevTriggered = true;
            _previousAction.canceled += inputInfo => PrevTriggered = false;
            _nextAction.performed += inputInfo => NextTriggered = true;
            _nextAction.canceled += inputInfo => NextTriggered = false;

            _aimAction.performed += inputInfo => AimTriggered = true;
            _aimAction.canceled += inputInfo => AimTriggered = false;
            _fireAction.performed += inputInfo => FireTriggered = true;
            _fireAction.canceled += inputInfo => FireTriggered = false;

            _pauseAction.performed += inputInfo => PauseTriggered = true;
            _pauseAction.canceled += inputInfo => PauseTriggered = false;

            _toggleCameraAction.performed += inputInfo => ToggleCameraTriggered = true;
            _toggleCameraAction.canceled += inputInfo => ToggleCameraTriggered = false;

        }

        private void OnEnable()
        {
            playerControls.FindActionMap(actionMapName).Enable();
        }

        private void OnDisable()
        {
            playerControls.FindActionMap(actionMapName).Disable();
        }

    }
}