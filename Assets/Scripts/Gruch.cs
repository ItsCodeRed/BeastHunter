using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gruch : Enemy
{
    [SerializeField] private float hitRange = 3;
    [SerializeField] private float chargeRange = 8;
    [SerializeField] private float chanceOfCharge = 50;
    [SerializeField] private float decisionInterval = 3;
    [SerializeField] private float chanceOfJump = 50;
    [SerializeField] private float heightForJump = 3;
    [SerializeField] private float lungeDelay;
    [SerializeField] private float attackDelay;
    [SerializeField] private float attackLength;
    [SerializeField] private float attackDownTime;
    [SerializeField] private float deathLength;
    [SerializeField] private Vector2 attackLungeMovement;
    [SerializeField] private float attackLungeLength;
    [SerializeField] private Vector2 wallCheckVector;

    [SerializeField] private float jumpDelay;

    [SerializeField] private HitBox mainAttackHitbox;

    [SerializeField] private AudioSource swipeSound;
    [SerializeField] private AudioSource deathSound;

    private float decisionTimer = 0;
    private bool attacking = false;
    private bool jumping = false;
    private bool hasJumped = false;

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (isDead) return;

        if (isAgroed && !attacking)
        {
            Vector2 displacement = Player.instance.transform.position - transform.position;

            if (displacement.magnitude < hitRange)
            {
                StartCoroutine(AttackRoutine());
                horizontalMovement = 0;
            }
            else
            {
                if (decisionTimer > 0)
                {
                    decisionTimer -= Time.fixedDeltaTime;

                    if (displacement.magnitude >= chargeRange)
                    {
                        horizontalMovement = Mathf.Sign(displacement.x);
                    }
                }
                else
                {
                    decisionTimer = decisionInterval;

                    Vector2 faceDirection = new Vector2(transform.localScale.x, 1);

                    if (displacement.y > heightForJump || Physics2D.OverlapCircle((Vector2)transform.position + Vector2.Scale(wallCheckVector, faceDirection), 0.1f) != null)
                    {
                        float randNum = Random.Range(0f, 100f);
                        if (randNum < chanceOfJump)
                        {
                            StartCoroutine(JumpRoutine());
                        }
                    }

                    if (displacement.magnitude < chargeRange)
                    {
                        bool isCharging = true;

                        float randNum = Random.Range(0f, 100f);
                        if (randNum > chanceOfCharge)
                        {
                            isCharging = false;
                        }

                        horizontalMovement = !isCharging ? 0 : Mathf.Sign(displacement.x);
                    }
                }
            }
        }
        else
        {
            horizontalMovement = 0;
        }

        if (isGrounded && hasJumped && !jumping && !attacking)
        {
            hasJumped = false;
            animator.Play("Land");
        }
        else if (isGrounded && !jumping && !animator.GetCurrentAnimatorStateInfo(0).IsName("Land"))
        {
            if (horizontalMovement != 0)
            {
                animator.Play("Walk");
                FaceForward(false);
            }
            else
            {
                if (!attacking)
                {
                    animator.Play("Idle");
                    FacePlayer(false);
                }
            }
        }
    }

    public IEnumerator JumpRoutine()
    {
        FacePlayer(false);
        animator.Play("Jump");
        hasJumped = true;
        jumping = true;

        yield return new WaitForSeconds(jumpDelay);

        Jump();

        yield return new WaitForSeconds(0.1f);
        jumping = false;
    }

    public IEnumerator AttackRoutine()
    {
        FacePlayer(false);
        attacking = true;
        animator.Play("Attack", -1, 0f);
        StartCoroutine(LungeRoutine());

        yield return new WaitForSeconds(attackDelay);

        bool didHit = false;

        swipeSound.Play();

        float timer = 0;
        while (timer < attackLength)
        {
            bool hit = !didHit && Attack(mainAttackHitbox);
            didHit = didHit || hit;
            if (hit)
            {
                Player.instance.gameObject.layer = LayerMask.NameToLayer("Invincible");
            }
            timer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(attackDownTime);

        if (didHit) Player.instance.gameObject.layer = LayerMask.NameToLayer("Player");

        attacking = false;
    }

    public IEnumerator LungeRoutine()
    {
        yield return new WaitForSeconds(lungeDelay);

        float startVel = attackLungeMovement.x * -Mathf.Sign(transform.localScale.x);

        float timer = attackLungeLength;
        applyHorizontalMovement = false;
        while (timer > 0)
        {
            body.velocity = new Vector2(startVel * (timer / attackLungeLength), body.velocity.y);

            timer -= Time.deltaTime;

            yield return null;
        }
        applyHorizontalMovement = true;
    }

    public override void Die()
    {
        base.Die();
        gameObject.layer = LayerMask.NameToLayer("Corpse");
        body.sharedMaterial = corpseMat;
        applyHorizontalMovement = false;
        animator.Play("Dead");
        StopAllCoroutines();
    }

    public void PlayDeathSound()
    {
        deathSound.Play();
    }
}
