using System.Collections;
using UnityEngine;

namespace Catalyst.Player.Handlers
{
    public class AnimationHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] PlayerData playerData;
        private Animator _animator;
        private InputHandler _playerInputHandler;

        [Header("Animation Triggers")]
        private int _animJump;
        private int _animGrounded;
        private int _animAttack;
        private int _animDash;
        private int _animShoot;
        private int _animReload;

        [Header("Animation Bools")]
        private int _animAim;

        private int _animArmed;


        [Header("Animation Floats")]
        private float _velocityX;
        private float _velocityZ;
        private int _animVelocityX;
        private int _animVelocityZ;


        void Start()
        {
            _animator = GetComponent<Animator>();
            _playerInputHandler = GetComponent<InputHandler>();
            SetupAnimator();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateAnimator();


        }

        private void SetupAnimator()
        {
            _animJump = Animator.StringToHash("Jump");
            _animGrounded = Animator.StringToHash("Grounded");
            _animAttack = Animator.StringToHash("Attack");
            _animDash = Animator.StringToHash("Dash");
            _animVelocityX = Animator.StringToHash("Velocity X");
            _animVelocityZ = Animator.StringToHash("Velocity Z");

            _animAim = Animator.StringToHash("Aiming");
            _animShoot = Animator.StringToHash("Shoot");
            _animReload = Animator.StringToHash("Reload");
            _animArmed = Animator.StringToHash("isArmed");
        }

        public void SetGrounded(bool grounded)
        {
            GetComponent<Animator>().SetBool(_animGrounded, grounded);
        }

        public void SetAiming(bool aiming)
        {
            _animator.SetBool(_animAim, aiming);
        }
        public void SetArmed(bool armed)
        {
            _animator.SetBool(_animArmed, armed);
        }


        public void ResetJumpTrigger()
        {
            _animator.ResetTrigger(_animJump);
        }

        public void UpdateMoveAnimations(Vector3 currentMovement)
        {
            if (_playerInputHandler.AimHeld)
                _animator.SetFloat(_animVelocityX, _playerInputHandler.MoveInput.x * 0.1f);

            else
                _animator.SetFloat(_animVelocityX, Mathf.SmoothDamp(_animator.GetFloat(_animVelocityX), _playerInputHandler.MoveInput.x * currentMovement.magnitude, ref _velocityX, 0.1f));

            _animator.SetFloat(_animVelocityZ, Mathf.SmoothDamp(_animator.GetFloat(_animVelocityZ), _playerInputHandler.MoveInput.y * currentMovement.magnitude, ref _velocityZ, 0.1f));
        }


        IEnumerator Attack()
        {

            _animator.SetTrigger(_animAttack);
            yield return new WaitForSeconds(1.0f);
            _animator.ResetTrigger(_animAttack);
            _playerInputHandler.AttackTriggered = false;
        }
        public void TriggerAttack()
        {
            StartCoroutine(Attack());
        }

        IEnumerator Dash()
        {
            _animator.SetTrigger(_animDash);

            yield return new WaitForSeconds(1.0f);
            _animator.ResetTrigger(_animDash);
            _playerInputHandler.DashTriggered = false;
        }
        public void TriggerDash()
        {
            StartCoroutine(Dash());
        }

        IEnumerator Jump()
        {
            _animator.SetTrigger(_animJump);
            
            yield return new WaitForSeconds(1f);
            _animator.ResetTrigger(_animJump);
            _playerInputHandler.JumpTriggered = false;
        }

        public void TriggerJump()
        {
            StartCoroutine(Jump());
        }
        private void UpdateAnimator()
        {
            _animator.SetLayerWeight(1, 1 - (playerData.Health / playerData.HealthMax));

        }
        public void TriggerShoot()
        {
            _animator.SetTrigger(_animShoot);
        }
        public void SetReloading(bool reloading)
        {
            _animator.SetBool(_animReload, reloading);
        }
        public void TriggerReload()
        {
            _animator.SetTrigger(_animReload);
        }
    }
}
