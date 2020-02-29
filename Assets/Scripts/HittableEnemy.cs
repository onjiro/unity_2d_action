using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HittableEnemy : MonoBehaviour
{
    public int hp = 1;
    public GameObject destroyEffect;

    public void Hit(PlayerAttack attack)
    {
        this.hp -= attack.damage;
        if (this.hp <= 0)
        {
            Instantiate(this.destroyEffect, this.gameObject.transform.position, Quaternion.identity);
            Destroy(this.gameObject);
        }
    }
}
