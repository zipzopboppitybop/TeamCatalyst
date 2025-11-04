using System.Collections;
using Catalyst.Player;
using UnityEngine;

namespace Catalyst.GamePlay
{
    public class GunManager : MonoBehaviour
    {
        public static GunManager Instance;
        [Header("SETTINGS")]
        [Space(10)]
        [SerializeField] PlayerData playerData;
        [SerializeField] private Renderer weaponModel;
        [SerializeField] private WeaponData currentWeaponData;
        [Space(10)]
        [SerializeField] public LayerMask shootIgnoreLayer;
        [Space(10)]
        [SerializeField] AudioClip fireSound;
        private int _weaponListIndex;
        private float _shootTimer;
        private bool _isReloading;

        private Vector3 _targetRecoilRotation;
        private Vector3 _targetRecoilPosition;

        void Start()
        {
            Instance = this;
        }
        private void Update()
        {
            UpdateShoot();
            SelectWeapon();

            if (Input.GetKeyDown(KeyCode.R))
            {
                AttemptReload();
            }
        }

        public void GetGunStats(WeaponData weaponData)
        {
            if (playerData.Guns.Contains(weaponData)) { return; }

            playerData.Guns.Add(weaponData);
            _weaponListIndex = playerData.Guns.Count - 1;

            // Activate the Ammo UI 
            ChangeWeapon();
        }
        // ReSharper disable Unity.PerformanceAnalysis
        private void ChangeWeapon()
        {
            currentWeaponData = playerData.Guns[_weaponListIndex];
            playerData.CurrentGun = currentWeaponData;

            weaponModel.GetComponent<MeshFilter>().sharedMesh = playerData.Guns[_weaponListIndex].model.GetComponent<MeshFilter>().sharedMesh;
            weaponModel.GetComponent<MeshRenderer>().sharedMaterial = playerData.Guns[_weaponListIndex].model.GetComponent<MeshRenderer>().sharedMaterial;

            //SoundManager.instance.soundSource.PlayOneShot(weaponList[weaponListIndex].pickUpSound);
        }
        private void SelectWeapon()
        {
            // if (playerController.playerInputHandler.NextTriggered && _weaponListIndex < playerData.Guns.Count - 1)
            // {
            //     _weaponListIndex++;
            //     ChangeWeapon();
            // }
            // else if (playerController.playerInputHandler.PrevTriggered && _weaponListIndex > 0)
            // {
            //     _weaponListIndex--;
            //     ChangeWeapon();
            // }
        }

        private bool CheckIfGunCanShoot()
        {
            return currentWeaponData.ammoCur > 0 && !_isReloading;
        }
        private void UpdateShoot()
        {
            _shootTimer += Time.deltaTime;

            /*if (playerData.Guns.Count > 0)
            {
                if (playerController.playerInputHandler.FireTriggered && CheckIfGunCanShoot() && _shootTimer >= currentWeaponData.shootRate)
                {
                    Shoot();
                    AudioSource.PlayClipAtPoint(fireSound, transform.position);
                }
                else if (currentWeaponData.ammoCur <= 0 && !_isReloading)
                {
                    AttemptReload();
                }
            }*/
        }
        private void Shoot()
        {
            // resetting the shoot timer //
            _shootTimer = 0;
            currentWeaponData.ammoCur--;
            UpdateAmmoCount();
            PerformShoot();
            //SetTargetRecoilMultipliers();
        }
        private void PerformShoot()
        {
            /*// performing shoot raycast //
            if (Physics.Raycast(playerController.mainCamera.transform.position, playerController.mainCamera.transform.forward, out RaycastHit hit, currentWeaponData.shootDist, ~shootIgnoreLayer))
            {
                // logging the collider the raycast hit //
                Debug.Log(hit.collider.name);

                // if the collider has the IDamage interface, we store it in 'target'
                IDamage target = hit.collider.GetComponent<IDamage>();

                // null check on the target. if target is not null, we call 'TakeDamage'
                target?.TakeDamage(currentWeaponData.shootDamage);

                Instantiate(playerData.Guns[_weaponListIndex].hitEffect, hit.point, Quaternion.identity);

                if (hit.rigidbody != null)
                {
                    hit.rigidbody.AddForce(-hit.normal * currentWeaponData.impactForce);
                }
            }*/
        }

