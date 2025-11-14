using Catalyst.Player;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Catalyst.Player.Handlers
{
    public class RigHandler : MonoBehaviour
    {
        private Rig rig;
        private InputHandler playerInput;
        private float targetWeight;

        void Awake()
        {
            rig = GetComponent<Rig>();
            playerInput = GetComponentInParent<InputHandler>();
            targetWeight = 0f;
        }

        // Update is called once per frame
        void Update()
        {


            if (playerInput.AimHeld) // Right mouse button held
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
