using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float speed;
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
}
