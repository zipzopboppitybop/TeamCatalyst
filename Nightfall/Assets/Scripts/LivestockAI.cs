using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Livestock : MonoBehaviour, IDamage
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
    }
    void Update()
    {
        if (agent == null || !agent.isOnNavMesh) return;

        if (agent.remainingDistance < 0.01f)
            roamTimer += Time.deltaTime;

        if (!GameManager.instance.IsNight)
        {
            CheckRoam();
        }
        else
        {
            HeadHome();
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
