using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GreatGruch : Enemy
{
    [SerializeField] private float hitRange = 3;
    [SerializeField] private float chargeRange = 8;
    [SerializeField] private float chanceOfScrem = 20;
    [SerializeField] private float chanceOfCharge = 50;
    [SerializeField] private float spawnDelay = 0.1f;
    [SerializeField] private float screamTime = 0.6f;
    [SerializeField] private Vector2 spawnOffset;
    [SerializeField] private float decisionInterval = 3;

    [SerializeField] private float chanceOfJumpAttack = 0.5f;
    [SerializeField] private float jumpAttackDelay = 0.5f;
    [SerializeField] private Vector2 jumpVector;
    [SerializeField] private float diveSpeed = 15;
    [SerializeField] private float diveDist = 2;
    [SerializeField] private float diveGroundDist = 2;

    [SerializeField] private HitBox stabGroundHitbox;
    [SerializeField] private HitBox slashForwardHitbox;
    [SerializeField] private HitBox slashBackwardHitbox;
    [SerializeField] private float slashDelay = 0.5f;
    [SerializeField] private float slashLength = 0.5f;
    [SerializeField] private float slashDownTime = 0.5f;

    [SerializeField] private HitBox stabHitbox;
    [SerializeField] private float chanceOfStab = 50;
    [SerializeField] private float stabDelay = 0.5f;
    [SerializeField] private float stabLength = 0.5f;
    [SerializeField] private float stabRange = 0.5f;
    [SerializeField] private float stabDownTime = 0.5f;

    [SerializeField] private float chanceOfSpit = 50;
    [SerializeField] private float spitDelay = 0.5f;
    [SerializeField] private float spitDownTime = 0.5f;
    [SerializeField] private float spitSpeed = 15;

    [SerializeField] private Projectile poisonGlob;
    [SerializeField] private Transform globSpawn;

    [SerializeField] private float chanceOfRetreat = 0.5f;
    [SerializeField] private float retreatLength = 0.5f;

    [SerializeField] private float deathLength;

    public Collider2D legCollider;
    public Collider2D playerLegCollider;
    public Transform legHeight;
    public Transform legMinimum;
    public Transform legBottom;

    [SerializeField] AudioSource boomSound;
    [SerializeField] AudioSource scremSound;

    private float decisionTimer = 0;
    private bool attacking = false;

    private MonsterAsset minionMonster;

    private void Start()
    {
        minionMonster = additionalEnemies[0].monsterInstance.monsterAsset;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (isDead) return;

        if (isAgroed)
        {
            Vector2 displacement = Player.instance.transform.position - transform.position;

            if (decisionTimer > 0)
            {
                decisionTimer -= Time.fixedDeltaTime;

                if (displacement.magnitude >= chargeRange)
                {
                    horizontalMovement = Mathf.Sign(displacement.x);
                }
                if (!attacking && displacement.magnitude < hitRange)
                {
                    StartCoroutine(StabRoutine());
                    return;
                }
            }
            else if (!attacking)
            {
                decisionTimer = decisionInterval;

                float randNum = Random.Range(0f, 100f);
                if (displacement.magnitude > chargeRange)
                {
                    if (randNum < chanceOfSpit)
                    {
                        StartCoroutine(SpitRoutine());
                        return;
                    }
                }

                randNum = Random.Range(0f, 100f);
                if (randNum < chanceOfScrem && additionalEnemies[0].isDead)
                {
                    additionalEnemies.Clear();
                    StartCoroutine(ScremRoutine());
                    return;
                }

                randNum = Random.Range(0f, 100f);
                if (randNum < chanceOfJumpAttack)
                {
                    StartCoroutine(JumpSlashRoutine());
                    return;
                }

                if (displacement.magnitude < chargeRange)
                {
                    randNum = Random.Range(0f, 100f);
                    if (randNum < chanceOfRetreat)
                    {
                        StartCoroutine(RetreatRoutine(displacement.x));
                        return;
                    }

                    bool isCharging = true;

                    randNum = Random.Range(0f, 100f);
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
                FaceForward(false);
            }
            else
            {
                animator.Play("Idle");
                FacePlayer(false);
            }
        }
    }

    private IEnumerator RetreatRoutine(float displacement)
    {
        attacking = true;
        animator.Play("WalkBackwards");
        FacePlayer(false);

        applyHorizontalMovement = false;

        float timer = 0;
        while (timer < retreatLength)
        {
            timer += Time.deltaTime;
            body.velocity = new Vector2(movementSpeed * -Mathf.Sign(displacement), body.velocity.y);
            yield return null;
        }

        applyHorizontalMovement = true;

        attacking = false;
    }

    private IEnumerator ScremRoutine()
    {
        attacking = true;
        animator.Play("Screm");
        FacePlayer(false);

        yield return new WaitForSeconds(spawnDelay);

        scremSound.Play();
        Enemy additionalEnemy = Instantiate(minionMonster.monsterPrefab, transform.position + Vector3.Scale(spawnOffset, new Vector2(Mathf.Sign(transform.localScale.x), 1)), Quaternion.identity);
        MonsterManager.instance.allEnemies.Add(additionalEnemy);
        additionalEnemies.Add(additionalEnemy);
        additionalEnemy.monsterInstance = new MonsterInstance(minionMonster);
        additionalEnemy.health = minionMonster.maxHealth;

        yield return new WaitForSeconds(screamTime - spawnDelay);

        attacking = false;
    }

    private IEnumerator StabRoutine()
    {
        attacking = true;
        animator.Play("StabAttack");
        FacePlayer(false);

        yield return new WaitForSeconds(stabDelay);

        transform.position += new Vector3(stabRange * Mathf.Sign(transform.localScale.x), 0, 0);
        float timer = 0;
        while (timer < stabLength)
        {
            timer += Time.fixedDeltaTime;
            Attack(stabHitbox);
            yield return new WaitForFixedUpdate();
        }

        yield return new WaitForSeconds(stabDownTime);

        attacking = false;
    }


    private IEnumerator JumpSlashRoutine()
    {
        attacking = true;
        applyHorizontalMovement = false;
        animator.Play("Jump");
        FacePlayer(false);

        yield return new WaitForSeconds(jumpAttackDelay);

        body.velocity = Vector2.Scale(jumpVector, new Vector2(Mathf.Sign(transform.localScale.x), 1));

        bool hasTriggered = false;
        while (!hasTriggered)
        {
            if (Mathf.Abs(Player.instance.transform.position.x - transform.position.x) < diveDist || (Physics2D.OverlapPoint(transform.position + Vector3.down * diveGroundDist) && body.velocity.y < 0.1f))
            {
                hasTriggered = true;
            }

            yield return null;
        }

        playerLegCollider.enabled = false;
        animator.Play("StabGround");
        applyHorizontalMovement = true;
        body.velocity = Vector2.down * diveSpeed;

        yield return new WaitUntil(() => isGrounded);

        Attack(stabGroundHitbox);

        bool slashForward = (Player.instance.transform.position.x - transform.position.x) * Mathf.Sign(transform.localScale.x) < 0;

        if (slashForward)
        {
            animator.Play("KickForward");
        }
        else
        {
            animator.Play("KickBackward");
        }

        yield return new WaitForSeconds(slashDelay);

        float timer = 0;
        while (timer < stabLength)
        {
            timer += Time.fixedDeltaTime;
            Attack(slashForward ? slashForwardHitbox : slashBackwardHitbox);
            yield return new WaitForFixedUpdate();
        }

        yield return new WaitForSeconds(slashDownTime);

        playerLegCollider.enabled = true;
        attacking = false;
    }

    private IEnumerator SpitRoutine()
    {
        attacking = true;
        animator.Play("PoisonSpit");
        FacePlayer(false);

        yield return new WaitForSeconds(spitDelay);

        Projectile glob = Instantiate(poisonGlob, globSpawn.position, globSpawn.rotation);
        Vector2 displacement = Player.instance.transform.position - globSpawn.position;

        glob.body.velocity = displacement.normalized * spitSpeed;
        glob.transform.right = displacement.normalized;

        yield return new WaitForSeconds(spitDownTime);
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
        legCollider.gameObject.layer = LayerMask.NameToLayer("Corpse");
        playerLegCollider.gameObject.layer = LayerMask.NameToLayer("Corpse");
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
