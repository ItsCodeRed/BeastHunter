using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spear : MonoBehaviour
{
    public Animator spearAnimator;

    public Transform hitParticleSpawn;
    public GameObject hitParticlePrefab;

    public LayerMask hitMask;

    public float spearStabCooldown = 0.4f;
    public float spearStabHitDelay = 0.2f;
    public float spearStabHitLength = 0.1f;
    public int spearStabStamina = 2;

    public float spearSlashCooldown = 0.4f;
    public float spearSlashHitDelay = 0.2f;
    public float spearSlashHitLength = 0.1f;
    public int spearSlashStamina = 1;

    public HitBox spearStabHitBox;
    public HitBox spearSlashHitBox;

    private float spearStabTimer = 0;
    private float spearSlashTimer = 0;

    public AudioSource spearSound;

    void Update()
    {
        transform.parent.right = -Mathf.Sign(transform.parent.parent.localScale.x) * ((Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.parent.position)).normalized;
        transform.parent.localScale = new Vector3(transform.parent.localScale.x, Mathf.Abs(transform.parent.localScale.y) * transform.parent.up.y > 0 ? 1 : -1, 1);

        if (spearSlashTimer > 0)
        {
            spearSlashTimer -= Time.deltaTime;
        }
        else if (Input.GetMouseButtonDown(0) && spearStabTimer <= 0 && Player.instance.stamina >= spearSlashStamina)
        {
            spearSlashTimer = spearSlashCooldown;
            Player.instance.ChangeStamina(-spearSlashStamina);

            StartCoroutine(Slash());
        }

        if (spearStabTimer > 0)
        {
            spearStabTimer -= Time.deltaTime;
        }
        else if (Input.GetMouseButtonDown(1) && spearSlashTimer <= 0 && Player.instance.stamina >= spearStabStamina)
        {
            spearStabTimer = spearStabCooldown;
            Player.instance.ChangeStamina(-spearStabStamina);

            StartCoroutine(Stab());
        }

        Vector3 adjustedDisplacement = spearStabHitBox.displacement.x * transform.lossyScale.x * transform.right + spearStabHitBox.displacement.y * transform.lossyScale.y * transform.up;
        Debug.DrawLine(transform.position + adjustedDisplacement + spearStabHitBox.size.x /2 * transform.right + spearStabHitBox.size.y / 2 * transform.up, transform.position + adjustedDisplacement - spearStabHitBox.size.x / 2 * transform.right + spearStabHitBox.size.y / 2 * transform.up);
        Debug.DrawLine(transform.position + adjustedDisplacement + spearStabHitBox.size.x / 2 * transform.right + spearStabHitBox.size.y / 2 * transform.up, transform.position + adjustedDisplacement + spearStabHitBox.size.x / 2 * transform.right - spearStabHitBox.size.y / 2 * transform.up);
        Debug.DrawLine(transform.position + adjustedDisplacement + spearStabHitBox.size.x / 2 * transform.right - spearStabHitBox.size.y / 2 * transform.up, transform.position + adjustedDisplacement - spearStabHitBox.size.x / 2 * transform.right - spearStabHitBox.size.y / 2 * transform.up);
        Debug.DrawLine(transform.position + adjustedDisplacement - spearStabHitBox.size.x / 2 * transform.right + spearStabHitBox.size.y / 2 * transform.up, transform.position + adjustedDisplacement - spearStabHitBox.size.x / 2 * transform.right - spearStabHitBox.size.y / 2 * transform.up);
    }

    public IEnumerator Stab()
    {
        spearAnimator.Play("Stab");
        yield return new WaitForSeconds(spearStabHitDelay);

        float timer = 0;
        while (timer < spearStabHitLength)
        {
            timer += Time.deltaTime;
            Attack(spearStabHitBox);
            yield return null;
        }
    }

    public IEnumerator Slash()
    {
        spearAnimator.Play("Slash");
        yield return new WaitForSeconds(spearSlashHitDelay);

        float timer = 0;
        while (timer < spearSlashHitLength)
        {
            timer += Time.deltaTime;
            Attack(spearSlashHitBox);
            yield return null;
        }
    }

    public void Attack(HitBox hitbox)
    {
        Vector3 adjustedDisplacement = hitbox.displacement.x * transform.lossyScale.x * transform.right + hitbox.displacement.y * transform.lossyScale.y * transform.up;
        Collider2D[] cols = Physics2D.OverlapBoxAll(transform.position + adjustedDisplacement, hitbox.size, hitbox.angle - Vector2.SignedAngle(transform.right * -Mathf.Sign(transform.lossyScale.x), Vector2.right), hitMask);

        foreach (Collider2D col in cols)
        {
            if (col.gameObject.CompareTag("Enemy"))
            {
                Vector2 facingVector = new Vector2(Mathf.Sign((col.transform.position - transform.parent.parent.position).x), 1);

                Enemy enemy = col.attachedRigidbody.GetComponent<Enemy>();
                if (enemy.Hit(hitbox.damage, Vector3.Scale(hitbox.knockback, facingVector), hitbox.stun, hitbox.hitLength))
                {
                    Instantiate(hitParticlePrefab, hitParticleSpawn.position, hitParticleSpawn.rotation);
                    spearSound.Play();
                }
            }
        }
    }
}
