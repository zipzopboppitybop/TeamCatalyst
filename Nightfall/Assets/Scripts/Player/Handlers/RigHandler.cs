using Catalyst.GamePlay;
using Catalyst.Player;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Catalyst.Player.Handlers
{
    public class RigHandler : MonoBehaviour
    {
        private Rig rig;
        private InputHandler _playerInput;
        private Animator _animator;
        private PlayerController _playerController;
        private GunManager _gunManager;
        [SerializeField] private float targetWeight;

        void Awake()
        {
            rig = GetComponent<Rig>();
            _playerInput = GetComponentInParent<InputHandler>();
            _playerController = GetComponentInParent<PlayerController>();
            targetWeight = 0f;
        }

        // Update is called once per frame
        void Update()
        {
            //playerInput.AimHeld = true; // For testing purposes, remove this line in production 

            if (_gunManager.isArmed && _animator.GetBool("Aiming")) // Right mouse button held
            {
                SetTargetWeight(1f);
                //targetWeight = 1f;


            }
            else
            {
                SetTargetWeight(0f);
                //targetWeight = 0f;

            }
        }

        private void SetTargetWeight(float weight)
        {
            targetWeight = weight;
            rig.weight = Mathf.Lerp(rig.weight, targetWeight, Time.deltaTime * 10f);
        }

    }
}
