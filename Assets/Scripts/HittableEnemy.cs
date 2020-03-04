using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HittableEnemy : MonoBehaviour
{
    public int hp = 1;
    public GameObject destroyEffect;
    public Vector2 destroyEffectOffset = Vector2.zero;

    public void Hit(PlayerAttack attack)
    {
        this.hp -= attack.damage;
        if (this.hp <= 0)
        {
            Instantiate(this.destroyEffect, this.gameObject.transform.TransformPoint(destroyEffectOffset), Quaternion.identity);
            Destroy(this.gameObject);
        }
    }
}
