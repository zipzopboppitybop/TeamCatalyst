using UnityEngine;



namespace Catalyst.GamePlay.Weapons
{
    public class Swingable : Damage
    {
        [SerializeField] private float swingForce = 10f;
        [SerializeField] private int damageAmount = 5;
        [SerializeField] private AudioClip[] swingSounds;
        [SerializeField] private float swingVolume = 0.7f;
        [SerializeField] private float swingPitchRange = 0.2f;
        [SerializeField] private AudioClip[] hitSounds;


        private AudioSource aud;
        private Collider weaponCollider;

        private void Awake()
        {
            aud = GetComponent<AudioSource>();
            weaponCollider = GetComponent<Collider>();
        }
        private void Start()
        {
            if (weaponCollider != null)
            {
                weaponCollider.enabled = false; // Disable collider at start
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

        public void PlaySwingSound()
        {
            if (swingSounds.Length > 0 && aud != null)
            {
                AudioClip swingClip = swingSounds[Random.Range(0, swingSounds.Length)];
                aud.pitch = 1f + Random.Range(-swingPitchRange, swingPitchRange);
                aud.PlayOneShot(swingClip, swingVolume);
            }
        }

        public void PlayHitSound()
        {
            if (hitSounds.Length > 0 && aud != null)
            {
                AudioClip hitClip = hitSounds[Random.Range(0, hitSounds.Length)];
                aud.pitch = 1f + Random.Range(-swingPitchRange, swingPitchRange);
                aud.PlayOneShot(hitClip, swingVolume);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {


            ApplyDamage(collision.collider, damageAmount, collision.GetContact(0).point, collision.relativeVelocity);
        }

        private void OnCollisionExit(Collision collision)
        {
            // Optional: Handle logic when collision ends
        }



        public void TurnOnCollider()
        {
            if (weaponCollider != null)
            {
                weaponCollider.enabled = true;
            }
            Debug.Log("Weapon collider turned on.");

        }

        public void TurnOffCollider()
        {
            if (weaponCollider != null)
            {
                weaponCollider.enabled = false;
            }
            Debug.Log("Weapon collider turned off.");
        }
    }
}
