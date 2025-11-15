using UnityEngine;

public class EnemyAI : AILogic
{
    protected override void Start()
    {
        base.Start();

        targetsPlayer = false;
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
    }
}
