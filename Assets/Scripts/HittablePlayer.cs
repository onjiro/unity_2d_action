using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(PlayerController))]
public class HittablePlayer : MonoBehaviour
{
    public int hp = 5;
    public GameObject destroyEffect;

    public void OnCollisionEnter2D(Collision2D otherCollision)
    {
        var other = otherCollision.gameObject;
        var attack = other.GetComponent<EnemyAttack>();
        if (attack)
        {
            this.hp -= attack.damage;
            if (hp > 0)
            {
                this.GetComponent<PlayerController>().AddActionTrigger(PlayerController.ActionTrigger.Damaged);
            }
            else
            {
                this.GetComponent<PlayerController>().AddActionTrigger(PlayerController.ActionTrigger.Dead);
            }
        }
    }
}
