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

    [SerializeField] int hp;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int FOV;
    [SerializeField] int roamDist;
    [SerializeField] int roamPauseTime;
    [Range(0, 100)][SerializeField] int dropChance;
    [SerializeField] float biteRate;

    Color colorOrig;

    bool playerInRange;
    bool targetsPlayer = true;
    bool isScared = false;

    float biteTimer;
    float roamTimer;
    float angleToTarget;
    float stoppingDistOrg;

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
    }

    // Update is called once per frame
    void Update()
    {
        biteTimer += Time.deltaTime;

        //animator.SetFloat("Speed", agent.velocity.normalized.magnitude);

        if (agent.remainingDistance < 0.01f)
        {
            roamTimer += Time.deltaTime;
        }
        if (targetsPlayer)
        {        
            if (playerInRange && !CanSeeTarget())
            {
                CheckRoam();
            }
            else if (!playerInRange)
            {
                CheckRoam();
            }
        }
    }
    public void takeDamage(int amount)
    {
        hp -= amount;
        if (GameManager.instance != null && targetsPlayer)
        {
            targetObj = GameManager.instance.player;
            agent.SetDestination(targetObj.transform.position);
        }
            

        else if (!isScared)
        {
            StartCoroutine(getScared());
            agent.stoppingDistance = 0;
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
        if (Physics.Raycast(headPos.position, targetDir, out hit))
        {
            Debug.Log(hit.collider.name);

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
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            agent.stoppingDistance = 0;
        }
    }

    void attack(GameObject target)
    {

        if (target == null) return;

        IDamage targetHealth = target.GetComponent<IDamage>();
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
        yield return new WaitForSeconds(5);
        roamPauseTime = roamTimeOrig;
        isScared = false;

    }
}
