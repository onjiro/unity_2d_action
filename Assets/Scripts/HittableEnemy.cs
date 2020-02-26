using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HittableEnemy : MonoBehaviour
{
    public int hp = 1;

    public void Hit(PlayerAttack attack)
    {
        this.hp -= attack.damage;
        if (this.hp <= 0)
        {
            Destroy(this.gameObject);
        }
    }
}
