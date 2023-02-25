using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Animator animator;

    [SerializeField] private Vector2 offset;

    [SerializeField] protected PhysicsMaterial2D corpseMat;
    [SerializeField] protected float agroRange = 30;
    [SerializeField] protected float jumpPower = 10;
    [SerializeField] protected float knockbackResistence = 1;
    [SerializeField] protected float stunResistence = 1;
    [SerializeField] protected float movementSpeed = 12;
    [SerializeField] protected BloodParticle bloodParticle;

    public LayerMask hitMask;

    protected bool isAgroed;

    protected Rigidbody2D body;
    protected bool isGrounded;
    private float stunTimer = 0;
    private float noHitTimer = 0;

    public bool isDead = false;

    protected float horizontalMovement;
    protected bool applyHorizontalMovement = true;

    public MonsterInstance monsterInstance;

    public List<Enemy> additionalEnemies = new List<Enemy>();

    public int health;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    public bool Hit(int damage, Vector2 knockback, float stunTime, float hitLength)
    {
        if (noHitTimer <= 0)
        {
            health -= damage;
            body.velocity = knockback / knockbackResistence;
            stunTimer = stunTime / stunResistence;
            noHitTimer = hitLength;

            monsterInstance.currentHealth = health;

            if (health <= 0 && !isDead)
            {
                isDead = true;
                Die();
            }

            OnHit();

            return true;
        }

        return false;
    }

    public virtual void OnHit()
    {

    }

    public virtual void FixedUpdate()
    {
        isAgroed = Player.instance == null ? false : Vector2.Distance(transform.position, Player.instance.transform.position) < agroRange;

        if (noHitTimer > 0)
        {
            noHitTimer -= Time.deltaTime;
        }
        if (stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
            return;
        }

        if (applyHorizontalMovement)
        {
            body.velocity = new Vector2(horizontalMovement * movementSpeed, body.velocity.y);
        }
    }

    protected void Jump()
    {
        if (isGrounded && stunTimer <= 0)
        {
            body.velocity = new Vector2(body.velocity.x, jumpPower);
        }
    }

    public virtual void Die()
    {
        GameManager.instance.ChangeMeat(monsterInstance.monsterAsset.meat);
    }

    public void FacePlayer(bool facingRightByDefault = true)
    {
        if (Player.instance != null)
        {
            transform.localScale = new Vector3((facingRightByDefault ? 1 : -1) * Mathf.Abs(transform.localScale.x) * Mathf.Sign((Player.instance.transform.position - (transform.position + (Vector3)offset * Mathf.Sign(transform.localScale.x))).x), transform.localScale.y, 1f);
        }
    }

    public void FaceForward(bool facingRightByDefault = true)
    {
        if (horizontalMovement != 0)
        {
            transform.localScale = new Vector3((facingRightByDefault ? 1 : -1) * Mathf.Abs(transform.localScale.x) * Mathf.Sign(horizontalMovement), transform.localScale.y, 1f);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.isTrigger)
        {
            isGrounded = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.isTrigger)
        {
            isGrounded = false;
        }
    }

    public bool Attack(HitBox hitbox)
    {
        Vector2 facingVector = new Vector2(Mathf.Sign(transform.localScale.x), 1);
        Collider2D[] cols = Physics2D.OverlapBoxAll(transform.position + Vector3.Scale(hitbox.displacement, facingVector), hitbox.size, hitbox.angle, hitMask);

        foreach(Collider2D col in cols)
        {
            if (col.CompareTag("Player"))
            {
                int modifier = monsterInstance == null ? 1 : (monsterInstance.monsterAsset.isBig ? Mathf.CeilToInt(hitbox.damage * GameManager.instance.attackMultiplier * (GameManager.instance.dayNum - 1)) : 0);
                if (Player.instance.TakeDamage(hitbox.damage + modifier, Vector3.Scale(hitbox.knockback, facingVector), hitbox.hitLength, hitbox.stun))
                {
                    if (hitbox.isPoison)
                    {
                        Player.instance.isPoisoned = true;
                    }
                    if (hitbox.lifeSteal > 0)
                    {
                        health = Mathf.Min(health + Mathf.RoundToInt((hitbox.damage + modifier) * hitbox.lifeSteal), monsterInstance.monsterAsset.maxHealth + (GameManager.instance.dayNum - 1) * GameManager.instance.healthIncrement);
                        BloodParticle blood = Instantiate(bloodParticle, Player.instance.transform.position, Quaternion.identity);
                        blood.target = transform;
                    }

                    return true;
                }
            }
        }

        return false;
    }
}

[Serializable]
public struct HitBox
{
    public Vector2 size;
    public Vector2 displacement;
    public float angle;
    public int damage;
    public float hitLength;
    public float stun;
    public Vector2 knockback;
    public bool isPoison;
    public float lifeSteal;
}
