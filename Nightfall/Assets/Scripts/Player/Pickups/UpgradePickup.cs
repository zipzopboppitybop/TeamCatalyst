using Catalyst.GamePlay;
using Catalyst.Player;
using UnityEngine;



namespace Catalyst.Player.Pickups
{
    public class UpgradePickup : MonoBehaviour, IInteractable
    {
        enum pickupType { health, key, stealth, gun }


        [SerializeField] pickupType type;
        [SerializeField] int healAmount;

        [SerializeField] WeaponData gunData;
        //[SerializeField] inventoryItem gun;

        public float rotateSpeed = 50f; // Speed at which the pickup rotates for visibility
        public float pulseSpeed = 2f; // Speed of the pulsing effect
        public float pulseMagnitude = 0.1f; // Magnitude of the pulsing effect
        private float pulseTimer = 0f; // Timer for pulsing effect


        private Vector3 originalPosition;
        private Vector3 initialScale; // Initial scale of the pickup for pulsing effect
        private Vector3 initialRotation;
        private Quaternion rotation;

        private PlayerController _player;
        private GunManager _gunManager;


        private void Start()
        {
            originalPosition = transform.position;
            initialScale = transform.localScale;
            initialRotation = transform.eulerAngles;
            rotation = Quaternion.Euler(initialRotation);

        }

        private void Update()
        {
            // Optional: Add any rotation or animation to the pickup object for visual effect
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime); // Rotate around the Y-axis
                                                                        // Pulsing effect with up and down movement
            pulseTimer += Time.deltaTime * pulseSpeed;
            float scaleFactor = 1 + (Mathf.Sin(pulseTimer) * pulseMagnitude);
            transform.localScale = initialScale * scaleFactor;
            transform.position = originalPosition + new Vector3(0, Mathf.Sin(pulseTimer) * pulseMagnitude, 0); // Adjust the Y position for pulsing effect


        }

        public void Interact()
        {



            switch (type)
            {
                case pickupType.health:
                    _player.Heal(healAmount);
                    Destroy(gameObject);
                    break;
                case pickupType.key:
                    //_player.AddKey();
                    Destroy(gameObject);
                    break;
                case pickupType.stealth:
                    //_player.ActivateStealth(10.0f);
                    Destroy(gameObject);
                    break;
                case pickupType.gun:
                    if (_gunManager.HasGun(gunData))
                    {
                        Debug.Log("Player already has " + gunData.name);
                        return;
                    }

                    Debug.Log("Should be picking up");

                    _gunManager.GetWeaponData(gunData);

                    Destroy(gameObject);
                    Debug.Log("Object destroyed after pickup");
                    break;
                default:
                    Debug.LogWarning("Unknown pickup type");
                    break;
            }
        }

        private void OnTriggerEnter(Collider other)
        {

            if (other.CompareTag("Player"))
            {
                _player = other.GetComponent<PlayerController>();
                _gunManager = _player.GetComponent<GunManager>();


                //HUDManager.instance.UpdateInteractPrompt("Press 'E' to pick up " + gun.itemName);
                //HUDManager.instance.interactPromptText.color = Color.white;
                Debug.Log("Player in range to pick up");

            }

        }


    }
}