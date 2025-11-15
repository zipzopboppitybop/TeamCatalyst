using Catalyst.GamePlay;
using System.Collections;
using System.Collections.Generic;
using Unity.Transforms;
using UnityEngine;
using static UnityEngine.GridBrushBase;

public class GuardDogAI : AILogic, IInteractable
{
    [SerializeField] Transform homePosTransform;
    [SerializeField] private AudioClip[] audHappy;
    private Transform playerPos;
    public Vector3 homePos;
    public Chest FeedingTrough;
    private bool targetInRange;

    private List<GameObject> enemiesInRange = new List<GameObject>();

    [SerializeField] private string verticalParam = "Vert";
    [SerializeField] private string stateParam = "State";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        homePos = homePosTransform.position;
        targetObj = null;
        agent.updateRotation = false;
        playerPos = GameManager.instance.player.transform;
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            takeDamage(1);
        }
        base.Update();

        if (isHealing || hp <= 0)
        {
            return;
        }

        enemiesInRange.RemoveAll(e => e == null);
        UpdateTarget();

        if (enemiesInRange.Count == 0)
        {
            roamTimer = roamPauseTime;
        }


        if (targetObj != null)
        {
            ChaseTarget();
        }
        else if (enemiesInRange.Count == 0)
        {
            if (playerInRange)
            {
                FollowPlayer();
                RotateWhileMoving();
            }
            else
            {
                RotateWhileMoving();
                CheckRoam();
            }
        }

        AnimateMovement();
    }

    private void RotateWhileMoving()
    {
        Vector3 velocity = agent.velocity;
        velocity.y = 0f; 
        if (velocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(velocity);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * faceTargetSpeed);
        }
    }

    private void ChaseTarget()
    {
        if (targetObj == null)
        {
            return; 
        }

        targetDir = targetObj.transform.position - transform.position;
        agent.SetDestination(targetObj.transform.position);
        FaceTarget();
        float distance = targetDir.magnitude;
        if (distance <= stoppingDistOrg && biteTimer > biteRate)
        {
            attack(targetObj);
            biteTimer = 0;
        }
    }

    private void UpdateTarget()
    {
        if (enemiesInRange.Count == 0)
        {
            targetObj = null;
            targetInRange = false;
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

        aud.Stop();
        aud.clip = audHurt[Random.Range(0, audHurt.Length)];
        aud.Play();
        hp -= amount;

        if (hp > 0)
        {
            StartCoroutine(flashRed());
            return;
        }

        hp = 0;

        targetObj = null;
        targetInRange = false;

        isHealing = false;

        if (agent != null)
        {
            agent.isStopped = false;
            agent.updateRotation = false;
            agent.stoppingDistance = 0;
        }

        gameObject.tag = "Untagged";

        GoHome();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && !enemiesInRange.Contains(other.gameObject))
        {
            aud.PlayOneShot(audNotice[Random.Range(0, audNotice.Length)]);
            enemiesInRange.Add(other.gameObject);
        }

        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }

        if (other.CompareTag("HomePos"))
        {
            if (!isHealing && hp < hpOrig)
            {
                StartCoroutine(Heal());
            }
        }
    }
    protected override void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemiesInRange.Remove(other.gameObject);
        }

        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private void GoHome()
    {
        if (agent == null)
        {
            return;
        }

        agent.isStopped = false;
        agent.updateRotation = false;
        agent.stoppingDistance = 0;
        agent.SetDestination(homePos);

        StartCoroutine(FaceHomeWhileReturning());
    }

    private IEnumerator FaceHomeWhileReturning()
    {
        while (hp <= 0 && Vector3.Distance(transform.position, homePos) > 0.5f)
        {
            Vector3 toHome = agent.steeringTarget - transform.position;
            toHome.y = 0f;

            if (toHome.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(toHome);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * faceTargetSpeed);
            }

            AnimateMovement();
            yield return null;
        }

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        AnimateMovement(); 
    }

    IEnumerator Heal()
    {
        while (!agent.isOnNavMesh || agent.pathPending)
            yield return null;

        while (Vector3.Distance(transform.position, homePos) > 0.5f)
            yield return null;

        isHealing = true; 
        agent.isStopped = true;
        agent.velocity = Vector3.zero; 
        AnimateMovement();

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
        AnimateMovement();
        CheckRoam();
    }
    private void AnimateMovement()
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

    protected override void HandleIdleSound()
    {
        if (hp <= 0 || isHealing || targetObj != null)
        {
            return;
        }

        base.HandleIdleSound();
    }

    public void Interact()
    {
        aud.PlayOneShot(audHappy[Random.Range(0, audHappy.Length)]);
        StartCoroutine(DoBackflip());
    }
    private IEnumerator DoBackflip()
    {
        if (agent != null)
        {
            agent.enabled = false;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        float duration = 0.5f; 
        float elapsed = 0f;
        float jumpHeight = 1.0f;
        Quaternion startRot = transform.rotation;
        Quaternion endRot = transform.rotation * Quaternion.Euler(360f, 0f, 0f);
        Vector3 startPos = transform.position;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float heightOffset = Mathf.Sin(Mathf.PI * t) * jumpHeight;
            transform.position = startPos + Vector3.up * heightOffset;
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = startPos;
        transform.rotation = startRot;

        agent.enabled = true;
    }

    private void FollowPlayer()
    {
        if (playerPos == null || agent == null) return;

        float distance = Vector3.Distance(transform.position, playerPos.position);

        if (distance > 2f)
        {
            if (agent.enabled)
            {
                agent.SetDestination(playerPos.position);
            }
        }
        else
        {
            if (agent.enabled)
            {
                agent.ResetPath();
            }
        }
    }
}
