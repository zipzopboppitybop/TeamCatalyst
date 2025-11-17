using UnityEngine;
using System.Collections;

public class RabbitAI : EnemyAI
{
    private int lastAnimIndex = -1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();

        targetsPlayer = false;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        HandleAnimation();
    }

    protected override void HandleAnimation()
    {
        if (animator == null) return;

        int newIndex;

        if (hp <= 0)
        {
            newIndex = 2;
        }
        else if (agent.velocity.sqrMagnitude > 0.01f)
        {
            newIndex = 1;
        }
        else
        {
            newIndex = 0; 
        }

        if (animator)
        {
            if (newIndex != lastAnimIndex)
            {
                lastAnimIndex = newIndex;
                animator.SetInteger("AnimIndex", newIndex);
                animator.SetTrigger("Next");
            }
        }
    }

    public override void takeDamage(int amount)
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

            StartCoroutine(Die());
        }
        else
        {
            StartCoroutine(flashRed());
        }
    }

    IEnumerator Die()
    {
        animator.SetInteger("AnimIndex", 2);
        animator.SetTrigger("Next");

        agent.enabled = false;

        yield return new WaitForSeconds(2f);  

        Destroy(gameObject);
    }

    protected override void LookForTarget()
    {
        if (cropSearchTimer >= cropSearchInterval)
        {
            cropSearchTimer = 0f;
            FindNearestCrop();
        }
        if (targetObj == null)
        {
            CheckRoam();
            return;
        }

        CanSeeTarget();
    }
}
