using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossUpperBulletController : MonoBehaviour
{
    public Vector2 velocity;
    private void Awake()
    {
        Destroy(gameObject, 2f);
    }

    private void FixedUpdate()
    {
        var rb2d = GetComponent<Rigidbody2D>();
        rb2d.MovePosition(rb2d.position + this.velocity);
    }
}
