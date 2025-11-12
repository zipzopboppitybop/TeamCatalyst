using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Livestock : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform headPos;
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
    [SerializeField] Transform homePosTransform;
    protected Vector3 homePos;

    Color colorOrig;

    float biteTimer;
    float roamTimer;
    float stoppingDistOrg;
    int roamTimeOrig;

    Vector3 startingPos;

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
        else if (hunger <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void takeDamage(int amount)
    {
        hp -= amount;
        StartCoroutine(flashRed());
        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }

    void Roam()
    {
        roamTimer = 0;
        agent.stoppingDistance = 0;

        Vector3 ranPos = Random.insideUnitSphere * roamDist + startingPos;

        if (NavMesh.SamplePosition(ranPos, out NavMeshHit hit, roamDist, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
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

            if (Vector3.Distance(transform.position, FeedingTrough.transform.position) <= agent.stoppingDistance + 0.5f)
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
}