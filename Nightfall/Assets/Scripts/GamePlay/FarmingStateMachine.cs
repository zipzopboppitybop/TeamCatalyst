using Catalyst.Player;
using UnityEngine;

namespace Catalyst.GamePlay.Farming
{
    public class FarmingStateMachine : MonoBehaviour
    {
        [SerializeField] private PlayerData farmerData;
        [SerializeField] private Animator animator;

        public enum FarmingState { None, Plowing, Watering, Planting, Harvesting }

        private FarmingState currentFarmingState = FarmingState.None;

        [Header("Animation")]
        private int _animPlow;
        private int _animWatering;
        private int _animPlanting;
        private int _animHarvesting;

        private void Start()
        {
            //farmerData = GetComponent<PlayerData>();
            //animator = GetComponent<Animator>();
            SetupAnimator();
            if (farmerData != null)
            {
                currentFarmingState = farmerData.CurrentFarmingState;
            }
        }
        private void Update()
        {
            SetFarmingState(currentFarmingState);
        }

        private void SetupAnimator()
        {
            _animPlow = Animator.StringToHash("isPlowing");
            _animWatering = Animator.StringToHash("isWatering");
            _animPlanting = Animator.StringToHash("isPlanting");
            _animHarvesting = Animator.StringToHash("isHarvesting");
        }

        public void SetFarmingState(FarmingState newState)
        {
            currentFarmingState = newState;
            switch (currentFarmingState)
            {
                case FarmingState.None:
                    ClearFarmingStates();
                    // Handle None state
                    break;
                case FarmingState.Plowing:
                    ClearFarmingStates();
                    animator.SetBool(_animPlow, true);
                    // Handle Plowing state
                    break;
                case FarmingState.Watering:
                    ClearFarmingStates();
                    animator.SetBool(_animWatering, true);
                    // Handle Watering state
                    break;
                case FarmingState.Planting:
                    ClearFarmingStates();
                    animator.SetBool(_animPlanting, true);
                    // Handle Planting state
                    break;
                case FarmingState.Harvesting:
                    ClearFarmingStates();
                    animator.SetBool(_animHarvesting, true);
                    // Handle Harvesting state
                    break;
                default:
                    ClearFarmingStates();
                    break;
            }
        }

        public void ClearFarmingStates()
        {
            animator.SetBool(_animPlow, false);
            animator.SetBool(_animWatering, false);
            animator.SetBool(_animPlanting, false);
            animator.SetBool(_animHarvesting, false);
        }

    }



}
