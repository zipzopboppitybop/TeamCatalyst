using UnityEngine;

public class Sprinkler : Damage
{

    [SerializeField] private int damageAmount;


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

    }
}
