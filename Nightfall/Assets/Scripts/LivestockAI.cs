using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Livestock : MonoBehaviour, IDamage
{

    [SerializeField] protected Renderer model;
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] protected Transform headPos;
    [SerializeField] protected GameObject itemDrop;
    [SerializeField] protected GameObject targetObj;
    [SerializeField] protected float cropSearchInterval;
    [SerializeField] protected float cropDetectionRadius;
    [SerializeField] protected float cropThreshold;
    [SerializeField] protected Animator animator;
    [SerializeField] protected int hp;
    [SerializeField] protected int hpMax;
    [SerializeField] protected float hunger;
    [SerializeField] protected float hungerMax;
    [SerializeField] protected float hungerRate;
    [SerializeField] protected int faceTargetSpeed;
    [SerializeField] protected int FOV;
    [SerializeField] protected int roamDist;
    [SerializeField] protected int roamPauseTime;
    [Range(0, 100)][SerializeField] protected int dropChance;
    [SerializeField] protected float biteRate;
    public Vector3 homePos;
    public Chest FeedingTrough;
    protected bool isScared = false;

    Color colorOrig;

    protected float biteTimer;
    protected float roamTimer;
    protected float stoppingDistOrg;
    protected int roamTimeOrig;

    protected Vector3 startingPos;

    [SerializeField] protected AudioSource aud;
    [SerializeField] protected AudioClip[] audIdles;
    [SerializeField] protected AudioClip[] audHurt;
    [SerializeField] protected float idleSoundMinDelay;
    [SerializeField] protected float idleSoundMaxDelay;
    protected float idleSoundTimer = 0f;
    protected float nextIdleSoundTime = 0f;

    protected virtual void Start()
    {
        colorOrig = model.material.color;
        roamTimeOrig = roamPauseTime;
        stoppingDistOrg = agent.stoppingDistance;
        startingPos = transform.position;

        nextIdleSoundTime = Random.Range(idleSoundMinDelay, idleSoundMaxDelay);
    }

    protected virtual void Update()
    {
        if (agent == null || !agent.isOnNavMesh) return;

        if (agent.remainingDistance < 0.01f)
            roamTimer += Time.deltaTime;

        HandleIdleSound();

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
        UpdateMovement();
    }

    public void takeDamage(int amount)
    {
        hp -= amount;
        aud.Stop();
        aud.clip = audHurt[Random.Range(0, audHurt.Length)];
        aud.Play();
        StartCoroutine(flashRed());
        StartCoroutine(getScared());
        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }

    protected void Roam()
    {
        roamTimer = 0;
        agent.stoppingDistance = 0;

        Vector3 ranPos = Random.insideUnitSphere * roamDist + startingPos;

        if (NavMesh.SamplePosition(ranPos, out NavMeshHit hit, roamDist, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    protected void CheckRoam()
    {
        if (roamTimer >= roamPauseTime && agent.remainingDistance < 0.01f)
        {
            Roam();
        }
    }

    protected void HeadHome()
    {
        agent.SetDestination(homePos);
    }

    protected void FindFood()
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

    protected void EatCrop(InventorySlot cropSlot)
    {
        PlayEatAnimation();

        cropSlot.RemoveFromStack(1);

        if (cropSlot.StackSize <= 0)
        {
            cropSlot.UpdateInventorySlot(null, 0);
        }

        FeedingTrough.PrimaryInventory.OnInventorySlotChanged?.Invoke(cropSlot);
    }
    void PlayEatAnimation()
    {
        if (animator != null)
        {
            return;
        }

        animator.SetTrigger("Chewing");
    }

    void UpdateMovement()
    {
        if ( animator == null || agent == null)
        {
            return ;
        }
        bool isMoving = agent.velocity.sqrMagnitude > 1;
        animator.SetBool("Walking", isMoving);
    }

    protected IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }

    protected virtual void HandleIdleSound()
    {
        idleSoundTimer += Time.deltaTime;

        if (idleSoundTimer >= nextIdleSoundTime)
        {
            aud.PlayOneShot(audIdles[Random.Range(0, audIdles.Length)]);
            idleSoundTimer = 0f;
            nextIdleSoundTime = Random.Range(idleSoundMinDelay, idleSoundMaxDelay);
        }
    }

    protected IEnumerator getScared()
    {
        isScared = true;
        roamPauseTime = 1;
        yield return new WaitForSeconds(10);
        roamPauseTime = roamTimeOrig;
        isScared = false;
    }

    public void OnDayStart()
    {
        if (hp <= 0) return;

        if (hunger <= 30) return;

        DropItem();
    }

    protected void DropItem()
    {
        Vector3 dropPosition = transform.position + new Vector3(0, 1f, 0);
        Instantiate(itemDrop, dropPosition, Quaternion.identity);
    }
}