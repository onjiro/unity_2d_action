using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float maxSpeed;
    public float jumpInitialSpeed;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb2d;
    // 現在の速度
    private Vector2 velocity;
    // Physics Cycle の間に到達する最高速度
    private Vector2 targetVelocity;
    private ContactFilter2D contactFilter;
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
        if (Input.GetButtonUp("Jump"))
        {
            this.targetVelocity.y = this.jumpInitialSpeed;
        }
    }

    // FixedUpdate is called on Physics Cycle, called some times on frame
    void FixedUpdate()
    {
        this.velocity = CalculateVelocity(this.velocity, this.targetVelocity);
        this.rb2d.MovePosition(CalculatePosition(this.rb2d.position, this.velocity));
    }

    // 物理演算ループでの速度を計算する
    private Vector2 CalculateVelocity(Vector2 currentVelocity, Vector2 targetVelocity)
    {
        Vector2 velocity = new Vector2(targetVelocity.x, targetVelocity.y);
        velocity += Physics2D.gravity * Time.deltaTime;

        return velocity;
    }

    // 物理演算ループでの位置を計算する
    private Vector2 CalculatePosition(Vector2 currentPosition, Vector2 velocity)
    {
        var deltaPosition = velocity * Time.deltaTime;
        var deltaMagnitude = deltaPosition.magnitude;

        // 他のオブジェクトとのあたり判定を求める
        // 結果は hitBuffers に入ってかえってくる
        var hitBuffers = new List<RaycastHit2D>();
        this.rb2d.Cast(deltaPosition, this.contactFilter, hitBuffers, deltaMagnitude + 0.01f);
        foreach (var hitBuffer in hitBuffers)
        {
            // ヒットしたオブジェクトからの法線ベクトルを求める
            // y 方向が 0.01 より大きければ地面に設置したとみなせる
            var normal = hitBuffer.normal;
            Debug.Log($"normal({normal.x}, {normal.y}), {deltaPosition.y}, {hitBuffer.distance}, {hitBuffer.distance + 0.01f}");
            if (normal.y > 0.01f)
            {
                if (deltaPosition.y < -hitBuffer.distance + 0.01f)
                {
                    deltaPosition.y = -hitBuffer.distance + 0.01f;
                }
            }
        }

        return currentPosition + deltaPosition;
    }
}
