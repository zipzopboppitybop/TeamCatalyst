using UnityEngine;

public class MoleAI : AILogic
{

    //bool isUnderground = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        
        base.Start();

        targetsPlayer = false;
        //isUnderground = true;
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
