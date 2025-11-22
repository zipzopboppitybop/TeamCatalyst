
using Catalyst.GamePlay.Weapons;
using Catalyst.Player;
using Catalyst.Player.Handlers;
using UnityEngine;


namespace Catalyst.GamePlay
{
    public class GunManager : MonoBehaviour
    {
        public static GunManager Instance;
        [SerializeField] private GameObject gunModel;
        [SerializeField] private Transform gunPos;
        [SerializeField] private PlayerData player;
        [SerializeField] private AudioClip magPickupSound;
        [SerializeField] private LayerMask ignoreLayer;
        private AudioSource aud;
        private InputHandler playerInputHandler;
        //private PlayerController playerController;
        private AnimationHandler anim;
        private Swingable swingableWeapon;


        private int _gunListPos;

        private float _shootTimer = 0f;

        public bool isReloading = false;
        public bool isArmed = false;


        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            aud = GetComponent<AudioSource>();
            playerInputHandler = GetComponent<InputHandler>();
            //playerController = GetComponent<PlayerController>();
            anim = GetComponent<AnimationHandler>();
            swingableWeapon = gunModel.GetComponent<Swingable>();

        }
        private void Start()
        {
            ClearCurrentGun();
            EquipGun();
        }

        private void Update()
        {
            _shootTimer += Time.deltaTime;



            if (player.Guns.Count == 0 || player.CurrentGun == null)
            {
                HealthBarUI.instance.HideWeaponUI();
                gunModel.SetActive(false);
                return;
            }
            else
            {
                SelectWeapon();
            }

        }

        private void EquipGun()
        {
            if (player.Guns.Count == 0)
            {
                isArmed = false;
                return;
            }
            ChangeWeapon();
        }
        private void ClearCurrentGun()
        {
            player.CurrentGun = null;
        }


        public void GetWeaponData(WeaponData weaponData)
        {
            player.Guns.Add(weaponData);
            _gunListPos = player.Guns.IndexOf(weaponData);
            ChangeWeapon();

        }

        public WeaponData ExistingWeaponData(WeaponData weaponData)
        {
            foreach (WeaponData gun in player.Guns)
            {
                if (gun == weaponData)
                {
                    return gun;
                }
            }
            return null;
        }
        private void ChangeWeapon()
        {
            player.CurrentGun = player.Guns[_gunListPos];

            //gunModel = Instantiate(player.CurrentGun.model, gunPos.position, gunPos.rotation, gunPos);
            //gunModel.GetComponent<MeshFilter>().sharedMesh = player.CurrentGun.model.GetComponent<MeshFilter>().sharedMesh;
            //gunModel.GetComponent<MeshRenderer>().sharedMaterial = player.CurrentGun.model.GetComponent<MeshRenderer>().sharedMaterial;

            if (player.CurrentGun.ammoCur > 0)
                aud.PlayOneShot(player.CurrentGun.pickUpSound, 0.5f);

            player.ShootDamage = player.CurrentGun.shootDamage;
            player.ShootDist = player.CurrentGun.shootDistance;
            player.ShootRate = player.CurrentGun.shootRate;
            player.AmmoCount = player.CurrentGun.ammoCur;
            player.AmmoMax = player.CurrentGun.ammoMax;

            if (player.CurrentGun.gunType == WeaponData.GunType.Shotgun)
                gunModel.SetActive(true);
            isArmed = true;
            HealthBarUI.instance.ShowWeaponUI();

        }
        private void SelectWeapon()
        {
            //if (playerInputHandler.NextTriggered)
            //{
            //    _gunListPos++;
            //    if (_gunListPos >= player.Guns.Count)
            //    {
            //        _gunListPos = 0;
            //    }
            //    ChangeWeapon();
            //}
            //else if (playerInputHandler.PrevTriggered)
            //{
            //    _gunListPos--;
            //    if (_gunListPos < 0)
            //    {
            //        _gunListPos = player.Guns.Count - 1;
            //    }
            //    ChangeWeapon();
            //}
            HandleAim();
            HandleReload();

        }

        private bool PlayerCanShoot()
        {

            if (player.Guns.Count == 0)
            {
                isArmed = false;
                HealthBarUI.instance.HideWeaponUI();
                return false;
            }
            if (_gunListPos < 0 || _gunListPos >= player.Guns.Count) return false;
            if (isReloading) return false;
            return true;
        }


        private void HandleAim()
        {

            if (!PlayerCanShoot())
            {
                return;
            }
            if (playerInputHandler.AimHeld)
            {
                player.isAiming = true;
                anim.SetAiming(true);
                if (playerInputHandler.FireHeld)
                    HandleShoot();

            }
            else
            {
                player.isAiming = false;
                anim.SetAiming(false);

            }

        }
        private void HandleReload()
        {
            if (playerInputHandler.ReloadTriggered)
            {
                StartReload();
            }
        }

