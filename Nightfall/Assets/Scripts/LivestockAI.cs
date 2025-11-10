using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Livestock : MonoBehaviour, IDamage
{
    // need boolean to check if this attacks plants or not
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform headPos;
    [SerializeField] Transform homePosTransform;
    [SerializeField] GameObject itemDrop;
    [SerializeField] GameObject targetObj;
    [SerializeField] float cropSearchInterval;
    [SerializeField] float cropDetectionRadius;
    [SerializeField] float cropThreshold;
    [SerializeField] private Chest FeedingTrough;


    [SerializeField] int hp;
    [SerializeField] int hpMax;
    [SerializeField] float hunger;
    [SerializeField] float hungerMax;
    [SerializeField] float hungerRate;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int FOV;
    [SerializeField] int roamDist;
    [SerializeField] int roamPauseTime;
    [Range(0, 100)][SerializeField] int dropChance;
    [SerializeField] float biteRate;

    Color colorOrig;

    bool playerInRange;
    [SerializeField] bool targetsPlayer = true;
    bool isScared = false;
    bool headHome = false;

    float biteTimer;
    float roamTimer;
    float angleToTarget;
    float stoppingDistOrg;
    float cropSearchTimer;

    int roamTimeOrig;

    Vector3 targetDir;
    Vector3 startingPos;
    Vector3 homePos;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrig = model.material.color;
        roamTimeOrig = roamPauseTime;
        stoppingDistOrg = agent.stoppingDistance;
        startingPos = transform.position;
        homePos = homePosTransform.position;
    }
    void Update()
    {
        if (agent == null || !agent.isOnNavMesh) return;

        if (agent.remainingDistance < 0.01f)
            roamTimer += Time.deltaTime;



        if (!GameManager.instance.IsNight)
        {
            CheckRoam();
            hunger -= hungerRate;
        }
        else
        {
            HeadHome();
        }

        if (hunger <= 50)
        {
            FindFood();
        }
        else if (hunger <=0)
        {
            Destroy(gameObject);
        }
    }

    public void takeDamage(int amount)
    {
        hp -= amount;

        StopCoroutine(getScared());
        isScared = false;
        StartCoroutine(getScared());
        Roam();


        if (hp <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(flashRed());
        }
    }

    void Roam()
    {
        roamTimer = 0;
        agent.stoppingDistance = 0;

        Vector3 ranPos = Random.insideUnitSphere * roamDist;
        ranPos += startingPos;

        NavMeshHit hit;
        NavMesh.SamplePosition(ranPos, out hit, roamDist, 1);
        agent.SetDestination(hit.position);
    }

    void CheckRoam()
    {
        if (roamTimer >= roamPauseTime && agent.remainingDistance < 0.01f)
        {
            Roam();
        }
    }

    void HeadHome()
    {
        headHome = true;
        agent.SetDestination(homePos);
    }

    void FindFood()
    {
        if (FeedingTrough == null || FeedingTrough.PrimaryInventory == null) return;

        InventorySlot cropSlot = null;

        foreach (var slot in FeedingTrough.PrimaryInventory.InventorySlots)
        {
            if (slot.ItemData != null && slot.ItemData.itemType == ItemData.ItemType.Resource && slot.StackSize > 0)
            {
                cropSlot = slot;
                break;
            }
        }

        if (cropSlot != null)
        {
            agent.SetDestination(FeedingTrough.transform.position);

            float dist = Vector3.Distance(transform.position, FeedingTrough.transform.position);
            if (dist <= agent.stoppingDistance + 0.5f)
            {
                EatCrop(cropSlot);
            }
        }
        else
        {
            CheckRoam();
        }
    }

    void EatCrop(InventorySlot cropSlot)
    {
        cropSlot.RemoveFromStack(1);

        if (cropSlot.StackSize <= 0)
        {
            cropSlot.UpdateInventorySlot(null, 0);
        }

        FeedingTrough.PrimaryInventory.OnInventorySlotChanged?.Invoke(cropSlot);
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }

    IEnumerator getScared()
    {

        isScared = true;
        roamPauseTime = 1;
        yield return new WaitForSeconds(10);
        roamPauseTime = roamTimeOrig;
        isScared = false;
    }
}
