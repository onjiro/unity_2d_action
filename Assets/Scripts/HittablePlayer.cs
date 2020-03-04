using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D), typeof(PlayerController))]
public class HittablePlayer : MonoBehaviour
{
    public int hp = 5;
    public bool enable = true;
    public GameObject destroyEffect;

    public void OnCollisionEnter2D(Collision2D otherCollision)
    {
        if (!enable) return;

        var other = otherCollision.gameObject;
        var attack = other.GetComponent<EnemyAttack>();
        if (attack)
        {
            this.hp -= attack.damage;
            if (hp > 0)
            {
                this.GetComponent<PlayerController>().AddActionTrigger(PlayerController.ActionTrigger.Damaged);
                StartCoroutine(CoroutineBlink());
            }
            else
            {
                this.GetComponent<PlayerController>().AddActionTrigger(PlayerController.ActionTrigger.Dead);
            }
        }
    }

    private IEnumerator CoroutineBlink()
    {
        this.enable = false;
        this.gameObject.AddComponent<BlinkSprite>();
        yield return new WaitForSeconds(1f);
        Destroy(this.gameObject.GetComponent<BlinkSprite>());
        this.enable = true;
    }
}
