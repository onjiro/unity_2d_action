using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class KinematicBody : MonoBehaviour
{
    // 設置しているとみなす距離
    public const float GROUNDING_DISTANCE = 0.04f;
    // 物体の接触時に開けておく距離
    private const float SHELL_RADIUS = 0.01f;
    // 重力による終端速度
    private const float GRAVITY_TERMINAL_VELOCITY = -8f;
    public Vector2 targetVelocity = Vector2.zero;
    private Rigidbody2D rb2d;
    private ContactFilter2D contactFilter;

    private void Awake()
    {
        this.rb2d = gameObject.GetComponent<Rigidbody2D>();

        // 移動時の当たり判定の対象のフィルタ。壁のレイヤーだけを対象にする
        this.contactFilter.useTriggers = false;
        this.contactFilter.SetLayerMask(LayerMask.GetMask("Wall"));
        this.contactFilter.useLayerMask = true;
    }

    private void FixedUpdate()
    {
        this.rb2d.velocity = this.CalculateVelocity(this.rb2d.velocity, this.targetVelocity, this.IsGrounding());
        this.rb2d.MovePosition(this.CalculatePosition(this.rb2d.velocity));
    }


    // 指定した Rigidbody2D が地面に設置しているか判定する
    public bool IsGrounding()
    {
        var hitBuffers = new List<RaycastHit2D>();
        this.rb2d.Cast(new Vector2(0, -GROUNDING_DISTANCE), this.contactFilter, hitBuffers, GROUNDING_DISTANCE);

        // 下方向にヒットした物体の法線ベクトルが (0, 1) となっているものがあれば設置しているとみなせる
        return hitBuffers.Where(buffer => buffer.normal.y > 0).Count() > 0;
    }

    // 物理演算ループでの速度を計算する
    public Vector2 CalculateVelocity(Vector2 currentVelocity, Vector2 targetVelocity, bool grounding)
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
            velocity.y = Mathf.Max((currentVelocity + Physics2D.gravity * Time.deltaTime).y, GRAVITY_TERMINAL_VELOCITY);
        }

        return velocity;
    }

    // 物理演算ループでの位置を計算する
    public Vector2 CalculatePosition(Vector2 velocity)
    {
        var currentPosition = this.rb2d.position;
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

}