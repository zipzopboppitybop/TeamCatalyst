
using Catalyst.Player;
using UnityEngine;


namespace Catalyst.GamePlay
{
    public class GunManager : MonoBehaviour
    {
        [SerializeField] private Renderer gunModel;
        [SerializeField] private Transform gunPos;
        [SerializeField] private PlayerData player;
        [SerializeField] private Transform gunHolder;
        [SerializeField] private Animator animator;
        [SerializeField] private AudioSource aud;
        [SerializeField] private InputHandler playerInputHandler;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private AudioClip magPickupSound;

        private WeaponData _currentWeapon;

        private AudioClip[] shootSound;



        [SerializeField] private LayerMask ignoreLayer;

        private int _gunListPos;

        private int _animAim;
        private int _animShoot;
        private int _animReload;
        private int _animArmed;


        private float _shootTimer = 0f;

        private bool isReloading = false;



        private void Awake()
        {
            if (playerInputHandler == null)
            {
                playerInputHandler = GetComponent<InputHandler>();
            }
            if (animator == null)
            {
                animator = gunHolder.GetComponent<Animator>();
            }
            if (aud == null)
            {
                aud = GetComponent<AudioSource>();

            }
            SetupCombatAnimator();
        }
        private void Start()
        {

            EquipGun();



        }

        private void Update()
        {
            _shootTimer += Time.deltaTime;

            if (player.CurrentGun != null)
            {
                SelectWeapon();
            }

            if (player.Guns.Count == 0 || player.CurrentGun == null)
            {
                HealthBarUI.instance.HideWeaponUI();
                return;
            }

        }



        private void EquipGun()
        {


            ChangeWeapon();
            //animator.SetBool(_animArmed, true);

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
            shootSound = player.CurrentGun.shootSounds;
            Debug.Log("Equipped " + _currentWeapon.name);
            gunModel.GetComponent<MeshFilter>().sharedMesh = player.CurrentGun.model.GetComponent<MeshFilter>().sharedMesh;
            gunModel.GetComponent<MeshRenderer>().sharedMaterial = player.CurrentGun.model.GetComponent<MeshRenderer>().sharedMaterial;

            if (_currentWeapon.ammoCur > 0)
                aud.PlayOneShot(player.CurrentGun.pickUpSound, 0.5f);

            player.ShootDamage = player.CurrentGun.shootDamage;
            player.ShootDist = player.CurrentGun.shootDistance;
            player.ShootRate = player.CurrentGun.shootRate;
            player.AmmoCount = player.CurrentGun.ammoCur;
            player.AmmoMax = player.CurrentGun.ammoMax;

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
        private void SetupCombatAnimator()
        {
            _animAim = Animator.StringToHash("Aiming");
            _animShoot = Animator.StringToHash("Shoot");
            _animReload = Animator.StringToHash("isReloading");
            _animArmed = Animator.StringToHash("isArmed");
        }

        private bool PlayerCanShoot()
        {

            if (player.Guns.Count == 0)
            {
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
                animator.SetBool(_animArmed, false);
                return;
            }
            if (playerInputHandler.AimHeld)
            {
                animator.SetBool(_animAim, true);
                Debug.Log("Aiming");
                HandleShoot();

            }
            else
            {
                animator.SetBool(_animAim, false);

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
            if (playerInputHandler.FireHeld && _shootTimer > player.ShootRate && PlayerCanShoot())
            {
                _shootTimer = 0f;

                if (player.CurrentGun.ammoCur <= 0)
                {
                    Debug.Log("Out of Ammo!");
                    aud.PlayOneShot(player.CurrentGun.emptyClipSound, player.CurrentGun.shootVolume);
                    return;
                }
                else
                    animator.SetTrigger(_animShoot);

                Debug.Log("Shooting");

                //animator.ResetTrigger("Shoot");

            }
        }

        public void Shoot()
        {
            player.CurrentGun.ammoCur--;
            aud.PlayOneShot(player.CurrentGun.shootSounds[Random.Range(0, player.Guns[_gunListPos].shootSounds.Length)], player.CurrentGun.shootVolume);

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

            if (player.MagazineSize > 0)
            {
                player.MagazineSize--;
                isReloading = true;
                animator.SetTrigger(_animReload);
                Debug.Log("Reloading...");
                FinishReload();
            }

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

    }
}