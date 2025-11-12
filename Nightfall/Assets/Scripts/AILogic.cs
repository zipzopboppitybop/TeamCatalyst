using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AILogic : MonoBehaviour, IDamage
{
    // need boolean to check if this attacks plants or not
    [SerializeField] protected Renderer model;
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] protected Animator animator;
    [SerializeField] protected Transform headPos;
    [SerializeField] protected GameObject itemDrop;
    [SerializeField] protected GameObject targetObj;
    [SerializeField] protected float cropSearchInterval; 
    [SerializeField] protected float cropDetectionRadius;

    [SerializeField] protected float hp;
    [SerializeField] protected int faceTargetSpeed;
    [SerializeField] protected int FOV;
    [SerializeField] protected int roamDist;
    [SerializeField] protected int roamPauseTime;
    [Range(0, 100)][SerializeField] int dropChance;
    [SerializeField] protected float biteRate;

    Color colorOrig;

    protected bool playerInRange;
    [SerializeField] protected bool targetsPlayer = true;
    protected bool isScared = false;
    protected bool isHealing;
    protected bool isPlayingSteps;

    protected float biteTimer;
    protected float roamTimer;
    protected float angleToTarget;
    protected float stoppingDistOrg;
    protected float cropSearchTimer;
    protected float hpOrig;
    protected float healRate;

    protected int roamTimeOrig;

    protected Vector3 targetDir;
    protected Vector3 startingPos;

    [SerializeField] protected AudioSource aud;
    [SerializeField] protected AudioClip[] audSteps;
    [SerializeField] protected AudioClip[] audBites;
    [SerializeField] protected AudioClip[] audIdles;
    [SerializeField] protected AudioClip[] audHurt;
    [SerializeField] protected AudioClip[] audNotice;
    [SerializeField] protected float idleSoundMinDelay;
    [SerializeField] protected float idleSoundMaxDelay;
    protected float idleSoundTimer = 0f;
    protected float nextIdleSoundTime = 0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        colorOrig = model.material.color;
        roamTimeOrig = roamPauseTime;
        stoppingDistOrg = agent.stoppingDistance;
        startingPos = transform.position;
        hpOrig = hp;
        healRate = 1;

        if (targetsPlayer)
        {
            targetObj = GameManager.instance.player;
        }

        nextIdleSoundTime = Random.Range(idleSoundMinDelay, idleSoundMaxDelay);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        biteTimer += Time.deltaTime;
        cropSearchTimer += Time.deltaTime;

        if (agent == null || !agent.isOnNavMesh) return;

        if (agent.remainingDistance <= 0.01f)
        {
            roamTimer += Time.deltaTime;
        }

        HandleIdleSound();
        //if (!targetsPlayer)
        //{
        //    if (targetObj == null || !targetObj.activeInHierarchy)
        //    {
        //        if (cropSearchTimer >= cropSearchInterval)
        //        {
        //            FindNearestCrop();
        //            cropSearchTimer = 0f;
        //        }
        //    }

        //    if (targetObj == null)
        //    {
        //        CheckRoam();
        //        return;
        //    }

        //    CanSeeTarget();
        //}
        //else
        //{
        //    if (playerInRange && !CanSeeTarget())
        //        CheckRoam();
        //    else if (!playerInRange)
        //        CheckRoam();
        //}

        //if (!targetsPlayer && targetObj == null && agent.remainingDistance < 0.5f)
        //    CheckRoam();
    }

    public virtual void takeDamage(int amount)
    {
        hp -= amount;
        if (GameManager.instance != null && targetsPlayer)
        {
            targetObj = GameManager.instance.player;
            agent.SetDestination(targetObj.transform.position);
        }
        else if (!targetsPlayer)
        {
            StopCoroutine(getScared());
            isScared = false;
            StartCoroutine(getScared());
            Roam();
        }
        

        if (hp <= 0)
        {

            if (Random.Range(1, 100) <= dropChance && itemDrop != null)
            {
                Debug.Log("Spawn!");
                Instantiate(itemDrop, headPos.position, transform.rotation);
            }

            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(flashRed());
        }
    }

    protected virtual void CheckRoam()
    {
        if (roamTimer >= roamPauseTime && agent.remainingDistance < 0.01f)
        {
            Roam();
        }
    }

    protected virtual void Roam()
    {
        roamTimer = 0;
        agent.stoppingDistance = 0;

        Vector3 ranPos = Random.insideUnitSphere * roamDist;
        ranPos += startingPos;

        NavMeshHit hit;
        NavMesh.SamplePosition(ranPos, out hit, roamDist, 1);
        agent.SetDestination(hit.position);
    }

    protected virtual bool CanSeeTarget()
    {
        if (!isScared)
        {
            targetDir = targetObj.transform.position - headPos.position;
            angleToTarget = Vector3.Angle(targetDir, transform.forward);
            Debug.DrawRay(headPos.position, targetDir, Color.red);

            RaycastHit hit;
            Debug.Log("You made it this far");
            if (Physics.Raycast(headPos.position, targetDir, out hit))
            {
                Debug.Log(hit.collider.name);
                Debug.DrawRay(headPos.position, headPos.transform.forward, Color.red);

                if (angleToTarget <= FOV)
                {
                    agent.SetDestination(targetObj.transform.position);

                    if (biteTimer > biteRate && agent.remainingDistance <= stoppingDistOrg)
                    {
                        if (targetObj)
                        {
                            attack(targetObj);
                            biteTimer = 0;
                        }
                    }

                    if (agent != null)
                    {
                        if (agent.remainingDistance <= stoppingDistOrg)
                            FaceTarget();
                    }

                    agent.stoppingDistance = stoppingDistOrg;
                    return true;
                }

            }
        }

        agent.stoppingDistance = 0;
        return false;
    }

    protected void FaceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(targetDir.x, 0, targetDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<TowerBase>() && other.GetComponent<TowerBase>().typeTower == TowerBase.TowerType.Crop)
        {
            playerInRange = true;
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<TowerBase>() && other.GetComponent<TowerBase>().typeTower == TowerBase.TowerType.Crop)
        {
            playerInRange = false;
            agent.stoppingDistance = 0;
        }
    }

    protected virtual void attack(GameObject target)
    {

        if (target == null) return;

        IDamage targetHealth = target.GetComponentInParent<IDamage>();
        if (targetHealth != null)
        {
            aud.PlayOneShot(audBites[Random.Range(0, audBites.Length)]);
            targetHealth.takeDamage(1);
            biteTimer = 0;
        }
    }

    protected IEnumerator flashRed()
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

    protected IEnumerator PlayStep()
    {
        isPlayingSteps = true;
        aud.PlayOneShot(audSteps[Random.Range(0, audSteps.Length)]);
        yield return new WaitForSeconds(.3f);
        isPlayingSteps = false;
    }
    void FindNearestCrop()
    {
        if (GameManager.instance == null || GameManager.instance.crops.Count == 0)
        {
            return;
        }

        float closestDist = Mathf.Infinity;
        GameObject nearestCrop = null;

        foreach (GameObject crop in GameManager.instance.crops)
        {
            if (crop == null) continue;

            float dist = Vector3.Distance(transform.position, crop.transform.position);
            if (dist < closestDist && dist <= cropDetectionRadius)
            {
                closestDist = dist;
                nearestCrop = crop;
            }
        }

        if (nearestCrop != null)
        {
            targetObj = nearestCrop;
            agent.SetDestination(targetObj.transform.position);
        }
    }

    protected virtual void HandleIdleSound()
    {
        idleSoundTimer += Time.deltaTime;

        if (idleSoundTimer >= nextIdleSoundTime)
        {
            aud.PlayOneShot(audIdles[0]);
            idleSoundTimer = 0f;
            nextIdleSoundTime = Random.Range(idleSoundMinDelay, idleSoundMaxDelay);
        }
    }
}
