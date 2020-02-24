using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy1Controller : MonoBehaviour
{
    public PatrolPath path;
    private KinematicStrategy kinematic;
    private Vector2 targetVelocity = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        this.kinematic = new KinematicStrategy(gameObject);

        this.targetVelocity = Vector2.right * 2;
        GetComponent<SpriteRenderer>().flipX = false;
    }

    // Update is called once per frame
    void Update()
    {
        var rb2d = this.GetComponent<Rigidbody2D>();

        if ((this.targetVelocity.x > 0 && rb2d.position.x >= this.path.EndPositionInGlobal().x)
            || (this.targetVelocity.x < 0 && rb2d.position.x < this.path.StartPositionInGlobal().x))
        {
            this.targetVelocity = Vector2.zero;
            StartCoroutine(flipAndGoAfter(1f, rb2d));
        }
    }

    private IEnumerator flipAndGoAfter(float seconds, Rigidbody2D rb2d)
    {
        yield return new WaitForSeconds(seconds);
        var spriteRenderer = this.GetComponent<SpriteRenderer>();
        this.targetVelocity = spriteRenderer.flipX ? Vector2.right * 2 : Vector2.left * 2;
        spriteRenderer.flipX = !spriteRenderer.flipX;
    }

    private void FixedUpdate()
    {
        var rb2d = this.GetComponent<Rigidbody2D>();
        var grounding = this.kinematic.IsGrounding();
        rb2d.velocity = this.kinematic.CalculateVelocity(rb2d.velocity, this.targetVelocity, grounding);
        rb2d.MovePosition(this.kinematic.CalculatePosition(rb2d.velocity));
    }
}
