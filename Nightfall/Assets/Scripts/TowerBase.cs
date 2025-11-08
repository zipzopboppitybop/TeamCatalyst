using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using Unity.VisualScripting;

public class TowerBase : MonoBehaviour, IDamage
{

    [SerializeField] public enum TowerType { Crop, Defensive, Sprinkler, Farmland };

    [SerializeField] public TowerType typeTower;

    [SerializeField] Tilemap map;

    [SerializeField] int hpMax;
    [SerializeField] int damage;
    [SerializeField] int healAmt;

    [SerializeField] float attSpeed;
    [SerializeField] float healSpeed;

    [SerializeField] GameObject itemDrop;
    [SerializeField] GameObject bullet;
    [SerializeField] GameObject shootPos;

    bool isFullyGrown = false;
    bool EnemyInRange = false;
    public bool isWatered = false;
    public bool isFertilized = false;
    bool isHealing = false;

    int enemiesInRange;
    int dayPlanted;
    public int hp = 5;
    [SerializeField] List<Transform> enemyPos;

    float shootTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {

        map = (Tilemap)FindAnyObjectByType(typeof(Tilemap));

        if (typeTower == TowerType.Sprinkler)
        {

            //enemyPos = new List<Transform>();

        }
        if (typeTower == TowerType.Farmland)
        {

            Fertilize();

        }
        if (typeTower == TowerType.Crop)
        {

            if (GameManager.instance != null)
            {
                GameManager.instance.UpdateCropCount(1);
                dayPlanted = GameManager.instance.GetDay();
            }

        }

    }

    // Update is called once per frame
    void Update()
    {

        if (hp < hpMax && !isHealing)
        {

            StartCoroutine(Heal());

        }

        if (typeTower == TowerType.Sprinkler)
        {        
            shootTime += Time.deltaTime;

            if (shootTime >= attSpeed && EnemyInRange)
            {

                FaceTarget();
                Shoot();
                shootTime = 0;

            }

        }

        if (typeTower == TowerType.Crop)
        {
            if (GameManager.instance != null && dayPlanted < GameManager.instance.GetDay() && !isFullyGrown)
                Grow();
        }

    }

    public void Grow()
    {

        if (isWatered) 
            isFullyGrown = true;

    }

    Vector3 ChooseClosestPos()
    {

        float shortestDistance = 10000;
        Vector3 closestPos = Vector3.zero;

        for (int i = 0; i < enemyPos.Count; i++)
        {

            if (enemyPos[i] != null && Vector3.Distance(enemyPos[i].position, transform.position) < shortestDistance)
            {

                shortestDistance = Vector3.Distance(enemyPos[i].position, transform.position);
                closestPos = enemyPos[i].position;

            }

        }

        if (closestPos == Vector3.zero)
        {

            EnemyInRange = false;
            enemiesInRange = 0;

        }

        return closestPos;

    }

    void Shoot()
    {

        Instantiate(bullet, shootPos.transform.position, transform.rotation);

    }

    void FaceTarget()
    {

        Vector3 chosenEnemyPos = ChooseClosestPos() - transform.position;
        Quaternion rot = Quaternion.LookRotation(chosenEnemyPos);
        transform.rotation = rot;

    }

    public void WaterCrop()
    {

        isWatered = true;

    }

    public void Fertilize()
    {
        isFertilized = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("Enemy"))
        {

            enemiesInRange++;
            enemyPos.Add(other.transform);
            CheckForEnemies();

        }
        if (other.GetComponent<TowerBase>())
        {

            TowerBase towerScript = other.GetComponent<TowerBase>();
            towerScript.WaterCrop();

        }

    }

    private void OnTriggerExit(Collider other)
    {
        
        if (other.CompareTag("Enemy"))
        {

            enemiesInRange--;
            enemyPos.Remove(other.transform);
            CheckForEnemies();

        }
        
    }

    void CheckForEnemies()
    {

        if (enemiesInRange > 0)
        {
            EnemyInRange = true;
        }
        else
        {
            EnemyInRange = false;
        }

    }

    public void takeDamage(int amount)
    {

        hp -= amount;
        if (hp <= 0)
        {
            if (typeTower == TowerType.Crop && isFullyGrown)
                Instantiate(itemDrop, transform.position, transform.rotation);

            if (map)
                map.SetTile(map.WorldToCell(transform.position), null);

            if (GameManager.instance != null)
                GameManager.instance.UpdateCropCount(-1);
            Destroy(gameObject);
            
        }

    }

    IEnumerator Heal()
    {
        isHealing = true;
        if (typeTower == TowerType.Defensive)
            takeDamage(1);
        else
            hp += healAmt;
        if (hp > hpMax)
        {
            hp = hpMax;
        }
        yield return new WaitForSeconds(healSpeed);
        isHealing = false;
    }

}
