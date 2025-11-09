using System.Collections.Generic;
using Catalyst.GamePlay.Farming;


using UnityEngine;


namespace Catalyst.Player
{
    [CreateAssetMenu(fileName = "Player Data", menuName = "Player/PlayerData")]
    public class PlayerData : ScriptableObject
    {

        [SerializeField] GameObject playerPrefab;
        [Header("Player Data")]
        [SerializeField] private string nameTag = "Player";
        [SerializeField] private int score;
        [SerializeField, Range(0, 100f)] private float health;
        [SerializeField] private int healthMax;
        [SerializeField, Range(0, 100)] private int stamina;
        [SerializeField, Range(0, 10)] private int staminaRegen;
        [SerializeField] private int staminaMax;
        [SerializeField, Range(0, 30)] private int stealth;
        [SerializeField, Range(0, 5)] private int interactRange;
        [SerializeField, Range (0, 999999)] private int currency ;
        [SerializeField] private FarmingStateMachine.FarmingState currentFarmingState = FarmingStateMachine.FarmingState.None;

        [Header("Weapon Data")]
        [SerializeField] private int shootDamage;
        [SerializeField] private float shootRate;
        [SerializeField] private int shootDist;
        [SerializeField] private int ammoCount;
        [SerializeField] private int ammoMax;



        [Header("Movement Speeds")]
        [SerializeField] private float walkingSpeed;
        [SerializeField] private float sprintSpeed;
        [SerializeField] private float dashSpeed;

        [Header("Jump Parameters")]
        [SerializeField] private float jumpForce;
        [SerializeField] private float gravityMultiplier;
        [SerializeField] private int jumpMax;

        [Header("Look Parameters")]
        [SerializeField] private float mouseSensitivity;
        [SerializeField, Range(0, 1f)] private float cameraRotationSpeed;
        [SerializeField, Range(1, 20)] private int rotationSpeed;
        [SerializeField] private float upLookRange;
        [SerializeField] private float downLookRange;

        [SerializeField] private float fpsVerticalRange;





        public string NameTag => nameTag;
        public int Score => score;
        public float Health { get => health; set => health = value; }
        public int HealthMax { get => healthMax; set => healthMax = value; }


        public int Stamina { get => stamina; set => stamina = value; }
        public int StaminaRegen { get => staminaRegen; set => staminaRegen = value; }
        public int StaminaMax { get => staminaMax; set => staminaMax = value; }
        public int Stealth { get => stealth; set => stealth = value; }
        public int InteractRange { get => interactRange; set => interactRange = value; }
        public int Currency { get => currency; set => currency = value; }


        public int ShootDamage { get => shootDamage; set => shootDamage = value; }
        public float ShootRate { get => shootRate; set => shootRate = value; }

        public int ShootDist { get => shootDist; set => shootDist = value; }

        public int AmmoCount { get => ammoCount; set => ammoCount = value; }
        public int AmmoMax { get => ammoMax; set => ammoMax = value; }


        public float Speed => walkingSpeed;
        public float SprintSpeed => sprintSpeed;
        public float DashSpeed => dashSpeed;
        public float CameraRotationSpeed => cameraRotationSpeed;
        public int RotationSpeed => rotationSpeed;



        public float JumpForce => jumpForce;
        public float GravityMultiplier => gravityMultiplier;
        public int JumpMax => jumpMax;

        public float MouseSensitivity => mouseSensitivity;
        public float UpLookRange => upLookRange;
        public float DownLookRange => downLookRange;
        public float FPSVerticalRange => fpsVerticalRange;

        public int keysCollected = 0;
        public int notesCollected = 0;

        public int roomsClear = 0;


        [SerializeField] List<WeaponData> guns;
        public List<WeaponData> Guns => guns;
        private WeaponData currentGun;

        public WeaponData CurrentGun
        {
            get => currentGun;
            set => currentGun = value;
        }

        // public List<WeaponData> Guns;pu



        void OnEnable()
        {
            InitializePlayer();

        }

        public void InitializePlayer()
        {
            health = healthMax;
            stamina = staminaMax;
            ammoCount = ammoMax;
        }

        public FarmingStateMachine.FarmingState CurrentFarmingState
        {
            get => currentFarmingState;
            set => currentFarmingState = value;
        }

        public void AddScore(int amount)
        {
            score += amount;
        }
        public void RemoveScore(int amount)
        {
            score -= amount;
        }

        public void Update()
        {
            // This method can be used to update player data if needed


        }
        public void Reset()
        {
            InitializePlayer();
        }

       
    }
}
