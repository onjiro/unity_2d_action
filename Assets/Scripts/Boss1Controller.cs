using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Boss1Controller : MonoBehaviour, EnemyControllerInterface
{
    public float moveSpeed = 1.2f;
    public float initialJumpSpeed = 8f;
    private KinematicStrategy kinematic;
    private Vector2 targetVelocity = Vector2.zero;
    private bool grounding = true;
    private List<ActionTrigger> actionTriggers = new List<ActionTrigger>();
    private IEnumerator interruptPattern = new ArrayList().GetEnumerator();

    // Start is called before the first frame update
    private void Awake()
    {
        this.kinematic = new KinematicStrategy(gameObject);

        StartCoroutine(this.Routine());
    }

    private IEnumerator Routine()
    {
        var hittableEnemy = this.GetComponent<HittableEnemy>();
        var pattern1 = Pattern1();
        var pattern2 = Pattern2();

        while (true)
        {
            // 割り込みがあればそれが完了するまで処理する
            while (interruptPattern.MoveNext())
            {
                yield return interruptPattern.Current;
            }

            if (hittableEnemy.currentHp >= hittableEnemy.maxHp / 2)
            {
                pattern1.MoveNext();
                yield return pattern1.Current;
            }
            else
            {
                pattern2.MoveNext();
                yield return pattern2.Current;
            }
        }
    }


    // HPが多いときの行動パターン
    //
    // 以下を繰り返す
    // ・待機
    // ・にじり寄り
    // ・にじり寄り
    // ・ジャンプ
    private IEnumerator Pattern1()
    {
        var spriteRenderer = this.GetComponent<SpriteRenderer>();
        while (true)
        {
            // wait
            for (int i = 0; i < 3; i++)
            {
                this.TurnToPlayerDirection();
                yield return new WaitForSeconds(0.5f);
            }

            // move x 2
            for (int j = 0; j < 2; j++)
            {
                this.TurnToPlayerDirection();
                this.targetVelocity.x = spriteRenderer.flipX ? -this.moveSpeed * 0.5f : this.moveSpeed * 0.5f;
                for (int i = 0; i < 120; i++)
                {
                    yield return null;
                }
                for (int i = 0; i < 60; i++)
                {
                    this.targetVelocity.x = this.targetVelocity.x * 0.7f;
                    yield return null;
                }
                this.targetVelocity.x = 0f;
                yield return new WaitForSeconds(1f);
            }

            // jump
            this.TurnToPlayerDirection();
            this.targetVelocity.x = spriteRenderer.flipX ? -this.moveSpeed : this.moveSpeed;
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
            yield return new WaitForSeconds(0.2f);
        }
    }

    // HPが少なくなったときの行動パターン
    //
    // 以下を繰り返す
    // ・ジャンプ
    private IEnumerator Pattern2()
    {
        while (true)
        {
            // jump
            this.TurnToPlayerDirection();
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
            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator DamagedPattern()
    {
        this.GetComponent<Animator>().SetTrigger("damaged");

        var spriteRenderer = this.GetComponent<SpriteRenderer>();
        this.targetVelocity.x = spriteRenderer.flipX ? this.moveSpeed * 0.2f : -this.moveSpeed * 0.2f;
        for (int i = 0; i < 120; i++)
        {
            yield return null;
        }
        this.targetVelocity.x = 0f;
        yield return new WaitForSeconds(0.5f);
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

        this.actionTriggers.Distinct().ToList().ForEach(HandleActionTrigger);
        this.actionTriggers.Clear();

        rb2d.velocity = this.kinematic.CalculateVelocity(rb2d.velocity, this.targetVelocity, grounding);
        rb2d.MovePosition(this.kinematic.CalculatePosition(rb2d.velocity));

        // Animation
        var animator = this.GetComponent<Animator>();
        animator.SetBool("grounding", this.grounding);
    }

    private void HandleActionTrigger(ActionTrigger trigger)
    {
        var spriteRenderer = this.GetComponent<SpriteRenderer>();
        switch (trigger)
        {
            case ActionTrigger.Damaged:
                this.interruptPattern = DamagedPattern();
                break;
        }

    }

    public void AddActionTrigger(ActionTrigger trigger)
    {
        this.actionTriggers.Add(trigger);
    }
}
