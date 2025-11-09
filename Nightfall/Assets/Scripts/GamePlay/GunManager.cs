using System.Collections;
using System.Collections.Generic;
using Catalyst.Audio;
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

        [SerializeField] private AudioClip shootSound;


        [SerializeField] private LayerMask ignoreLayer;

        private int _gunListPos;

        private int _animAim;
        private int _animShoot;

        private float _shootTimer = 0f;

        private bool isReloading = false;

        public WeaponData CurrentWeapon => _currentWeapon;

        private void Start()
        {
            EquipGun();
            SetupCombatAnimator();

        }

        private void Update()
        {
            _shootTimer += Time.deltaTime;
            HandleAim();
            SelectWeapon();
        }



        private void EquipGun()
        {
            if (player.Guns.Count == 0) return;

            _currentWeapon = player.Guns[0];

        }

        public void GetWeaponData(WeaponData weaponData)
        {
            //if (weapons.HasItem(weaponData))
            //{

            //}
            player.Guns.Add(weaponData);
            _gunListPos = player.Guns.IndexOf(weaponData);
            ChangeWeapon();
        }
        private void ChangeWeapon()
        {
            _currentWeapon = player.Guns[_gunListPos];
            Debug.Log("Equipped " + _currentWeapon.name);
            gunModel.GetComponent<MeshFilter>().sharedMesh = player.Guns[_gunListPos].model.GetComponent<MeshFilter>().sharedMesh;
            gunModel.GetComponent<MeshRenderer>().sharedMaterial = player.Guns[_gunListPos].model.GetComponent<MeshRenderer>().sharedMaterial;

            SoundManager.instance.playSFX(transform, new AudioClip[] { player.Guns[_gunListPos].pickUpSound }, 0.5f, 1f, 128);

        }
        private void SelectWeapon()
        {
            if (playerInputHandler.NextTriggered)
            {
                _gunListPos++;
                if (_gunListPos >= player.Guns.Count)
                {
                    _gunListPos = 0;
                }
                ChangeWeapon();
            }
            else if (playerInputHandler.PrevTriggered && !playerInputHandler.ToggleInventoryTriggered)
            {
                _gunListPos--;
                if (_gunListPos < 0)
                {
                    _gunListPos = player.Guns.Count - 1;
                }
                ChangeWeapon();
            }

        }
        private void SetupCombatAnimator()
        {
            _animAim = Animator.StringToHash("Aiming");
            _animShoot = Animator.StringToHash("Shoot");
        }

        private bool PlayerCanShoot()
        {
            return player.Guns[_gunListPos].ammoCur > 0 && !isReloading;
        }


        private void HandleAim()
        {

            if (!PlayerCanShoot())
            {
                animator.SetBool(_animAim, false);
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

                Shoot();
                animator.SetTrigger(_animShoot);

                Debug.Log("Shooting");

                //animator.ResetTrigger("Shoot");

            }
        }

        public void Shoot()
        {

            _shootTimer = 0;
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

    }
}