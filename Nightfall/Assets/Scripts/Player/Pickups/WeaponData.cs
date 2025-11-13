using UnityEngine;

namespace Catalyst.Player
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "Weapon/WeaponData")]

    public class WeaponData : ScriptableObject
    {
        public string gunName;
        public GameObject model;
        public enum GunType { Handgun, Shotgun, AutoRifle, Submachinegun, SniperRifle }

        public AudioClip pickUpSound;
        public AudioClip emptyClipSound;
        public AudioClip reloadSound;
        public AudioClip[] shootSounds;
        [Range(0, 1)] public float shootVolume = 0.5f; // Volume of the shooting sound

        public GunType gunType;
        public int shootDamage;
        public int shootDistance;
        public float shootRate;
        public int ammoCur;
        [Range(5, 50)] public int ammoMax; // Maximum ammo capacity
        public float reloadTime;

        public ParticleSystem hitEffect;
        public float impactForce;
        //public ParticleSystem muzzleFlash;
        //public Transform muzzleFlashPosition;
    }

}
