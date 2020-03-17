using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(KinematicBody))]
public class Enemy1Controller : MonoBehaviour, EnemyControllerInterface
{
    public PatrolPath path;
    public float initialJumpSpeed;
    private KinematicBody kinematic;
    private List<ActionTrigger> actionTriggers = new List<ActionTrigger>();

    // Start is called before the first frame update
    private void Awake()
    {
        this.kinematic = GetComponent<KinematicBody>();

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
                this.kinematic.targetVelocity.x -= acceleration;
                yield return null;
            }

            // patrol left
            while (rb2d.position.x > this.path.StartPositionInGlobal().x)
            {
                yield return null;
            }

            // wait
            this.kinematic.targetVelocity.x = 0;
            yield return new WaitForSeconds(2f);

            // accel right
            spriteRenderer.flipX = false;
            foreach (var acceleration in new[] { 0.5f, 0.5f, 0.5f, 0.5f })
            {
                this.kinematic.targetVelocity.x += acceleration;
                yield return null;
            }

            // patrol right
            while (rb2d.position.x <= this.path.EndPositionInGlobal().x)
            {
                yield return null;
            }

            // wait
            this.kinematic.targetVelocity.x = 0;
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
            this.kinematic.targetVelocity.y = this.initialJumpSpeed;
            yield return new WaitForSeconds(0.5f);
            this.TurnToPlayerDirection();
            this.kinematic.targetVelocity.y = 0f;
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
    }

    public void AddActionTrigger(ActionTrigger trigger)
    {
        this.actionTriggers.Add(trigger);
    }
}
