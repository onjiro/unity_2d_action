using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(KinematicBody))]
public class Boss1Controller : MonoBehaviour, EnemyControllerInterface
{
    public float moveSpeed = 1.2f;
    public float initialJumpSpeed = 8f;
    public GameObject upperBullet;
    private KinematicBody kinematic;
    private bool grounding = true;
    private List<ActionTrigger> actionTriggers = new List<ActionTrigger>();
    private IEnumerator interruptPattern = new ArrayList().GetEnumerator();

    // Start is called before the first frame update
    private void Awake()
    {
        this.kinematic = GetComponent<KinematicBody>();
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

            if (false)
            // if (hittableEnemy.currentHp >= hittableEnemy.maxHp / 2)
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
                this.kinematic.targetVelocity.x = spriteRenderer.flipX ? -this.moveSpeed * 0.5f : this.moveSpeed * 0.5f;
                for (int i = 0; i < 90; i++)
                {
                    yield return null;
                }
                for (int i = 0; i < 60; i++)
                {
                    this.kinematic.targetVelocity.x = this.kinematic.targetVelocity.x * 0.7f;
                    yield return null;
                }
                this.kinematic.targetVelocity.x = 0f;
                yield return new WaitForSeconds(1f);
            }

            // jump
            this.TurnToPlayerDirection();
            this.kinematic.targetVelocity.x = spriteRenderer.flipX ? -this.moveSpeed : this.moveSpeed;
            this.kinematic.targetVelocity.y = this.initialJumpSpeed;
            yield return new WaitForSeconds(0.1f);
            this.kinematic.targetVelocity.y = 0f;

            // wait for grounding
            while (!this.grounding)
            {
                yield return null;
            }
            this.TurnToPlayerDirection();
            this.kinematic.targetVelocity.x = 0f;
            this.kinematic.targetVelocity.y = 0f;
            yield return new WaitForSeconds(0.2f);
        }
    }

    // HPが少なくなったときの行動パターン
    //
    // 以下を繰り返す
    // ・ジャンプ
    private IEnumerator Pattern2()
    {
        var spriteRenderer = this.GetComponent<SpriteRenderer>();

        while (true)
        {
            // back jump
            this.TurnToPlayerDirection();
            this.kinematic.targetVelocity.x = this.GetComponent<SpriteRenderer>().flipX ? this.moveSpeed : -this.moveSpeed;
            this.kinematic.targetVelocity.y = this.initialJumpSpeed;
            yield return new WaitForSeconds(0.1f);
            this.kinematic.targetVelocity.y = 0f;

            // wait for grounding
            while (!this.grounding)
            {
                yield return null;
            }
            this.TurnToPlayerDirection();
            this.kinematic.targetVelocity.x = 0f;
            this.kinematic.targetVelocity.y = 0f;
            yield return new WaitForSeconds(0.2f);

            // charge
            this.GetComponent<Animator>().SetTrigger("charge");
            for (int i = 0; i < 180; i++)
            {
                var size = Random.Range(0.95f, 1.00f);
                gameObject.transform.localScale = new Vector3(size, size, 1f);
                yield return null;
                yield return null;
                yield return null;
            }
            gameObject.transform.localScale = Vector3.one;
            yield return new WaitForSeconds(0.5f);

            // upper
            this.GetComponent<Animator>().SetTrigger("upper");
            var bulletDirection = this.GetComponent<SpriteRenderer>().flipX ? -1f : 1f;

            Instantiate(this.upperBullet, transform.TransformPoint(new Vector2(bulletDirection * 2f, -2f)), transform.rotation);
            for (int i = 0; i < 60; i++)
            {
                yield return null;
            }
            Instantiate(this.upperBullet, transform.TransformPoint(new Vector2(bulletDirection * 4f, -2f)), transform.rotation);
            for (int i = 0; i < 60; i++)
            {
                yield return null;
            }
            Instantiate(this.upperBullet, transform.TransformPoint(new Vector2(bulletDirection * 6f, -2f)), transform.rotation);
            for (int i = 0; i < 60; i++)
            {
                yield return null;
            }
            yield return new WaitForSeconds(1f);

            // dash
            this.TurnToPlayerDirection();
            this.kinematic.targetVelocity.x = spriteRenderer.flipX ? -this.moveSpeed * 1.5f : this.moveSpeed * 1.5f;
            yield return new WaitForSeconds(1f);
            this.kinematic.targetVelocity.x = 0f;
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator DamagedPattern()
    {
        this.GetComponent<Animator>().SetTrigger("damaged");

        var spriteRenderer = this.GetComponent<SpriteRenderer>();
        this.kinematic.targetVelocity.x = spriteRenderer.flipX ? this.moveSpeed * 0.3f : -this.moveSpeed * 0.3f;
        for (int i = 0; i < 90; i++)
        {
            this.kinematic.targetVelocity.x *= 0.9f;
            yield return null;
        }
        this.kinematic.targetVelocity.x = 0f;
        yield return new WaitForSeconds(0.3f);
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