        private void HandleShoot()
        {
            if (_shootTimer > player.ShootRate && PlayerCanShoot())
            {
                _shootTimer = 0f;
                player.isShooting = true;
                if (player.CurrentGun.ammoCur <= 0)
                {
                    aud.PlayOneShot(player.CurrentGun.emptyClipSound, player.CurrentGun.shootVolume);
                    return;
                }
                else
                {
                    //aud.PlayOneShot(player.CurrentGun.shootSounds[Random.Range(0, player.Guns[_gunListPos].shootSounds.Length)], player.CurrentGun.shootVolume);

                    anim.TriggerShoot();

                }
            }
            else
            {
                player.isShooting = false;
            }

        }

        public void Shoot()
        {
            player.CurrentGun.ammoCur--;
            aud.PlayOneShot(player.CurrentGun.shootSounds[Random.Range(0, player.Guns[_gunListPos].shootSounds.Length)], player.CurrentGun.shootVolume);

            // Show muzzel flash
            if (player.CurrentGun.muzzleFlash != null && player.CurrentGun.muzzleFlashPosition != null)
            {
                ParticleSystem muzzleFlashInstance = Instantiate(player.CurrentGun.muzzleFlash, player.CurrentGun.muzzleFlashPosition.position, player.CurrentGun.muzzleFlashPosition.rotation);
                muzzleFlashInstance.Play();
                Destroy(muzzleFlashInstance.gameObject, muzzleFlashInstance.main.duration);
            }

            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, player.ShootDist, ~ignoreLayer))
            {
                Instantiate(player.CurrentGun.hitEffect, hit.point, Quaternion.identity);

                Debug.Log(hit.collider.name);

                IDamage dmg = hit.collider.GetComponent<IDamage>();

                if (dmg != null)
                {

                    dmg.takeDamage(player.ShootDamage);
                }
                if (hit.rigidbody != null)
                {
                    hit.rigidbody.AddForce(-hit.normal * player.CurrentGun.impactForce);
                }

            }

        }

        public void PlayShotgunLoadSound()
        {
            aud.PlayOneShot(player.CurrentGun.loadSound, player.CurrentGun.shootVolume);
        }
        public bool HasGun(WeaponData gun)
        {
            return player.Guns.Contains(gun);
        }

        public void StartReload()
        {
            if (isReloading) return;
            if (player.Guns.Count == 0) return;
            if (_gunListPos < 0 || _gunListPos >= player.Guns.Count) return;
            if (player.CurrentGun.ammoCur >= player.CurrentGun.ammoMax) return;

            isReloading = true;
            anim.TriggerReload();
        }

        public void FinishReload()
        {

            ReloadWeapon(player.CurrentGun);
            isReloading = false;
        }

        public void AddMagazine(int amount)
        {
            player.MagazineSize += amount;
            aud.PlayOneShot(magPickupSound, player.CurrentGun.shootVolume);

        }

        public void ToggleWeaponCollider()
        {
            if (player.CurrentGun != null && swingableWeapon != null)
            {
                if (swingableWeapon.ColliderState(false))
                    swingableWeapon.SetColliderToggleAs(true);
                else
                    swingableWeapon.SetColliderToggleAs(false);
            }
        }


        public bool ReloadWeapon(WeaponData weaponData)
        {
            WeaponData gun = player.Guns[player.Guns.IndexOf(weaponData)];

            if (gun.ammoCur == gun.ammoMax)
            {
                return false;
            }

            else
            {
                gun.ammoCur = gun.ammoMax;
                aud.PlayOneShot(gun.reloadSound, gun.shootVolume);
                return true;
            }

        }



        public void PlayHitSound()
        {
            if (player.CurrentGun.hitSounds.Length > 0 && aud != null)
            {
                AudioClip hitClip = player.CurrentGun.hitSounds[Random.Range(0, player.CurrentGun.hitSounds.Length)];
                aud.pitch = 1f + Random.Range(-player.CurrentGun.PitchRange, player.CurrentGun.PitchRange);
                aud.PlayOneShot(hitClip, player.CurrentGun.actionVolume);
            }
        }
        public void PlaySwingSound()
        {
            if (player.CurrentGun.swingSounds.Length > 0 && aud != null)
            {
                AudioClip swingClip = player.CurrentGun.swingSounds[Random.Range(0, player.CurrentGun.swingSounds.Length)];
                aud.pitch = 1f + Random.Range(-player.CurrentGun.PitchRange, player.CurrentGun.PitchRange);
                aud.PlayOneShot(swingClip, player.CurrentGun.actionVolume);
            }
        }
    }
}