using System.Collections;
using UnityEngine;

public class Boss1Controller : MonoBehaviour
{
    public float moveSpeed = 1.2f;
    public float initialJumpSpeed = 8f;
    private KinematicStrategy kinematic;
    private Vector2 targetVelocity = Vector2.zero;
    private bool grounding = true;

    // Start is called before the first frame update
    private void Awake()
    {
        this.kinematic = new KinematicStrategy(gameObject);

        StartCoroutine(this.CoroutineWaitAndJump());
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
            this.targetVelocity.x = this.GetComponent<SpriteRenderer>().flipX ? -this.moveSpeed : this.moveSpeed;
            this.targetVelocity.y = this.initialJumpSpeed;
            yield return new WaitForSeconds(0.1f);
            this.targetVelocity.y = 0f;

            // wait for grounding
            while (!this.grounding)
            {
                yield return null;
            }
            this.TurnToPlayerDirection();
            this.targetVelocity.x = 0f;
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
        this.grounding = this.kinematic.IsGrounding();
        rb2d.velocity = this.kinematic.CalculateVelocity(rb2d.velocity, this.targetVelocity, grounding);
        rb2d.MovePosition(this.kinematic.CalculatePosition(rb2d.velocity));
    }
}
