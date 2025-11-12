using Catalyst.GamePlay;
using Catalyst.Player;
using UnityEngine;



namespace Catalyst.Player.Pickups
{
    public class UpgradePickup : MonoBehaviour, IInteractable
    {
        enum pickupType { health, ammo, stealth, gun }


        [SerializeField] private pickupType type;
        [SerializeField] private int value;

        [SerializeField] private WeaponData gunData;
        //[SerializeField] inventoryItem gun;

        public float rotateSpeed = 50f; // Speed at which the pickup rotates for visibility
        public float pulseSpeed = 2f; // Speed of the pulsing effect
        public float pulseMagnitude = 0.1f; // Magnitude of the pulsing effect
        private float _pulseTimer = 0f; // Timer for pulsing effect


        private Vector3 _originalPosition;
        private Vector3 _initialScale; // Initial scale of the pickup for pulsing effect
        private Vector3 _initialRotation;
        private Quaternion _rotation;

        private PlayerController _player;
        private GunManager _gunManager;


        private void Start()
        {
            _originalPosition = transform.position;
            _initialScale = transform.localScale;
            _initialRotation = transform.eulerAngles;
            _rotation = Quaternion.Euler(_initialRotation);

        }

        private void Update()
        {
            // Optional: Add any rotation or animation to the pickup object for visual effect
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime); // Rotate around the Y-axis
                                                                        // Pulsing effect with up and down movement
            _pulseTimer += Time.deltaTime * pulseSpeed;
            float scaleFactor = 1 + (Mathf.Sin(_pulseTimer) * pulseMagnitude);
            transform.localScale = _initialScale * scaleFactor;
            transform.position = _originalPosition + new Vector3(0, Mathf.Sin(_pulseTimer) * pulseMagnitude, 0); // Adjust the Y position for pulsing effect


        }

        public void Interact()
        {
            switch (type)
            {
                case pickupType.health:
                    _player.Heal(value);
                    Destroy(gameObject);
                    break;
                case pickupType.ammo:



                    Destroy(gameObject);
                    break;
                case pickupType.stealth:
                    //_player.ActivateStealth(value);
                    Destroy(gameObject);
                    break;
                case pickupType.gun:
                    if (_gunManager.HasGun(gunData))
                    {
                        if (_gunManager.ReloadWeapon(gunData))
                        {
                            Debug.Log("Reloaded existing weapon");
                            Destroy(gameObject);
                        }
                        else
                        {
                            Debug.Log("Already have this weapon with max ammo, cannot pick up another");
                        }

                        return;
                    }

                    _gunManager.GetWeaponData(gunData);
                    Debug.Log("Picked Up " + gunData.name);
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