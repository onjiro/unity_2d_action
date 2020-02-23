using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    // 設置しているとみなす距離
    private const float GROUNDING_DISTANCE = 0.04f;
    // 物体の接触時に開けておく距離
    private const float SHELL_RADIUS = 0.01f;
    // 重力による終端速度
    private const float GRAVITY_TERMINAL_VELOCITY = -8f;
    public float maxSpeed;
    public float jumpInitialSpeed;
    // ショットオブジェクト
    public GameObject bullet;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb2d;
    private bool grounding;
    // 現在の速度
    private Vector2 velocity;
    // Physics Cycle の間に到達する最高速度
    private Vector2 targetVelocity;
    private ContactFilter2D contactFilter;
    private bool controllable = true;
    private List<ActionTrigger> actionTriggers = new List<ActionTrigger>();

    // Start is called before the first frame update
    void Awake()
    {
        this.spriteRenderer = this.GetComponent<SpriteRenderer>();
        this.rb2d = this.GetComponent<Rigidbody2D>();

        // 当たり判定の対象のフィルタ。レイヤーが同じものだけを対象にする
        this.contactFilter.useTriggers = false;
        this.contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        this.contactFilter.useLayerMask = true;
    }

    // Update is called once per frame
    void Update()
    {
        // y方向の速度は特に何もなければ現在の速度を維持
        this.targetVelocity.y = this.velocity.y;

        // 横方向の入力
        var inputX = Input.GetAxis("Horizontal");
        if (inputX != 0)
        {
            this.spriteRenderer.flipX = (inputX < 0);
        }
        this.targetVelocity.x = inputX * this.maxSpeed;

        // ジャンプ
        if (Input.GetButtonDown("Jump"))
        {
            this.actionTriggers.Add(ActionTrigger.Jump);
        }

        // アタック
        if (Input.GetButtonDown("Attack"))
        {
            this.actionTriggers.Add(ActionTrigger.Attack);
        }
    }

    // FixedUpdate is called on Physics Cycle, called some times on frame
    void FixedUpdate()
    {
        // アニメーション状態でコントロールが禁止されている場合はコントロールできない状態
        var animator = this.GetComponent<Animator>();
        this.controllable = !animator.GetCurrentAnimatorStateInfo(0).IsTag("wait");

        this.grounding = IsGrounding(this.rb2d);
        if (this.controllable)
        {
            foreach (var trigger in this.actionTriggers)
            {
                switch (trigger)
                {
                    case ActionTrigger.Jump:
                        this.targetVelocity.y = this.jumpInitialSpeed;
                        break;
                    case ActionTrigger.Attack:
                        animator.SetTrigger("attacking");
                        var bullet = (GameObject)Instantiate(this.bullet, this.rb2d.position, this.transform.rotation);
                        var controller = bullet.GetComponent<BulletController>();
                        var bulletRb2d = bullet.GetComponent<Rigidbody2D>();
                        if (this.spriteRenderer.flipX)
                        {
                            bulletRb2d.position += this.bullet.GetComponent<Rigidbody2D>().position * new Vector2(-1, 1);
                            controller.speed *= -1;
                        }
                        else
                        {
                            bulletRb2d.position += this.bullet.GetComponent<Rigidbody2D>().position;
                        }
                        break;
                }
            }
            this.actionTriggers.Clear();

            this.velocity = CalculateVelocity(this.velocity, this.targetVelocity, this.grounding);
        }
        else
        {
            this.velocity = CalculateVelocity(this.velocity, new Vector2(0, 0), this.grounding);
        }
        this.rb2d.MovePosition(CalculatePosition(this.rb2d.position, this.velocity));

        // アニメーションの変更
        animator.SetFloat("velocityX", Mathf.Abs(this.targetVelocity.x));
        animator.SetFloat("velocityY", this.targetVelocity.y);
        animator.SetBool("grounding", this.grounding);
    }

    // 指定した Rigidbody2D が地面に設置しているか判定する
    private bool IsGrounding(Rigidbody2D rb2d)
    {
        var hitBuffers = new List<RaycastHit2D>();
        rb2d.Cast(new Vector2(0, -GROUNDING_DISTANCE), this.contactFilter, hitBuffers, GROUNDING_DISTANCE);

        // 下方向にヒットした物体の法線ベクトルが (0, 1) となっているものがあれば設置しているとみなせる
        return hitBuffers.Where(buffer => buffer.normal.y > 0).Count() > 0;
    }

    // 物理演算ループでの速度を計算する
    private Vector2 CalculateVelocity(Vector2 currentVelocity, Vector2 targetVelocity, bool grounding)
    {
        Vector2 velocity = new Vector2(targetVelocity.x, targetVelocity.y);
        // 接地していて下方向への速度が生じていればリセットする
        if (grounding && velocity.y <= 0)
        {
            velocity.y = -2f;
        }
        // 接地していなければ下方向に加速させる
        else if (!grounding)
        {
            velocity.y = Mathf.Max((velocity + Physics2D.gravity * Time.deltaTime).y, GRAVITY_TERMINAL_VELOCITY);
        }

        return velocity;
    }

    // 物理演算ループでの位置を計算する
    private Vector2 CalculatePosition(Vector2 currentPosition, Vector2 velocity)
    {
        var deltaPosition = velocity * Time.deltaTime;

        // 他のオブジェクトとのあたり判定を求める
        // x方向、y方向をまとめて判定すると片方の軸しかあたり判定を検出せず、壁をすり抜けてしまうことがあるため
        // x方向、y方向に分解して他のオブジェクトとの当たり判定を求める。

        var fractionY = 0.0f;

        // 結果は hitBuffers に入ってかえってくる
        var xHitBuffers = new List<RaycastHit2D>();
        var xDelta = new Vector2(deltaPosition.x, 0);
        this.rb2d.Cast(xDelta, this.contactFilter, xHitBuffers, xDelta.magnitude + SHELL_RADIUS);
        foreach (var hitBuffer in xHitBuffers)
        {
            // ヒットしたオブジェクトからの法線ベクトルを求める
            // 画面左下が (0, 0) という座標系になっているため、以下のようになる
            // 左側の壁にぶつかると  (1, 0)
            // 右側の壁にぶつかると (-1, 0)
            var normal = hitBuffer.normal;
            if (Mathf.Abs(normal.x) > 0.5)
            {
                fractionY = 0;
                if (normal.x > 0f)
                {
                    deltaPosition.x = Mathf.Max(deltaPosition.x, -hitBuffer.distance + SHELL_RADIUS);
                }
                else
                {
                    deltaPosition.x = Mathf.Min(deltaPosition.x, hitBuffer.distance - SHELL_RADIUS);
                }
            }
            else
            {
                deltaPosition.x = deltaPosition.x * (normal.y / normal.magnitude);
                fractionY = deltaPosition.x * Mathf.Abs(normal.x / normal.magnitude);
            }

        }

        var yHitBuffers = new List<RaycastHit2D>();
        var yDelta = new Vector2(0, deltaPosition.y);
        this.rb2d.Cast(yDelta, this.contactFilter, yHitBuffers, yDelta.magnitude + SHELL_RADIUS);
        foreach (var hitBuffer in yHitBuffers)
        {
            // ヒットしたオブジェクトからの法線ベクトルを求める
            // 画面左下が (0, 0) という座標系になっているため、以下のようになる
            // 地面にぶつかると (0,  1)
            // 天井にぶつかると (0, -1)
            var normal = hitBuffer.normal;
            if (normal.y > 0)
            {
                deltaPosition.y = Mathf.Max(deltaPosition.y, -hitBuffer.distance + SHELL_RADIUS);
            }
            else if (normal.y < 0)
            {
                deltaPosition.y = Mathf.Min(deltaPosition.y, hitBuffer.distance - SHELL_RADIUS);
            }
        }
        deltaPosition.y += fractionY;

        return currentPosition + deltaPosition;
    }

    public enum ActionTrigger
    {
        Jump,
        Attack
    }
}
