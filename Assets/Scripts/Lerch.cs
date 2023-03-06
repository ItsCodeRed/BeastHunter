using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Lerch : Enemy
{
    [SerializeField] Projectile barbPrefab;
    [SerializeField] GameObject windPrefab;

    [SerializeField] private float hitRange = 3;
    [SerializeField] private float jumpInterval = 3;
    [SerializeField] private float chanceOfJump = 50;
    [SerializeField] private float chanceOfFly = 50;
    [SerializeField] private float chanceOfBarb = 50;
    [SerializeField] private float jumpDelay = 3;

    [SerializeField] private float wingAttackDelay;
    [SerializeField] private float wingAttackLandTime;
    [SerializeField] private Vector2 wingAttackVel;

    [SerializeField] private float launchDelay;
    [SerializeField] private float callDelay;
    [SerializeField] private float callDelay2;
    [SerializeField] private float launchLength;
    [SerializeField] private float flySpeed;
    [SerializeField] private float flyHeight;
    [SerializeField] private float maxFlyTime;
    [SerializeField] private float peckDelay;
    [SerializeField] private float peckRange;
    [SerializeField] private float peckSpeed;
    [SerializeField] private float peckBarbDelay;
    [SerializeField] private float peckBarbDownTime;
    [SerializeField] private float barbSpinSpeed;
    [SerializeField] private Vector2 peckBarbVel;

    [SerializeField] private float barbDelay;
    [SerializeField] private float barbDownTime;
    [SerializeField] private float barbSpeed;

    [SerializeField] private float deathLength;

    [SerializeField] private HitBox wingAttackMainHitbox;
    [SerializeField] private HitBox wingAttackSecondHitbox;

    [SerializeField] private HitBox peckAttackHitbox;

    [SerializeField] Transform peckBarbSpawn;
    [SerializeField] Transform barbSpawn;
    [SerializeField] Transform windParticleSpawn;

    [SerializeField] Collider2D legCollider;

    [SerializeField] AudioSource boomSound;
    [SerializeField] AudioSource wingSound;
    [SerializeField] AudioSource duckCallSound;
    [SerializeField] AudioSource throwSound;

    private float jumpTimer = 0;
    private bool attacking = false;
    private bool jumping = false;

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (isDead) return;

        if (isAgroed && !attacking)
        {
            Vector2 displacement = Player.instance.transform.position - transform.position;

            if (displacement.magnitude < hitRange)
            {
                StartCoroutine(AttackRoutine(displacement));
                horizontalMovement = 0;
            }
            else
            {
                if (jumpTimer > 0)
                {
                    jumpTimer -= Time.fixedDeltaTime;
                }
                else
                {
                    jumpTimer = jumpInterval;

                    float randNum = Random.Range(0f, 100f);
                    if (randNum < chanceOfBarb)
                    {
                        StartCoroutine(BarbRoutine());
                        return;
                    }

                    randNum = Random.Range(0f, 100f);
                    if (randNum < chanceOfFly)
                    {
                        StartCoroutine(FlyRoutine());
                        return;
                    }

                    randNum = Random.Range(0f, 100f);
                    if (randNum < chanceOfJump)
                    {
                        StartCoroutine(JumpRoutine(displacement));
                    }
                }
            }
        }

        if (isGrounded && !jumping)
        {
            horizontalMovement = 0;
        }

        if (!attacking && isGrounded && !jumping)
        {
            animator.Play("Idle");
            FacePlayer();
        }
    }

    public IEnumerator AttackRoutine(Vector2 displacement)
    {
        attacking = true;
        animator.Play("WingAttack");
        yield return new WaitForSeconds(wingAttackDelay);

        wingSound.Play();
        Attack(wingAttackMainHitbox);
        Attack(wingAttackSecondHitbox);

        applyHorizontalMovement = false;

        FacePlayer();
        Vector2 facingVector = new Vector2(Mathf.Sign(displacement.x), 1);

        GameObject wind = Instantiate(windPrefab, windParticleSpawn.transform.position, Quaternion.identity);
        wind.transform.localScale = new Vector2(-Mathf.Sign(displacement.x), 1);

        legCollider.enabled = false;
        body.velocity = Vector2.Scale(wingAttackVel, facingVector);

        yield return new WaitForSeconds(0.1f);
        yield return new WaitUntil(() => isGrounded);
        legCollider.enabled = true;

        animator.Play("Land");
        applyHorizontalMovement = true;

        yield return new WaitForSeconds(wingAttackLandTime);

        attacking = false;
    }

    public IEnumerator JumpRoutine(Vector2 displacement)
    {
        jumping = true;
        animator.Play("Jump");
        yield return new WaitForSeconds(jumpDelay);

        horizontalMovement = Mathf.Sign(displacement.x);
        Jump();

        yield return new WaitForSeconds(0.1f);
        jumping = false;
    }

    public IEnumerator FlyRoutine()
    {
        attacking = true;
        animator.Play("Launch");

        yield return new WaitForSeconds(callDelay);

        duckCallSound.Play();

        yield return new WaitForSeconds(callDelay2);

        duckCallSound.Play();

        yield return new WaitForSeconds(launchDelay - callDelay - callDelay2);

        Jump();
        applyHorizontalMovement = false;

        yield return new WaitForSeconds(launchLength - launchDelay);

        animator.Play("Fly");

        float flyTimer = 0;
        while (Mathf.Abs((Player.instance.transform.position - transform.position).x) > peckRange && flyTimer < maxFlyTime)
        {
            flyTimer += Time.fixedDeltaTime;

            Vector3 targetPosition = new Vector2(Player.instance.transform.position.x, Player.instance.transform.position.y + flyHeight);

            body.velocity = Vector2.zero;
            transform.position += Time.fixedDeltaTime * flySpeed * (Vector3)((Vector2)(targetPosition - transform.position)).normalized;

            yield return new WaitForFixedUpdate();
        }
        body.velocity = Vector2.down * peckSpeed;

        yield return new WaitUntil(() => isGrounded);
        applyHorizontalMovement = true;

        animator.Play("Peck");

        yield return new WaitForSeconds(peckDelay);

        Attack(peckAttackHitbox);
        boomSound.Play();

        yield return new WaitForSeconds(peckBarbDelay);

        throwSound.Play();

        Projectile barb = Instantiate(barbPrefab, peckBarbSpawn.position, peckBarbSpawn.rotation);
        barb.body.velocity = Vector2.Scale(peckBarbVel, new Vector2(Mathf.Sign(transform.localScale.x), 1));
        barb.body.gravityScale = 0;
        barb.body.angularVelocity = barbSpinSpeed;

        yield return new WaitForSeconds(peckBarbDownTime);

        attacking = false;
    }

    public IEnumerator BarbRoutine()
    {
        attacking = true;
        animator.Play("Barb");
        yield return new WaitForSeconds(barbDelay);

        throwSound.Play();

        Projectile barb = Instantiate(barbPrefab, barbSpawn.position, barbSpawn.rotation);

        Vector2 displacement = Player.instance.transform.position - barbSpawn.position;
        float timeToHit = displacement.magnitude / barbSpeed;

        barb.body.velocity = displacement.normalized * barbSpeed + new Vector2(0, 24 * timeToHit);
        barb.body.angularVelocity = barbSpinSpeed;

        yield return new WaitForSeconds(barbDownTime);
        attacking = false;
    }

    public override void Die()
    {
        base.Die();
        gameObject.layer = LayerMask.NameToLayer("Corpse");
        body.sharedMaterial = corpseMat;
        applyHorizontalMovement = false;
        animator.Play("Death");
        StopAllCoroutines();
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(deathLength);
        boomSound.Play();
    }
}
