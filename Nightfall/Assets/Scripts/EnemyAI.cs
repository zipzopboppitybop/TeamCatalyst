using UnityEngine;

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
