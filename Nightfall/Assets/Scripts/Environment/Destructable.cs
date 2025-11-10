
using UnityEngine;




namespace Catalyst.Environment
{
    public class Destructable : MonoBehaviour, IDamage
    {
        [SerializeField] private int hp = 100;

        public void takeDamage(int amount)
        {
            hp -= amount;

            if (hp <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

}
