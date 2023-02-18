using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Dthaech : Enemy
{
    [SerializeField] private float hitRange = 3;
    [SerializeField] private float chargeRange = 8;
    [SerializeField] private float chanceOfCharge = 50;
    [SerializeField] private float decisionInterval = 3;

    [SerializeField] private HitBox biteHitbox;
    [SerializeField] private float chanceOfBite = 50;
    [SerializeField] private float biteDelay = 0.5f;
    [SerializeField] private float biteDownTime = 0.5f;
    [SerializeField] private float biteMinDiff = -2;

    [SerializeField] private HitBox tailHitbox;
    [SerializeField] private float chanceOfTail = 70;
    [SerializeField] private float tailBehindTheshold;
    [SerializeField] private float tailDelay = 0.5f;
    [SerializeField] private float tailDownTime = 0.5f;

    [SerializeField] private HitBox flameBreathHitbox;
    [SerializeField] private float chanceOfFlame = 50;
    [SerializeField] private float flameDelay = 0.5f;
    [SerializeField] private float flameLength = 0.5f;
    [SerializeField] private float flameDownTime = 0.5f;
    [SerializeField] private float flameRange = 3;

    [SerializeField] private HitBox flipHitbox;
    [SerializeField] private float chanceOfFlip = 50;
    [SerializeField] private float flipDelay = 0.5f;
    [SerializeField] private float flipRange = 0.5f;
    [SerializeField] private float flipDownTime = 0.5f;

    [SerializeField] private float deathLength;

    public Collider2D tailCollider;
    public Collider2D legCollider;
    public Transform legHeight;
    public Transform legMinimum;
    public Transform legBottom;

    [SerializeField] AudioSource boomSound;
    [SerializeField] AudioSource fireSound;

    private float decisionTimer = 0;
    private bool attacking = false;

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (isDead) return;

        if (isAgroed)
        {
            Vector2 displacement = Player.instance.transform.position - transform.position;

            if (displacement.magnitude < hitRange && !attacking && displacement.y > biteMinDiff)
            {
                horizontalMovement = 0;

                float randNum = Random.Range(0f, 100f);
                if (Vector2.Scale(displacement, new Vector2(Mathf.Sign(transform.localScale.x), 1)).x < tailBehindTheshold && ((randNum < chanceOfTail) || displacement.x * transform.localScale.x < 0))
                {
                    StartCoroutine(TailRoutine());
                    return;
                }

                randNum = Random.Range(0f, 100f);
                if (randNum < chanceOfBite)
                {
                    StartCoroutine(BiteRoutine());
                    return;
                }
            }
            if (decisionTimer > 0)
            {
                decisionTimer -= Time.fixedDeltaTime;

                if (displacement.magnitude >= chargeRange)
                {
                    horizontalMovement = Mathf.Sign(displacement.x);
                }
            }
            else if (!attacking)
            {
                decisionTimer = decisionInterval;

                if (displacement.magnitude < flameRange)
                {
                    float randNum = Random.Range(0f, 100f);
                    if (randNum < chanceOfFlame)
                    {
                        StartCoroutine(FlameRoutine());
                        return;
                    }
                    else if (randNum < chanceOfFlame + chanceOfFlip)
                    {
                        StartCoroutine(FlipRoutine());
                        return;
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

        if (!isAgroed || attacking)
        {
            horizontalMovement = 0;
        }

        if (!attacking)
        {
            if (horizontalMovement != 0)
            {
                animator.Play("Walk");
                FaceForward();
            }
            else
            {
                animator.Play("Idle");
                FacePlayer();
            }
        }
    }

    private IEnumerator BiteRoutine()
    {
        attacking = true;
        animator.Play("Bite");
        FacePlayer();

        yield return new WaitForSeconds(biteDelay);

        Attack(biteHitbox);

        yield return new WaitForSeconds(biteDownTime);

        attacking = false;
    }

    private IEnumerator FlipRoutine()
    {
        attacking = true;
        animator.Play("FlipAttack");
        FacePlayer();

        yield return new WaitForSeconds(flipDelay);

        transform.position += new Vector3(flipRange * Mathf.Sign(transform.localScale.x), 0, 0);
        Attack(flipHitbox);
        boomSound.Play();

        yield return new WaitForSeconds(flipDownTime);

        attacking = false;
    }


    private IEnumerator TailRoutine()
    {
        attacking = true;
        animator.Play("TailAttack");
        tailCollider.enabled = false;

        yield return new WaitForSeconds(tailDelay);

        bool didHit = Attack(tailHitbox);
        if (didHit) Player.instance.gameObject.layer = LayerMask.NameToLayer("Invincible");

        yield return new WaitForSeconds(tailDownTime);

        if (didHit) Player.instance.gameObject.layer = LayerMask.NameToLayer("Player");
        tailCollider.enabled = true;
        attacking = false;
    }

    private IEnumerator FlameRoutine()
    {
        attacking = true;
        animator.Play("FlameBreath");
        FacePlayer();

        yield return new WaitForSeconds(flameDelay);

        fireSound.Play();

        float timer = 0;
        while (timer < flameLength)
        {
            Attack(flameBreathHitbox);
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        yield return new WaitForSeconds(flameDownTime);

        attacking = false;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.otherCollider == legCollider && !collision.gameObject.CompareTag("Player"))
        {
            ContactPoint2D contact = collision.GetContact(0);
            if (contact.point.y < legHeight.position.y && contact.point.y > legMinimum.position.y)
            {
                transform.position = transform.position + new Vector3(contact.point.x - legBottom.position.x, contact.point.y - legBottom.position.y, 0);
            }
        }
    }

    public override void Die()
    {
        base.Die();
        gameObject.layer = LayerMask.NameToLayer("Corpse");
        tailCollider.gameObject.layer = LayerMask.NameToLayer("Corpse");
        legCollider.gameObject.layer = LayerMask.NameToLayer("Corpse");
        body.sharedMaterial = corpseMat;
        applyHorizontalMovement = false;
        animator.Play("Dead");
        StopAllCoroutines();
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(deathLength);
        boomSound.Play();
    }
}
