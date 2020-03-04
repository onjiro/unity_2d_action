using System.Collections;
using UnityEngine;

public class Enemy1Controller : MonoBehaviour
{
    public PatrolPath path;
    public float initialJumpSpeed;
    private KinematicStrategy kinematic;
    private Vector2 targetVelocity = Vector2.zero;

    // Start is called before the first frame update
    private void Awake()
    {
        this.kinematic = new KinematicStrategy(gameObject);

        if (this.path)
        {
            StartCoroutine(this.CoroutinePatrol());
        }
        else
        {
            StartCoroutine(this.CoroutineWaitAndJump());
        }
    }

    private IEnumerator CoroutinePatrol()
    {
        var rb2d = this.GetComponent<Rigidbody2D>();
        var spriteRenderer = this.GetComponent<SpriteRenderer>();

        // go left first

        while (true)
        {
            // accel left
            spriteRenderer.flipX = true;
            foreach (var acceleration in new[] { 0.5f, 0.5f, 0.5f, 0.5f })
            {
                this.targetVelocity.x -= acceleration;
                yield return null;
            }

            // patrol left
            while (rb2d.position.x > this.path.StartPositionInGlobal().x)
            {
                yield return null;
            }

            // wait
            this.targetVelocity.x = 0;
            yield return new WaitForSeconds(2f);

            // accel right
            spriteRenderer.flipX = false;
            foreach (var acceleration in new[] { 0.5f, 0.5f, 0.5f, 0.5f })
            {
                this.targetVelocity.x += acceleration;
                yield return null;
            }

            // patrol right
            while (rb2d.position.x <= this.path.EndPositionInGlobal().x)
            {
                yield return null;
            }

            // wait
            this.targetVelocity.x = 0;
            yield return new WaitForSeconds(2f);
        }
    }

    private IEnumerator CoroutineWaitAndJump()
    {
        while (true)
        {
            // wait
            for (int i = 0; i < 5; i++)
            {
                this.TurnToPlayerDirection();
                yield return new WaitForSeconds(1f);
            }

            // jump
            this.targetVelocity.y = this.initialJumpSpeed;
            yield return new WaitForSeconds(0.5f);
            this.TurnToPlayerDirection();
            this.targetVelocity.y = 0f;
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void TurnToPlayerDirection()
    {
        var player = PlayerController.Instance?.gameObject;
        if (!player) return;

        this.GetComponent<SpriteRenderer>().flipX = (this.transform.position.x > player.transform.position.x);
    }

    private void FixedUpdate()
    {
        var rb2d = this.GetComponent<Rigidbody2D>();
        var grounding = this.kinematic.IsGrounding();
        rb2d.velocity = this.kinematic.CalculateVelocity(rb2d.velocity, this.targetVelocity, grounding);
        rb2d.MovePosition(this.kinematic.CalculatePosition(rb2d.velocity));
    }
}
