using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HittableEnemy : MonoBehaviour
{
    public int maxHp = 1;
    public int currentHp;
    public int rectionByDamageCount = 1;
    private int damageCount;
    public GameObject destroyEffect;
    public Vector2 destroyEffectOffset = Vector2.zero;

    private void Awake()
    {
        this.currentHp = this.maxHp;
    }

    public void Hit(PlayerAttack attack)
    {
        this.currentHp -= attack.damage;
        if (this.currentHp <= 0)
        {
            Instantiate(this.destroyEffect, this.gameObject.transform.TransformPoint(destroyEffectOffset), Quaternion.identity);
            Destroy(this.gameObject);
        }

        this.damageCount += attack.damage;
        if (this.damageCount > this.rectionByDamageCount)
        {
            this.damageCount = 0;
            this.gameObject.GetComponent<EnemyControllerInterface>().AddActionTrigger(ActionTrigger.Damaged);
        }
    }
}
