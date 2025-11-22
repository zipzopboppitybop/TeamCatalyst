using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

public class AILogic : MonoBehaviour, IDamage
{
    // need boolean to check if this attacks plants or not
    [SerializeField] protected LayerMask ignoreLayer;

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
    [Range(0, 100)][SerializeField] protected int dropChance;
    [SerializeField] protected float biteRate;

    Color colorOrig;

    protected bool playerInRange;
    [SerializeField] protected bool targetsPlayer;
    protected bool isScared = false;
    protected bool isHealing;
    protected bool isPlayingSteps;
    protected bool attackingWall;
    protected bool attackMode;

    protected float biteTimer = 0f;
    protected float roamTimer;
    protected float angleToTarget;
    protected float stoppingDistOrg;
    protected float cropSearchTimer;
    protected float hpOrig;
    protected float healRate;

    protected int roamTimeOrig;

    protected Vector3 targetDir;
    protected Vector3 startingPos;

    protected Vector3Int currentCell;

    protected Tilemap map;

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
            targetObj = null; 
        }

        nextIdleSoundTime = Random.Range(idleSoundMinDelay, idleSoundMaxDelay);
    }

    // Update is called once per frame
    protected virtual void Update()
    {

        if (attackMode)
        {
            biteTimer += Time.deltaTime;
        }

        cropSearchTimer += Time.deltaTime;

        if (agent == null || !agent.isOnNavMesh) return;

        if (agent.remainingDistance <= 0.01f)
        {
            roamTimer += Time.deltaTime;
        }

        HandleIdleSound();
        //LookForTarget();

    }

    protected virtual void LookForTarget()
    {

        if (!targetsPlayer)
        {
            FindNearestCrop();
            return;
        }

        if (playerInRange && GameManager.instance != null)
        {
            targetObj = GameManager.instance.player;
            CanSeeTarget();
            return;
        }

        GameObject nearestLivestock = FindNearestLivestock();
        if (nearestLivestock != null)
        {
            targetObj = nearestLivestock;
            CanSeeTarget();
            return;
        }

        if (targetObj == null || !targetObj.activeInHierarchy)
        {
            if (cropSearchTimer >= cropSearchInterval)
            {
                FindNearestCrop();
                cropSearchTimer = 0f;
            }
        }

        if (targetObj == null && GameManager.instance != null)
        {
            targetObj = GameManager.instance.player;
        }

        if (targetObj != null)
            CanSeeTarget();
        else
            CheckRoam();
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
            if (!attackingWall)
            {


                RaycastHit fenceCheck;

                if (Physics.Raycast(headPos.position, targetObj.transform.position, out fenceCheck))
                {



                    map = FindFirstObjectByType<Tilemap>();
                    currentCell = map.WorldToCell(fenceCheck.point);

                    GameObject existing = map.GetInstantiatedObject(currentCell);
                    TowerBase existingTower = existing ? existing.GetComponent<TowerBase>() : null;

                    if (existingTower != null && existingTower.typeTower == TowerBase.TowerType.Defensive)
                    {

                        attackingWall = true;
                        targetObj = existing;
                        agent.SetDestination(targetObj.transform.position);

                    }
                }
            }

                targetDir = targetObj.transform.position - headPos.position;
            angleToTarget = Vector3.Angle(targetDir, transform.forward);
            Debug.DrawRay(headPos.position, targetDir, Color.red);

            RaycastHit hit;
            //Debug.Log("You made it this far");
            if (Physics.Raycast(headPos.position, targetDir, out hit))
            {
                //Debug.Log(hit.collider.name);
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
                    attackMode = true;
                    return true;
                }

            }
        }

        agent.stoppingDistance = 0;
        attackMode = false;
        return false;
    }

    protected void FaceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(targetDir.x, 0, targetDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            return;
        }

        if (other.CompareTag("LiveStock"))
        {
            targetObj = other.gameObject;
            return;
        }

        TowerBase tower = other.GetComponent<TowerBase>();
        if (tower != null && tower.typeTower == TowerBase.TowerType.Crop)
        {
            targetObj = other.gameObject;
            return;
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            agent.stoppingDistance = 0;
            return;
        }
    }

    protected virtual void attack(GameObject target)
    {

        if (target == null) return;

        IDamage targetHealth = target.GetComponentInParent<IDamage>();
        if (targetHealth != null)
        {
            if (audBites.Length > 0)
            {
                aud.PlayOneShot(audBites[Random.Range(0, audBites.Length)]);
            }
            targetHealth.takeDamage(1);
            biteTimer = 0;
        }
    }

    protected IEnumerator flashRed()
    {
        model.material.color = Color.red;
        Debug.Log("Wolf Color: " + model.material.color);
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }

    protected IEnumerator getScared()
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
        if (audSteps.Length > 0)
        {
            aud.PlayOneShot(audSteps[Random.Range(0, audSteps.Length)]);
        }

        yield return new WaitForSeconds(.3f);
        isPlayingSteps = false;
    }
    protected void FindNearestCrop()
    {
        if (GameManager.instance == null || GameManager.instance.crops.Count == 0)
        {
            targetObj = null;   
            attackingWall = false;
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
            attackingWall = false;
        }
        else
        {
            targetObj = null;
        }
    }

    protected GameObject FindNearestLivestock()
    {
        GameObject[] livestock = GameObject.FindGameObjectsWithTag("LiveStock");
        float closestDist = Mathf.Infinity;
        GameObject nearest = null;

        foreach (GameObject animal in livestock)
        {
            if (animal == null || !animal.activeInHierarchy) continue;

            float dist = Vector3.Distance(transform.position, animal.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                nearest = animal;
            }
        }

        return nearest;
    }

    protected virtual void HandleIdleSound()
    {
        idleSoundTimer += Time.deltaTime;

        if (idleSoundTimer >= nextIdleSoundTime)
        {
            if (audIdles.Length > 0)
            {
                aud.PlayOneShot(audIdles[Random.Range(0, audIdles.Length)]);
            }
            idleSoundTimer = 0f;
            nextIdleSoundTime = Random.Range(idleSoundMinDelay, idleSoundMaxDelay);
        }
    }
}
