using System.Collections;
using System.Collections.Generic;
using Unity.Transforms;
using UnityEngine;

public class GuardDogAI : AILogic
{
    [SerializeField] Transform homePosTransform;
    protected Vector3 homePos;
    bool targetInRange;

    private List<GameObject> enemiesInRange = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        homePos = homePosTransform.position;
        targetObj = null;
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            takeDamage(1);
        }
        base.Update();

        if (isHealing)
        {
            return;
        }

        enemiesInRange.RemoveAll(e => e == null);

        UpdateTarget();

        if (targetObj != null)
        {
            CanSeeTarget();
        }
        else
        {
            CheckRoam();
        }
    }


    private void UpdateTarget()
    {
        if (enemiesInRange.Count == 0)
        {
            targetObj = null;
            return;
        }

        GameObject closest = null;
        float minDist = Mathf.Infinity;
        foreach (GameObject enemy in enemiesInRange)
        {
            if (enemy == null) continue;
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }

        targetObj = closest;
        targetInRange = targetObj != null;
    }

    public override void takeDamage(int amount)
    {
        if (hp <= 0)
        {
            return;
        }

        hp -= amount;

        if (hp > 0)
        {
            StartCoroutine(flashRed());
            return;
        }

        hp = 0;

        targetObj = null;
        targetInRange = false;

        gameObject.tag = "Untagged";

        GoHome();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && !enemiesInRange.Contains(other.gameObject))
        {
            enemiesInRange.Add(other.gameObject);
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
            enemiesInRange.Remove(other.gameObject);
        }
    }

    protected override bool CanSeeTarget()
    {
        if (targetObj == null || agent == null || !agent.isOnNavMesh || hp <= 0)
        {
            return false;
        }

        agent.SetDestination(targetObj.transform.position);

        if (Vector3.Distance(transform.position, targetObj.transform.position) <= stoppingDistOrg) 
        {
            FaceTarget();
            if (biteTimer > biteRate)
            {
                attack(targetObj);
                biteTimer = 0;
            }
        }
        return true;
    }

    private void GoHome()
    {
        agent.SetDestination(homePos);
        agent.stoppingDistance = 0;
    }

    IEnumerator Heal()
    {
        while (Vector3.Distance(transform.position, homePos) > 0.5f) 
        {
            agent.SetDestination(homePos);
            yield return null;
        }

        isHealing = true; 
        agent.isStopped = true;

        while (hp < hpOrig)
        {
            hp += healRate * Time.deltaTime;

            if (hp >= hpOrig)
            {
                hp = hpOrig;
                break;
            }

            yield return null;
        }

        targetObj = null;
        targetInRange = false;

        gameObject.tag = "LiveStock";

        isHealing = false;
        agent.isStopped = false;

        CheckRoam();
    }
}
