using System.Collections;
using Unity.Transforms;
using UnityEngine;

public class GuardDogAI : AILogic
{
    bool targetInRange;
    [SerializeField] Transform homePosTransform;
    protected Vector3 homePos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        homePos = homePosTransform.position;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (targetInRange && (targetObj == null || !targetObj.activeInHierarchy ||
            Vector3.Distance(transform.position, targetObj.transform.position) > 20f))
        {
            targetInRange = false;
            targetObj = null;
            agent.stoppingDistance = 0;
        }

        if (!targetInRange)
        {
            CheckRoam();
        }
        else
        {
            CanSeeTarget();
        }
    }

    public override void takeDamage(int amount)
    {
        hp -= amount;

        agent.SetDestination(targetObj.transform.position);


        if (hp <= 0)
        {
            GoHome();
        }
        else
        {
            StartCoroutine(flashRed());
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            targetInRange = true;
            targetObj = other.gameObject;
        }

        if (other.CompareTag("HomePos"))
        {
            StartCoroutine(Heal());
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            targetInRange = false;
            agent.stoppingDistance = 0;
        }
    }

    protected override bool CanSeeTarget()
    {
        if (targetObj == null) return false;

        agent.stoppingDistance = stoppingDistOrg;

        agent.SetDestination(targetObj.transform.position);

        if (biteTimer > biteRate && agent.remainingDistance <= stoppingDistOrg)
        {
            attack(targetObj);
            biteTimer = 0;
        }

        Vector3 dir = targetObj.transform.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
        }

        return true;
    }

    private void GoHome()
    {
        agent.SetDestination(homePos);
    }

    IEnumerator Heal()
    {
        while (hp < hpOrig)
        {
            hp += healRate;
            yield return null;
        }

        CheckRoam();
    }
}
