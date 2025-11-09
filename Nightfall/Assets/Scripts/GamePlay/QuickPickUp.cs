
using Catalyst.GamePlay;
using Catalyst.Player;
using UnityEngine;

namespace Catalyst.GamePlay
{
    public class QuickPickUp : MonoBehaviour
    {
        enum pickupType { health, key, stealth, gun }

        [SerializeField] pickupType type;
        [SerializeField] WeaponData gun;

        [SerializeField] int healAmount;
        public void Interact(Collider other)
        {
            IInteractable interactable = other.GetComponent<IInteractable>();
            if (type == pickupType.health)
            {
                interactable?.Interact();
                // other.GetComponent<PlayerController>().heal(healAmount);
                Destroy(gameObject);
            }

            else if (type == pickupType.key)
            {
                //GameManager.Instance.keyCount++;
                //GameManager.Instance.UpdateKeyCount();
                interactable?.Interact();
                Destroy(gameObject);
            }

            else if (type == pickupType.stealth)
            {
                //GameManager.Instance.StealthTimer(10.0f);
                Destroy(gameObject);
            }

            else if (interactable != null)
            {
                // gun.ammoCur = gun.ammoMax;
                // GunManager.Instance.GetGunStats(gun);
                Destroy(gameObject);
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            Interact(other);

        }
    }
}