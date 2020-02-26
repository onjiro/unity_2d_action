using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float speed;
    // 着弾エフェクト
    public GameObject landingEffect;
    public float lifeTimeSeconds;
    private Rigidbody2D rb2d;
    void Start()
    {
        this.rb2d = this.GetComponent<Rigidbody2D>();
        this.GetComponent<SpriteRenderer>().flipX = (this.speed < 0);
        Destroy(this.gameObject, this.lifeTimeSeconds);
    }

    void FixedUpdate()
    {
        var velocity = new Vector2(this.speed, 0);
        var deltaPosition = velocity * Time.deltaTime;

        this.rb2d.MovePosition(this.rb2d.position + deltaPosition);
    }

    /// <summary>
    /// Sent when another object enters a trigger collider attached to this
    /// object (2D physics only).
    /// </summary>
    /// <param name="other">The other Collider2D involved in this collision.</param>
    void OnTriggerEnter2D(Collider2D otherCollider)
    {
        var other = otherCollider.gameObject;
        switch (other.tag)
        {
            case "Enemy":
                Instantiate(this.landingEffect, this.rb2d.position + new Vector2(this.speed, 0) * Time.deltaTime, this.transform.rotation);
                Destroy(gameObject);
                other.GetComponent<HittableEnemy>()?.Hit(this.GetComponent<PlayerAttack>());
                break;
            case "Land":
                Instantiate(this.landingEffect, this.rb2d.position + new Vector2(this.speed, 0) * Time.deltaTime, this.transform.rotation);
                Destroy(gameObject);
                break;
        }
    }
}
