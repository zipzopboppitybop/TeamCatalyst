using System.Collections;
using Unity.Transforms;
using UnityEngine;

public class GuardDogAI : AILogic
{
    bool targetInRange;

    Vector3 homePos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (targetObj == null)
        {
            CheckRoam();
            return;
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
        }

        if (other.CompareTag("HomePosition"))
        {
            Heal();
        }
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
    }
}
