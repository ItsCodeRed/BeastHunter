using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage;
    public Vector2 knockback;
    public float hitLength;
    public float stun;
    public bool isPoison = false;
    public bool isBig = false;
    public string deathAnimation;
    public float deathLength;

    public Rigidbody2D body;
    private Animator anim;


    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            int modifier = isBig ? Mathf.CeilToInt(damage * GameManager.instance.attackMultiplier * (GameManager.instance.dayNum - 1)) : 0;
            Player.instance.TakeDamage(damage + modifier, Vector2.Scale(knockback, new Vector2(Mathf.Sign(body.velocity.x), 1)), hitLength, stun);
            if (isPoison)
            {
                Player.instance.isPoisoned = true;
            }
        }

        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        anim.Play(deathAnimation);
        body.isKinematic = true;
        yield return new WaitForSeconds(deathLength);
        Destroy(gameObject);
    }
}
