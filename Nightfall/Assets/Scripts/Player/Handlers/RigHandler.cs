using Catalyst.GamePlay;
using UnityEngine;
using UnityEngine.Animations.Rigging;


namespace Catalyst.Player.Handlers
{
    public class RigHandler : MonoBehaviour
    {
        private Rig rig;
        private InputHandler _playerInput;
        private PlayerController _playerController;
        private GunManager _gunManager;
        private float targetWeight;

        void Awake()
        {
            rig = transform.GetComponent<Rig>();
            _playerInput = GetComponent<InputHandler>();
            _playerController = transform.GetComponent<PlayerController>();
            targetWeight = 0f;
        }

        // Update is called once per frame
        void Update()
        {
            rig.weight = Mathf.Lerp(rig.weight, targetWeight, Time.deltaTime * 1);

            if (_playerInput.AimHeld) // Right mouse button held
            {
                targetWeight = 1f;


            }
            else
            {
                targetWeight = 0f;
            }
        }

        private void SetTargetWeight(float weight)
        {
            targetWeight = weight;

        }

    }
}
