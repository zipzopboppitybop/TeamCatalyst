using UnityEngine;



namespace Catalyst.GamePlay.Weapons
{
    public class Swingable : MonoBehaviour
    {


        private AudioSource aud;
        [SerializeField] private float swingForce = 10f;
        [SerializeField] private int damageAmount = 5;
        private MeshCollider weaponCollider;

        private void Awake()
        {
            aud = GetComponent<AudioSource>();
            weaponCollider = GetComponent<MeshCollider>();
            SetColliderToggleAs(false);

        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Destructible"))
            {
                ApplyDamage(collision.collider, damageAmount, collision.GetContact(0).point, collision.GetContact(0).normal);

            }
        }

        private void ApplyDamage(Collider other, float damage, Vector3 hitPoint, Vector3 hitDirection)
        {

            Rigidbody rb = other.attachedRigidbody;
            if (rb != null)
            {
                rb.AddForceAtPosition(hitDirection.normalized * swingForce, hitPoint, ForceMode.Impulse);

            }


            IDamage dmg = other.GetComponent<IDamage>();
            if (dmg != null)
            {
                dmg.takeDamage(damageAmount); // Example damage value
            }
        }

        public bool ColliderState(bool state)
        {
            if (weaponCollider != null)
            {
                weaponCollider.enabled = state;
                Debug.Log("Collider state set to: " + state);
                return true;
            }
            return false;
        }

        public void SetColliderToggleAs(bool state)
        {
            if (weaponCollider != null)
            {
                weaponCollider.enabled = state;
            }
        }

    }
}
