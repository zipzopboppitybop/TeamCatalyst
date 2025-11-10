using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
    // need boolean to check if this attacks plants or not
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform headPos;
    [SerializeField] GameObject itemDrop;
    [SerializeField] GameObject targetObj;
    [SerializeField] float cropSearchInterval; 
    [SerializeField] float cropDetectionRadius;

    [SerializeField] int hp;
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

    float biteTimer;
    float roamTimer;
    float angleToTarget;
    float stoppingDistOrg;
    float cropSearchTimer;

    int roamTimeOrig;

    Vector3 targetDir;
    Vector3 startingPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrig = model.material.color;
        roamTimeOrig = roamPauseTime;
        stoppingDistOrg = agent.stoppingDistance;
        startingPos = transform.position;

        if (targetsPlayer)
        {
            targetObj = GameManager.instance.player;
        }
    }

    // Update is called once per frame
    void Update()
    {
        biteTimer += Time.deltaTime;
        cropSearchTimer += Time.deltaTime;

        if (agent == null || !agent.isOnNavMesh) return;

        if (agent.remainingDistance < 0.01f)
            roamTimer += Time.deltaTime;

        if (!targetsPlayer)
        {
            if (targetObj == null || !targetObj.activeInHierarchy)
            {
                if (cropSearchTimer >= cropSearchInterval)
                {
                    FindNearestCrop();
                    cropSearchTimer = 0f;
                }
            }

            if (targetObj == null)
            {
                CheckRoam();
                return;
            }

            CanSeeTarget();
        }
        else
        {
            if (playerInRange && !CanSeeTarget())
                CheckRoam();
            else if (!playerInRange)
                CheckRoam();
        }

        if (!targetsPlayer && targetObj == null && agent.remainingDistance < 0.5f)
            CheckRoam();
    }

    public void takeDamage(int amount)
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

    void CheckRoam()
    {
        if (roamTimer >= roamPauseTime && agent.remainingDistance < 0.01f)
        {
            Roam();
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

    bool CanSeeTarget()
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
                        attack(targetObj);
                        biteTimer = 0;
                    }

                    if (agent.remainingDistance <= stoppingDistOrg)
                        FaceTarget();

                    agent.stoppingDistance = stoppingDistOrg;
                    return true;
                }

            }
        }

        agent.stoppingDistance = 0;
        return false;

    }
    void FaceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(targetDir.x, 0, targetDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<TowerBase>() && other.GetComponent<TowerBase>().typeTower == TowerBase.TowerType.Crop)
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<TowerBase>() && other.GetComponent<TowerBase>().typeTower == TowerBase.TowerType.Crop)
        {
            playerInRange = false;
            agent.stoppingDistance = 0;
        }
    }

    void attack(GameObject target)
    {

        if (target == null) return;

        IDamage targetHealth = target.GetComponentInParent<IDamage>();
        if (targetHealth != null)
        {
            Debug.Log("Attack");
            targetHealth.takeDamage(1);
        }
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
}
