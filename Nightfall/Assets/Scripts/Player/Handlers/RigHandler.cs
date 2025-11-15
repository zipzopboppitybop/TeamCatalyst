using UnityEngine;
using UnityEngine.Animations.Rigging;


namespace Catalyst.Player.Handlers
{
    public class RigHandler : MonoBehaviour
    {
        private Rig rig;
        [SerializeField] private InputHandler _playerInput;
        private float targetWeight;

        void Awake()
        {
            rig = transform.GetComponent<Rig>();
            rig.weight = 0f;
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
