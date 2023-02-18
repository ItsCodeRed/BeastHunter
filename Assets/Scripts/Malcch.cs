using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Malcch : Enemy
{
    [SerializeField] private float flybyMaxTime;
    [SerializeField] private Vector2 flyOffset;
    [SerializeField] private float flybyMinDist;
    [SerializeField] private float flybyMaxChargeLength;
    [SerializeField] private Vector2 flybyOffset;
    [SerializeField] private Vector2 flyRange;
    [SerializeField] private float flybySpeed;
    [SerializeField] private float flyAccel;
    [SerializeField] private float flySpeed;
    [SerializeField] private float changeTargetRange;

    [SerializeField] private float chargeRange = 8;
    [SerializeField] private float chanceOfCharge = 50;
    [SerializeField] private float decisionInterval = 3;

    [SerializeField] private float swipeRange = 3;
    [SerializeField] private HitBox swipeHitbox;
    [SerializeField] private HitBox swipeHitbox2;
    [SerializeField] private float chanceOfSwipe = 50;
    [SerializeField] private float swipeDelay = 0.5f;
    [SerializeField] private float swipeLength = 0.5f;
    [SerializeField] private float swipeDownTime = 0.5f;

    [SerializeField] private float stabRange = 3;
    [SerializeField] private HitBox stabHitbox;
    [SerializeField] private float chanceOfStab = 70;
    [SerializeField] private float stabDelay = 0.5f;
    [SerializeField] private float stabLength = 0.5f;
    [SerializeField] private float stabDownTime = 0.5f;

    [SerializeField] private HitBox diveSpinHitbox;
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

    [SerializeField] AudioSource boomSound;

    private Vector2 targetLocation;

    private float decisionTimer = 0;
    public bool attacking = false;
    private bool applyFlyMovement = true;

    private float currentFlySpeed;

    private void Start()
    {
        currentFlySpeed = flySpeed;
    }

    private void SetTargetLocation()
    {
        targetLocation = (Vector2)Player.instance.transform.position + flyOffset + new Vector2(Random.Range(-flyRange.x, flyRange.x), Random.Range(-flyRange.y, flyRange.y)); ;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        applyHorizontalMovement = false;

        if (isDead) return;

        if (isAgroed)
        {
            if (!attacking && (targetLocation == Vector2.zero || Vector2.Distance(transform.position, targetLocation) < changeTargetRange))
            {
                SetTargetLocation();
            }

            Vector2 displacement = Player.instance.transform.position - transform.position;

            if (displacement.magnitude < stabRange && !attacking)
            {
                StartCoroutine(StabRoutine());
                return;
            }
            if (decisionTimer > 0)
            {
                decisionTimer -= Time.fixedDeltaTime;
            }
            else if (!attacking)
            {
                decisionTimer = decisionInterval;

                float randNum = Random.Range(0f, 100f);
                if (randNum < chanceOfSwipe)
                {
                    StartCoroutine(FlybyRoutine());
                    return;
                }

                //if (displacement.magnitude < flameRange)
                //{
                //    float randNum = Random.Range(0f, 100f);
                //    if (randNum < chanceOfFlame)
                //    {
                //        StartCoroutine(FlameRoutine());
                //        return;
                //    }
                //    else if (randNum < chanceOfFlame + chanceOfFlip)
                //    {
                //        StartCoroutine(FlipRoutine());
                //        return;
                //    }
                //}
                //if (displacement.magnitude < chargeRange)
                //{
                //    bool isCharging = true;

                //    float randNum = Random.Range(0f, 100f);
                //    if (randNum > chanceOfCharge)
                //    {
                //        isCharging = false;
                //    }

                //    horizontalMovement = !isCharging ? 0 : Mathf.Sign(displacement.x);
                //}
            }
        }

        if (!isAgroed)
        {
            targetLocation = Vector2.zero;
        }

        if (!attacking)
        {
            animator.Play("Idle");
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x) * Mathf.Sign(body.velocity.x), transform.localScale.y, 1f);
        }

        if (applyFlyMovement)
        {
            if (targetLocation == Vector2.zero)
            {
                body.velocity -= body.velocity.normalized * (flyAccel * Time.fixedDeltaTime);
            }
            else
            {
                body.velocity += (targetLocation - (Vector2)transform.position).normalized * (flyAccel * Time.fixedDeltaTime);
            }

            if (body.velocity.magnitude > currentFlySpeed)
            {
                body.velocity = body.velocity.normalized * currentFlySpeed;
            }
        }
        else
        {
            body.velocity = Vector2.zero;
        }
    }

    private IEnumerator StabRoutine()
    {
        attacking = true;
        animator.Play("Stab");
        FacePlayer(false);
        targetLocation = Player.instance.transform.position;

        yield return new WaitForSeconds(stabDelay);

        float timer = 0;
        while (timer < swipeLength)
        {
            Attack(stabHitbox);
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        targetLocation = Vector2.zero;

        yield return new WaitForSeconds(stabDownTime);

        attacking = false;
    }

    private IEnumerator FlybyRoutine()
    {
        attacking = true;
        animator.Play("Idle");

        int randomSide = Random.Range(0, 2) == 0 ? -1 : 1;

        float timer = 0;
        while (Vector2.Distance(transform.position, Player.instance.transform.position) < flybyMinDist && timer < flybyMaxChargeLength)
        {
            timer += Time.fixedDeltaTime;

            targetLocation = (Vector2)Player.instance.transform.position + flyOffset + new Vector2(randomSide * flybyMinDist, 0);
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x) * Mathf.Sign(body.velocity.x), transform.localScale.y, 1f);

            yield return new WaitForFixedUpdate();
        }

        currentFlySpeed = flybySpeed;

        timer = 0;
        while (timer < flybyMaxTime)
        {
            timer += Time.fixedDeltaTime;

            targetLocation = (Vector2)Player.instance.transform.position + flybyOffset;
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x) * Mathf.Sign(body.velocity.x), transform.localScale.y, 1f);

            if (Vector2.Distance(transform.position, targetLocation) < swipeRange)
            {
                yield return SwipeRoutine();
                break;
            }

            yield return new WaitForFixedUpdate();
        }

        currentFlySpeed = flySpeed;
        attacking = false;
    }


    private IEnumerator SwipeRoutine()
    {
        animator.Play("Swipe");

        yield return new WaitForSeconds(swipeDelay);

        float swipeTimer = 0;
        while (swipeTimer < swipeLength)
        {
            Attack(swipeHitbox);
            Attack(swipeHitbox2);
            swipeTimer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        yield return new WaitForSeconds(swipeDownTime);
    }

    private IEnumerator FlameRoutine()
    {
        attacking = true;
        animator.Play("FlameBreath");
        FacePlayer();

        yield return new WaitForSeconds(flameDelay);

        float timer = 0;
        while (timer < flameLength)
        {
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        yield return new WaitForSeconds(flameDownTime);

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
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(deathLength);
        boomSound.Play();
    }
}
