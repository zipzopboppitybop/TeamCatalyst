using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class TowerBase : MonoBehaviour, IDamage
{

    [SerializeField] enum TowerType { Crop, Defensive, Offensive };

    [SerializeField] TowerType typeTower;

    [SerializeField] int hp;
    [SerializeField] int damage;

    [SerializeField] float attSpeed;

    [SerializeField] GameObject itemDrop;
    [SerializeField] GameObject bullet;
    [SerializeField] GameObject shootPos;

    bool isFullyGrown;
    bool EnemyInRange;

    int enemiesInRange;
    [SerializeField] List<Transform> enemyPos;

    float shootTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        
        if (typeTower == TowerType.Offensive)
        {

            //enemyPos = new List<Transform>();

        }

    }

    // Update is called once per frame
    void Update()
    {

        if (typeTower == TowerType.Offensive)
        {        
            shootTime += Time.deltaTime;

            if (shootTime >= attSpeed)
            {

                Vector3 chosenEnemyPos = ChooseClosestPos();
                Quaternion rot = Quaternion.LookRotation(chosenEnemyPos);
                transform.rotation = rot;
                Shoot();
                shootTime = 0;

            }
        }

    }

    void Grow()
    {

        isFullyGrown = true;

    }

    Vector3 ChooseClosestPos()
    {

        float shortestDistance = 1000;
        Vector3 closestPos = Vector3.zero;

        for (int i = 0; i < enemyPos.Count; i++)
        {

            if ((enemyPos[i].position - transform.position).normalized.magnitude < shortestDistance)
            {

                shortestDistance = (enemyPos[i].position - transform.position).normalized.magnitude;
                closestPos = enemyPos[i].position;

            }

        }

        return closestPos;

    }

    void Shoot()
    {

        Instantiate(bullet, shootPos.transform.position, transform.rotation);

    }



    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("Enemy"))
        {

            enemiesInRange++;
            enemyPos.Add(other.transform);
            CheckForEnemies();

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
            if (typeTower == TowerType.Crop)
                Instantiate(itemDrop, transform.position, transform.rotation);
            Destroy(gameObject);
        }

    }

}