        private void UpdateAmmoCount()
        {
            playerData.AmmoCount = currentWeaponData.ammoCur;
            playerData.AmmoMax = currentWeaponData.ammoMax;
        }

        private IEnumerator ReloadSequence()
        {
            _isReloading = true;

            if (currentWeaponData.reloadSound != null)
            {
                AudioSource.PlayClipAtPoint(currentWeaponData.reloadSound, transform.position);
            }

            yield return new WaitForSeconds(currentWeaponData.reloadTime);
            currentWeaponData.ammoCur = currentWeaponData.ammoMax;
            _isReloading = false;
        }
        public void AttemptReload()
        {
            if (playerData.Guns.Count > 0)
            {
                if (_isReloading || currentWeaponData.ammoCur >= currentWeaponData.ammoMax)
                    return;

                StartCoroutine(ReloadSequence());
            }
        }


        /*private void LateUpdateRecoil(Transform recoilPivot, Transform masterIK)
        {
            if (playerData.Guns.Count > 0)
            {
                float positionX = recoilPivot.localPosition.x + currentWeaponData.recoilXPositionCurve.Evaluate(shootTimer * currentWeaponData.recoilPlayRate) * targetRecoilPosition.x;
                float positionY = recoilPivot.localPosition.y + currentWeaponData.recoilYPositionCurve.Evaluate(shootTimer * currentWeaponData.recoilPlayRate) * targetRecoilPosition.y;
                float positionZ = recoilPivot.localPosition.z + currentWeaponData.recoilZPositionCurve.Evaluate(shootTimer * currentWeaponData.recoilPlayRate) * targetRecoilPosition.z;

                float rotationX = recoilPivot.localRotation.x + currentWeaponData.recoilXRotationCurve.Evaluate(shootTimer * currentWeaponData.recoilPlayRate) * targetRecoilRotation.x;
                float rotationY = recoilPivot.localRotation.y + currentWeaponData.recoilYRotationCurve.Evaluate(shootTimer * currentWeaponData.recoilPlayRate) * targetRecoilRotation.y;
                float rotationZ = recoilPivot.localRotation.z + currentWeaponData.recoilZRotationCurve.Evaluate(shootTimer * currentWeaponData.recoilPlayRate) * targetRecoilRotation.z;

                Vector3 recoilPosition = new(positionX, positionY, positionZ);
                Vector3 recoilRotation = new(rotationX, rotationY, rotationZ);

                recoilPivot.localPosition = Vector3.Lerp(masterIK.localPosition, recoilPosition, Time.deltaTime * currentWeaponData.recoilPositionSpeed);
                recoilPivot.localRotation = Quaternion.Slerp(masterIK.localRotation, Quaternion.Euler(recoilRotation), Time.deltaTime * currentWeaponData.recoilRotationSpeed);
            }
        }
        private void SetTargetRecoilMultipliers()
        {
            targetRecoilRotation = new(currentWeaponData.recoilXRotationMultiplier, Random.Range(-currentWeaponData.recoilYRotationMultiplier, currentWeaponData.recoilYRotationMultiplier), Random.Range(-currentWeaponData.recoilZRotationMultiplier, currentWeaponData.recoilZRotationMultiplier));
            targetRecoilPosition = new(Random.Range(-currentWeaponData.recoilXPositionMultiplier, currentWeaponData.recoilXPositionMultiplier), currentWeaponData.recoilYPositionMultiplier, currentWeaponData.recoilZPositionMultiplier);
        }*/
    }
}