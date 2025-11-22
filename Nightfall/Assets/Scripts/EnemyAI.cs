using UnityEngine;
using System.Collections;

public class EnemyAI : AILogic
{
    protected override void Start()
    {
        base.Start();

        FindNearestCrop();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        LookForTarget();
        if (targetObj == null)
        {
            FindNearestCrop();
        }

        if (targetsPlayer)
        {
            HandleAnimation();
        }

    }

    protected override IEnumerator flashRed()
    {
        if (targetsPlayer)
        {
            foreach (Material mat in model.materials)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.red * 5f);
            }

            yield return new WaitForSeconds(0.1f);

            foreach (Material mat in model.materials)
            {
                mat.SetColor("_EmissionColor", Color.black);
            }
        }
        else
        {
            StartCoroutine(base.flashRed());
        }
    }


    protected virtual void HandleAnimation()
    {
        if (animator == null) return;

        if (hp <= 0)
        {
            animator.Play("Death");
            return;
        }

        if (targetObj != null && agent.remainingDistance <= stoppingDistOrg && biteTimer < 0.1f)
        {
            animator.Play("Attack");
            return;
        }

        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            animator.Play("Run");
            return;
        }

        animator.Play("Idle");
    }
}
