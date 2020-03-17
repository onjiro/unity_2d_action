using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(KinematicBody))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    public float maxSpeed;
    public float jumpInitialSpeed;
    // ショットオブジェクト
    public GameObject bullet;
    public GameObject deadEffect;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb2d;
    private bool grounding;
    private bool controllable = true;
    private List<ActionTrigger> actionTriggers = new List<ActionTrigger>();
    private KinematicBody kinematic;
    void OnEnable()
    {
        Instance = this;
    }

    void OnDisable()
    {
        if (Instance == this) Instance = null;
    }

    private void Awake()
    {
        this.spriteRenderer = this.GetComponent<SpriteRenderer>();
        this.rb2d = this.GetComponent<Rigidbody2D>();
        this.kinematic = this.GetComponent<KinematicBody>();
    }

    // Update is called once per frame
    void Update()
    {
        // y方向の速度は特に何もなければ現在の速度を維持
        this.kinematic.targetVelocity.y = this.rb2d.velocity.y;

        this.controllable = !this.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsTag("wait");
        if (this.controllable)
        {
            // 横方向の入力
            var inputX = Input.GetAxis("Horizontal");
            if (inputX != 0)
            {
                this.spriteRenderer.flipX = (inputX < 0);
            }
            this.kinematic.targetVelocity.x = inputX * this.maxSpeed;

            // ジャンプ
            if (Input.GetButtonDown("Jump") && this.grounding)
            {
                this.actionTriggers.Add(ActionTrigger.Jump);
            }

            // アタック
            if (Input.GetButtonDown("Attack"))
            {
                this.actionTriggers.Add(ActionTrigger.Attack);
            }
        }
        else
        {
            this.kinematic.targetVelocity.x = this.rb2d.velocity.x;
        }
    }

    public void AddActionTrigger(ActionTrigger trigger)
    {
        this.actionTriggers.Add(trigger);
    }

    // FixedUpdate is called on Physics Cycle, called some times on frame
    void FixedUpdate()
    {
        // アニメーション状態でコントロールが禁止されている場合はコントロールできない状態
        var animator = this.GetComponent<Animator>();
        this.controllable = !animator.GetCurrentAnimatorStateInfo(0).IsTag("wait");

        this.grounding = this.kinematic.IsGrounding();
        if (this.controllable)
        {
            this.actionTriggers.Distinct().ToList().ForEach(HandleActionTrigger);
        }
        else
        {
            if (this.grounding)
            {
                this.kinematic.targetVelocity.x = 0;
            }
        }
        this.actionTriggers.Clear();

        // アニメーションの変更
        animator.SetFloat("velocityX", Mathf.Abs(this.rb2d.velocity.x));
        animator.SetFloat("velocityY", this.rb2d.velocity.y);
        animator.SetBool("grounding", this.grounding);
    }

    private void HandleActionTrigger(ActionTrigger trigger)
    {
        switch (trigger)
        {
            case ActionTrigger.Jump:
                this.kinematic.targetVelocity.y = this.jumpInitialSpeed;
                break;
            case ActionTrigger.Attack:
                this.GetComponent<Animator>().SetTrigger("attacking");
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
            case ActionTrigger.Damaged:
                this.kinematic.targetVelocity.x = (this.spriteRenderer.flipX) ? 2 : -2;
                this.kinematic.targetVelocity.y = 1.8f;
                this.GetComponent<Animator>().SetTrigger("damaged");
                break;
            case ActionTrigger.Dead:
                Instantiate(this.deadEffect, this.transform.position, this.transform.rotation);
                GameManager.Instance.PlayerDead();
                Destroy(this.gameObject);
                break;
        }
    }

    public enum ActionTrigger
    {
        Jump,
        Attack,
        Damaged,
        Dead,
    }
}
