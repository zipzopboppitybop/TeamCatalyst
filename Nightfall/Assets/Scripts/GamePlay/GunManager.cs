
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

        private WeaponData _currentWeapon;

        private AudioClip[] shootSound;


        [SerializeField] private LayerMask ignoreLayer;

        private int _gunListPos;

        private int _animAim;
        private int _animShoot;
        //private int _animReload;
        private int _animArmed;

        private float _shootTimer = 0f;

        private bool isReloading = false;

        public WeaponData CurrentWeapon => _currentWeapon;

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

            if (CurrentWeapon != null)
            {
                SelectWeapon();
            }

        }



        private void EquipGun()
        {
            if (player.Guns.Count == 0)
            {
                HealthBarUI.instance.HideWeaponUI();
                return;
            }

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
            _currentWeapon = player.Guns[_gunListPos];
            player.CurrentGun = _currentWeapon;
            shootSound = player.Guns[_gunListPos].shootSounds;
            Debug.Log("Equipped " + _currentWeapon.name);
            gunModel.GetComponent<MeshFilter>().sharedMesh = player.Guns[_gunListPos].model.GetComponent<MeshFilter>().sharedMesh;
            gunModel.GetComponent<MeshRenderer>().sharedMaterial = player.Guns[_gunListPos].model.GetComponent<MeshRenderer>().sharedMaterial;

            if (_currentWeapon.ammoCur > 0)
                aud.PlayOneShot(player.Guns[_gunListPos].pickUpSound, 0.5f);

            player.ShootDamage = player.Guns[_gunListPos].shootDamage;
            player.ShootDist = player.Guns[_gunListPos].shootDistance;
            player.ShootRate = player.Guns[_gunListPos].shootRate;
            player.AmmoCount = player.Guns[_gunListPos].ammoCur;
            player.AmmoMax = player.Guns[_gunListPos].ammoMax;

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
        }
        private void SetupCombatAnimator()
        {
            _animAim = Animator.StringToHash("Aiming");
            _animShoot = Animator.StringToHash("Shoot");
            //_animReload = Animator.StringToHash("isReloading");
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
            if (player.Guns[_gunListPos].ammoCur <= 0) return false;
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
            if (playerInputHandler.AimTriggered)
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

        private void HandleShoot()
        {
            if (playerInputHandler.FireTriggered && _shootTimer > player.ShootRate && PlayerCanShoot())
            {
                _shootTimer = 0f;

                animator.SetTrigger(_animShoot);

                Debug.Log("Shooting");

                //animator.ResetTrigger("Shoot");

            }
        }

        public void Shoot()
        {
            player.Guns[_gunListPos].ammoCur--;
            aud.PlayOneShot(player.Guns[_gunListPos].shootSounds[Random.Range(0, player.Guns[_gunListPos].shootSounds.Length)], player.Guns[_gunListPos].shootVolume);
            //updatePlayerUI();

            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, player.ShootDist, ~ignoreLayer))
            {
                Instantiate(player.Guns[_gunListPos].hitEffect, hit.point, Quaternion.identity);

                Debug.Log(hit.collider.name);

                IDamage dmg = hit.collider.GetComponent<IDamage>();

                if (dmg != null)
                {

                    dmg.takeDamage(player.ShootDamage);
                }
                if (hit.rigidbody != null)
                {
                    hit.rigidbody.AddForce(-hit.normal * CurrentWeapon.impactForce);
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
            if (player.Guns[_gunListPos].ammoCur >= player.Guns[_gunListPos].ammoMax) return;
            isReloading = true;
            animator.SetTrigger("Reload");
            Debug.Log("Reloading...");
        }

        public void FinishReload()
        {
            if (player.Guns.Count == 0) return;
            if (_gunListPos < 0 || _gunListPos >= player.Guns.Count) return;
            WeaponData gun = player.Guns[_gunListPos];
            int ammoNeeded = gun.ammoMax - gun.ammoCur;
            gun.ammoCur += ammoNeeded;
            if (gun.ammoCur > gun.ammoMax)
            {
                gun.ammoCur = gun.ammoMax;
            }
            isReloading = false;
            Debug.Log("Reloaded.");
        }

        public bool ReloadWeapon(WeaponData weaponData)
        {
            WeaponData gun = player.Guns[player.Guns.IndexOf(weaponData)];

            if (gun.ammoCur == gun.ammoMax) return false;

            else
            {
                int ammoNeeded = gun.ammoMax - gun.ammoCur;
                gun.ammoCur += ammoNeeded;
                if (gun.ammoCur > gun.ammoMax)
                {
                    gun.ammoCur = gun.ammoMax;
                }
                aud.PlayOneShot(gun.reloadSound, gun.shootVolume);
                return true;
            }

        }

    }
}