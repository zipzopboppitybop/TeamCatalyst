using UnityEngine;

public class ChickenAI : Livestock
{
    [SerializeField] private string verticalParam = "Vert";
    [SerializeField] private string stateParam = "State";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected  override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        HandleAnimation();
    }

    private void HandleAnimation()
    {
        if (agent == null || animator == null)
        {
            return;
        }

        Vector3 localVelocity = transform.InverseTransformDirection(agent.velocity);
        Vector2 axis = new Vector2(localVelocity.x, localVelocity.z);

        float speed = axis.magnitude;
        animator.SetFloat(verticalParam, speed);

        float state = 0f;
        if (speed > 0.01f && speed <= 0.5f)
        {
            state = 0.5f;
        }
        else if (speed > 0.5f)
        {
            state = 1f;
        }
        animator.SetFloat(stateParam, state);
    }
}
