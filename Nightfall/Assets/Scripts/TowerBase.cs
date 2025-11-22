using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TowerBase : MonoBehaviour, IDamage
{

    [SerializeField] public enum TowerType { Crop, Defensive, Sprinkler, Farmland };

    [SerializeField] public TowerType typeTower;
    [SerializeField] private RuleTile[] cropPhases;
    [SerializeField] Tilemap map;
    private Vector3Int cellPos;
    [SerializeField] int hpMax;
    [SerializeField] int damage;
    [SerializeField] int healAmt;
    [SerializeField] int phases;

    [SerializeField] float attSpeed;
    [SerializeField] float healSpeed;

    [SerializeField] ItemData itemDrop;
    [SerializeField] GameObject bullet;
    [SerializeField] GameObject shootPos;
    [SerializeField] Renderer model;

    [SerializeField] AudioClip[] audBreak;
    [SerializeField] AudioSource audSrc;

    Color colorOrig;

    public bool isFullyGrown = false;
    bool EnemyInRange = false;
    public bool isWatered = false;
    public bool isFertilized = false;
    private bool tutorialChecked = false;

    int towerPhase = 0;
    int enemiesInRange;
    int dayPlanted;
    public int hp = 5;
    [SerializeField] List<Transform> enemyPos;

    float shootTime;
    float healTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {

        colorOrig = model.material.color;
        map = (Tilemap)FindAnyObjectByType(typeof(Tilemap));
        cellPos = map.WorldToCell(transform.position);
        if (typeTower == TowerType.Sprinkler)
        {

            //enemyPos = new List<Transform>();

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

        if (hp < hpMax)
        {
            healTime += Time.deltaTime;

            if (healTime >= healSpeed)
            {
                Heal();
                healTime = 0f;
            }
                

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
            if (!isWatered) return;
            if (GameManager.instance != null && dayPlanted < GameManager.instance.GetDay() && !isFullyGrown)
            {
                Grow();
                dayPlanted++;
            }
                
        }

    }

    private void AddItemToInventory(Inventory inventory, ItemData item, int amount)
    {
        foreach (InventorySlot slot in inventory.InventorySlots)
        {
            if (slot.ItemData == item && slot.RoomLeftInStack(amount))
            {
                slot.AddToStack(amount);
                inventory.NotifySlotChanged(slot);
                return;
            }
        }

        foreach (InventorySlot slot in inventory.InventorySlots)
        {
            if (slot.ItemData == null)
            {
                slot.UpdateInventorySlot(item, amount);
                inventory.NotifySlotChanged(slot);
                return;
            }
        }

    }

    public void HarvestCrop(Inventory invent)
    {

        AddItemToInventory(invent, itemDrop, itemDrop.harvestAmount);

        Destroy(gameObject);

    }

    public void Grow()
    {
        if (!isWatered || isFullyGrown)
            return;

        towerPhase++;

        if (towerPhase < cropPhases.Length)
        {
            map.SetTile(cellPos, cropPhases[towerPhase]);
        }
        else
        {
            isFullyGrown = true;
            towerPhase = cropPhases.Length - 1;
            map.SetTile(cellPos, cropPhases[towerPhase]);
        }
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
        CheckTutorialState();

    }

    public void Fertilize()
    {
        isFertilized = true;
        CheckTutorialState();
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
            //towerScript.WaterCrop();

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
        StartCoroutine(flashRed());
        if (audBreak.Length > 0)
            audSrc.PlayOneShot(audBreak[Random.Range(0, audBreak.Length)]);

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

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    void Heal()
    {
        if (typeTower == TowerType.Defensive)
            takeDamage(1);
        else
            hp += healAmt;
        if (hp > hpMax)
        {
            hp = hpMax;
        }
    }
    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }

    private void CheckTutorialState()
    {
        if (tutorialChecked)
            return;

        if(isFertilized && isWatered)
        {
            tutorialChecked = true;

            if(TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnCropFullyPlanted();
            }
        }
       
    }

}
