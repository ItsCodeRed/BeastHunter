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

    [SerializeField] private float decisionInterval = 3;

    [Header("Swipe")]
    [SerializeField] private float swipeRange = 3;
    [SerializeField] private HitBox swipeHitbox;
    [SerializeField] private HitBox swipeHitbox2;
    [SerializeField] private float chanceOfSwipe = 50;
    [SerializeField] private float swipeDelay = 0.5f;
    [SerializeField] private float swipeLength = 0.5f;
    [SerializeField] private float swipeDownTime = 0.5f;

    [Header("Stab")]
    [SerializeField] private float stabRange = 3;
    [SerializeField] private HitBox stabHitbox;
    [SerializeField] private float stabDelay = 0.5f;
    [SerializeField] private float stabLength = 0.5f;
    [SerializeField] private float stabDownTime = 0.5f;

    [Header("Dive Spin")]
    [SerializeField] private float chanceOfDiveSpin = 50;
    [SerializeField] private HitBox diveSpinHitbox;
    [SerializeField] private HitBox diveSpinHitbox2;
    [SerializeField] private float diveSpinMinDist;
    [SerializeField] private float diveSpinMaxChargeLength;
    [SerializeField] private float diveSpinStartSpeed;
    [SerializeField] private float diveSpinEndSpeed;
    [SerializeField] private float diveSpinChargeDelay = 0.5f;
    [SerializeField] private float diveSpinAttackDelay = 0.5f;
    [SerializeField] private float diveSpinAttackDelay2 = 0.5f;
    [SerializeField] private float diveSpinLength = 0.5f;
    [SerializeField] private float diveSpinDownTime = 0.5f;

    [Header("Lazer")]
    [SerializeField] private float chanceOfLazer = 50;
    [SerializeField] private float lazerDelay = 0.5f;
    [SerializeField] private float maxLazerLength = 5f;
    [SerializeField] private Lazer lazerPrefab;
    [SerializeField] private Transform lazerOrigin;

    [SerializeField] private float tiredLength = 7;
    [SerializeField] private float deathLength;

    [Header("Hitboxes")]
    [SerializeField] private GameObject mainHitbox;
    [SerializeField] private GameObject landingHitbox;
    [SerializeField] private GameObject tiredHitbox;
    [SerializeField] private GameObject bellyHitbox;
    [SerializeField] private GameObject diveHitbox;

    [SerializeField] AudioSource boomSound;

    private Vector2 targetLocation;

    private float decisionTimer = 0;
    public bool attacking = false;
    private bool applyFlyMovement = true;
    private bool isFlying = true;
    private Lazer lazerInstance;

    private bool hitSomething = false;

    public int exhaustionPoints = 0;

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
                    exhaustionPoints += 2;
                    StartCoroutine(FlybyRoutine());
                    return;
                }

                if (randNum < chanceOfSwipe + chanceOfLazer)
                {
                    exhaustionPoints += 2;
                    StartCoroutine(LazerRoutine());
                    return;
                }

                if (randNum < chanceOfSwipe + chanceOfLazer + chanceOfDiveSpin)
                {
                    exhaustionPoints += 2;
                    StartCoroutine(DiveSpinRoutine());
                    return;
                }

                exhaustionPoints = Mathf.Max(exhaustionPoints - 3, 0);
            }

            if (displacement.magnitude < stabRange && !attacking)
            {
                exhaustionPoints += 2;
                StartCoroutine(StabRoutine());
                return;
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

        if (!isFlying) return;

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

    public override void OnHit()
    {
        exhaustionPoints++;

        if (exhaustionPoints >= 20)
        {
            StopAllCoroutines();
            StartCoroutine(TiredRoutine());
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
        while (timer < stabLength)
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

        currentFlySpeed = flybySpeed;
        int randomSide = Random.Range(0, 2) == 0 ? -1 : 1;

        float timer = 0;
        while (Vector2.Distance(transform.position, Player.instance.transform.position + (Vector3)flyOffset) < flybyMinDist && timer < flybyMaxChargeLength)
        {
            timer += Time.fixedDeltaTime;

            targetLocation = (Vector2)Player.instance.transform.position + flyOffset + new Vector2(randomSide * flybyMinDist, 0);
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x) * Mathf.Sign(body.velocity.x), transform.localScale.y, 1f);

            yield return new WaitForFixedUpdate();
        }

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

    private IEnumerator DiveSpinRoutine()
    {
        attacking = true;
        animator.Play("Idle");

        currentFlySpeed = flybySpeed;
        int randomSide = Random.Range(0, 2) == 0 ? -1 : 1;

        float timer = 0;
        while (Vector2.Distance(transform.position, Player.instance.transform.position + (Vector3)flyOffset) < diveSpinMinDist && timer < diveSpinMaxChargeLength)
        {
            timer += Time.fixedDeltaTime;

            targetLocation = (Vector2)Player.instance.transform.position + flyOffset + new Vector2(randomSide * diveSpinMinDist, 0);
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x) * Mathf.Sign(body.velocity.x), transform.localScale.y, 1f);

            yield return new WaitForFixedUpdate();
        }

        animator.Play("DiveSpin");

        targetLocation = Vector2.zero;
        transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x) * Mathf.Sign(Player.instance.transform.position.x - transform.position.x), transform.localScale.y, 1f);

        yield return new WaitForSeconds(diveSpinChargeDelay);

        mainHitbox.SetActive(false);
        diveHitbox.SetActive(true);
        hitSomething = false;
        currentFlySpeed = diveSpinStartSpeed;
        targetLocation = (Vector2)Player.instance.transform.position;
        targetLocation.y = -7;
        body.velocity = (targetLocation - (Vector2)transform.position).normalized * diveSpinStartSpeed;

        yield return new WaitUntil(() => hitSomething);

        animator.Play("SpinOnGround");

        timer = 0;
        while (timer < diveSpinAttackDelay + diveSpinLength)
        {
            timer += Time.fixedDeltaTime;

            body.gravityScale = 1;
            targetLocation = Vector2.right * Mathf.Sign(targetLocation.x - transform.position.x) * 999;

            float ratio = timer / (diveSpinAttackDelay + diveSpinLength);
            currentFlySpeed = diveSpinStartSpeed * (1 - ratio) + diveSpinEndSpeed * ratio;

            if (timer > diveSpinAttackDelay)
            {
                Attack(timer > diveSpinAttackDelay2 ? diveSpinHitbox2 : diveSpinHitbox);
            }

            yield return new WaitForFixedUpdate();
        }

        diveHitbox.SetActive(false);
        mainHitbox.SetActive(true);
        body.gravityScale = 0;
        currentFlySpeed = flySpeed;
        targetLocation = Vector2.zero;
        yield return new WaitForSeconds(diveSpinDownTime);

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

    private IEnumerator LazerRoutine()
    {
        attacking = true;
        animator.Play("Idle");

        currentFlySpeed = flybySpeed;
        int randomSide = Random.Range(0, 2) == 0 ? -1 : 1;

        float timer = 0;
        while (Vector2.Distance(transform.position, Player.instance.transform.position + (Vector3)flyOffset) < flybyMinDist && timer < flybyMaxChargeLength)
        {
            timer += Time.fixedDeltaTime;

            targetLocation = (Vector2)Player.instance.transform.position + flyOffset + new Vector2(randomSide * flybyMinDist, 0);
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x) * Mathf.Sign(body.velocity.x), transform.localScale.y, 1f);

            yield return new WaitForFixedUpdate();
        }
        randomSide = (int)Mathf.Sign(transform.position.x - Player.instance.transform.position.x);

        targetLocation = Vector2.zero;

        animator.Play("BloodLazer");

        yield return new WaitForSeconds(lazerDelay);

        animator.Play("LazerLoop");

        lazerInstance = Instantiate(lazerPrefab, lazerOrigin);
        Vector2 pos = (Vector2)Player.instance.transform.position + flyOffset + new Vector2(-randomSide * flybyMinDist, 0);
        float timer2 = 0;

        while (Vector2.Distance(transform.position, pos) > changeTargetRange && timer2 < maxLazerLength)
        {
            timer2 += Time.fixedDeltaTime;

            targetLocation = pos;
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x) * Mathf.Sign(body.velocity.x), transform.localScale.y, 1f);

            yield return new WaitForFixedUpdate();
        }

        Destroy(lazerInstance.gameObject);

        currentFlySpeed = flySpeed;
        attacking = false;
    }

    private IEnumerator FallDownRoutine()
    {
        isFlying = false;
        horizontalMovement = 0;
        exhaustionPoints = 0;
        applyHorizontalMovement = true;
        mainHitbox.SetActive(false);
        landingHitbox.SetActive(true);
        bellyHitbox.SetActive(false);
        diveHitbox.SetActive(false);
        if (lazerInstance != null)
        {
            Destroy(lazerInstance.gameObject);
        }
        body.gravityScale = 1;

        animator.Play("Fall");

        while (!isGrounded)
        {
            body.velocity = new Vector2(0, Mathf.Min(body.velocity.y, 0));

            yield return new WaitForFixedUpdate();
        }

        yield return new WaitUntil(() => isGrounded);
        tiredHitbox.SetActive(true);
    }

    private IEnumerator TiredRoutine()
    {
        attacking = true;
        currentFlySpeed = flySpeed;
        yield return FallDownRoutine();

        bellyHitbox.SetActive(true);
        animator.Play("Pant");

        Vector2 pos = transform.position;
        float timer = 0;
        while (timer < tiredLength)
        {
            timer += Time.fixedDeltaTime;

            body.velocity = new Vector2(0, Mathf.Min(body.velocity.y, 0));
            if (isGrounded)
            {
                transform.position = pos;
            }
            else
            {
                pos = transform.position;
            }

            exhaustionPoints = 0;

            yield return new WaitForFixedUpdate();
        }

        landingHitbox.SetActive(false);
        mainHitbox.SetActive(true);
        tiredHitbox.SetActive(false);
        bellyHitbox.SetActive(false);
        exhaustionPoints = 0;
        body.gravityScale = 0;
        attacking = false;
        applyHorizontalMovement = false;
        isFlying = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        hitSomething = true;
    }

    public override void Die()
    {
        base.Die();
        tiredHitbox.layer = LayerMask.NameToLayer("Corpse");
        body.sharedMaterial = corpseMat;
        StopAllCoroutines();
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        yield return FallDownRoutine();
        body.velocity = Vector2.zero;
        boomSound.Play();
        animator.Play("Dead");
    }
}
