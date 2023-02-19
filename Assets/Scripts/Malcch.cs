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
    [SerializeField] private HitBox diveSpinHitbox;

    [Header("Lazer")]
    [SerializeField] private float chanceOfLazer = 50;
    [SerializeField] private float lazerDelay = 0.5f;
    [SerializeField] private Lazer lazerPrefab;
    [SerializeField] private Transform lazerOrigin;

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

                randNum = Random.Range(0f, 100f);
                if (randNum < chanceOfLazer)
                {
                    StartCoroutine(LazerRoutine());
                    return;
                }
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
        while (Vector2.Distance(transform.position, Player.instance.transform.position) < flybyMinDist && timer < flybyMaxChargeLength)
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

        Debug.Log("a");

        currentFlySpeed = flybySpeed;
        int randomSide = Random.Range(0, 2) == 0 ? -1 : 1;

        float timer = 0;
        while (Vector2.Distance(transform.position, Player.instance.transform.position) < flybyMinDist && timer < flybyMaxChargeLength)
        {
            timer += Time.fixedDeltaTime;

            targetLocation = (Vector2)Player.instance.transform.position + flyOffset + new Vector2(randomSide * flybyMinDist, 0);
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x) * Mathf.Sign(body.velocity.x), transform.localScale.y, 1f);

            yield return new WaitForFixedUpdate();
        }

        randomSide *= -1;
        targetLocation = Vector2.zero;

        Debug.Log("b");
        animator.Play("BloodLazer");

        yield return new WaitForSeconds(lazerDelay);

        animator.Play("LazerLoop");

        Lazer lazer = Instantiate(lazerPrefab, lazerOrigin);
        targetLocation = (Vector2)Player.instance.transform.position + flyOffset + new Vector2(randomSide * flybyMinDist, 0);

        while (Vector2.Distance(transform.position, targetLocation) < changeTargetRange)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x) * Mathf.Sign(body.velocity.x), transform.localScale.y, 1f);

            yield return new WaitForFixedUpdate();
        }

        Destroy(lazer.gameObject);

        currentFlySpeed = flySpeed;
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
