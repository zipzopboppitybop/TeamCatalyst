using UnityEngine;
using System.Collections;

public class Damage : MonoBehaviour
{
    enum DamageType { Stationary, Moving, Homing, DOT, Sprinkler }
    [SerializeField] DamageType type;
    [SerializeField] Rigidbody rb;

    [SerializeField] int dmgAmt;
    [SerializeField] int speed;
    [SerializeField] int destroyTime;

    [SerializeField] float dmgRate;

    bool isDamaging;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if (type == DamageType.Moving || type == DamageType.Homing)
        {

            Destroy(gameObject, destroyTime);

            if (type == DamageType.Moving)
            {

                rb.linearVelocity = transform.forward * speed;

            }

        }


    }

    // Update is called once per frame
    void Update()
    {

        // TODO: Make Homing projectiles follow a target position instead of just the player.

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
        {

            return;

        }

        IDamage dmg = other.GetComponent<IDamage>();
        if (dmg != null && (type == DamageType.Moving || type == DamageType.Stationary || type == DamageType.Homing))
        {

            dmg.takeDamage(dmgAmt);

        }

        if (type == DamageType.Moving || type == DamageType.Homing)
        {

            Destroy(gameObject, destroyTime);
        }

        if(type == DamageType.Sprinkler && !other.CompareTag("Player"))
        {
            dmg.takeDamage(dmgAmt);
        }

    }

    private void OnTriggerStay(Collider other)
    {

        if (other.isTrigger)
        {

            return;

        }

        IDamage dmg = other.GetComponent<IDamage>();
        if (dmg != null && type == DamageType.DOT && !isDamaging)
        {

            StartCoroutine(damageOther(dmg));

        }

    }

    IEnumerator damageOther(IDamage d)
    {

        isDamaging = true;
        d.takeDamage(dmgAmt);
        yield return new WaitForSeconds(dmgRate);
        isDamaging = false;

    }

}
