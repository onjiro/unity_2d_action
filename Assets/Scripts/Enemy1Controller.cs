using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy1Controller : MonoBehaviour
{
    private KinematicStrategy kinematic;
    private Vector2 targetVelocity = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        this.kinematic = new KinematicStrategy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        this.targetVelocity = Vector2.right * 2;
        GetComponent<SpriteRenderer>().flipX = false;
    }

    private void FixedUpdate()
    {
        var rb2d = this.GetComponent<Rigidbody2D>();
        var grounding = this.kinematic.IsGrounding();
        rb2d.velocity = this.kinematic.CalculateVelocity(rb2d.velocity, this.targetVelocity, grounding);
        rb2d.MovePosition(this.kinematic.CalculatePosition(rb2d.velocity));
    }
}
