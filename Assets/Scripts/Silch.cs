using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Silch : Enemy
{
    [SerializeField] private float hitRange = 3;
    [SerializeField] private float chargeRange = 8;
    [SerializeField] private float chanceOfCharge = 50;
    [SerializeField] private float decisionInterval = 3;
    [SerializeField] private float chanceOfJump = 50;
    [SerializeField] private float heightForJump = 3;
    [SerializeField] private float attackDelay;
    [SerializeField] private float attackLength;
    [SerializeField] private float attackDownTime;
    [SerializeField] private float deathLength;
    [SerializeField] private Vector2 wallCheckVector;

    [SerializeField] AudioSource fireSound;

    [SerializeField] private HitBox mainAttackHitbox;

    private float decisionTimer = 0;
    private bool attacking = false;

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
                            Jump();
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

        if (horizontalMovement != 0)
        {
            animator.Play("Walk");
            FaceForward();
        }
        else
        {
            if (!attacking)
            {
                animator.Play("Idle");
                FacePlayer();
            }
        }
    }

    public IEnumerator AttackRoutine()
    {
        FacePlayer();
        attacking = true;
        animator.Play("Attack");
        yield return new WaitForSeconds(attackDelay);

        fireSound.Play();

        float timer = 0;
        while (timer < attackLength)
        {
            Attack(mainAttackHitbox);
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        yield return new WaitForSeconds(attackDownTime);

        attacking = false;
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
}
