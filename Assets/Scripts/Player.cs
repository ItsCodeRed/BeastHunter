using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Player : MonoBehaviour
{
    public static Player instance = null;

    public PlayerMovement movement;
    public Animator animator;

    public GameObject spear;

    public int maxHealth = 50;
    public int health = 50;

    public int maxStamina = 15;
    public int stamina = 15;

    public int staminaIncreaseAmount;
    public float staminaIncreaseInterval;

    private float staminaTimer = 0;
    private float noHitTimer = 0;
    private float poisonTimer = 0;
    public bool noKnockback = false;

    public float healDelay;
    public int healAmount;
    public float antidoteDelay;
    public float antidoteDownTime;
    public float poisonSpeed;
    public int poisonAmount;
    public int meatStaminaAmount;
    public float healSpeed;
    public float healDownTime;
    public float herbDelay;
    public int deathVillageFoodLoss;
    public float deathResourceLoss;

    public bool healing = false;
    public bool herbing = false;

    public AudioSource hurtSound;
    public AudioSource healSound;
    public AudioSource jumpSound;
    public AudioSource rollSound;

    private IEnumerator herbRoutine = null;

    private bool isDead = false;
    public bool isPoisoned = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (noHitTimer > 0)
        {
            noHitTimer -= Time.deltaTime;
        }

        if (staminaTimer > 0)
        {
            staminaTimer -= Time.deltaTime;
        }
        else
        {
            staminaTimer = staminaIncreaseInterval;
            ChangeStamina(staminaIncreaseAmount);
        }

        if (isPoisoned && health > 1)
        {
            if (poisonTimer < 0)
            {
                health -= poisonAmount;
                poisonTimer = poisonSpeed;
            }

            poisonTimer -= Time.deltaTime;
        }

        if (!movement.isRolling && !healing && !herbing)
        {
            if (!movement.isGrounded)
            {
                animator.Play(movement.body.velocity.y > 0 ? "Ascend" : "Descend");
            }
            else if (movement.isMoving)
            {
                animator.Play("Walk");
            }
            else
            {
                animator.Play("Idle");
            }
        }

        if (movement.isRolling)
        {
            StopCoroutine(nameof(HealRoutine));
            StopCoroutine(nameof(AntidoteRoutine));
            if (herbRoutine != null)
            {
                StopCoroutine(herbRoutine);
            }
            healSound.Stop();
            if (healing || herbing)
            {
                spear.SetActive(true);
                healing = false;
                herbing = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            switch (Inventory.instance.selectedItem)
            {
                case InventoryItems.potion:
                    if (GameManager.instance.potionAmount > 0 && health < maxHealth && !healing)
                    {
                        StartCoroutine(nameof(HealRoutine));
                    }
                    break;
                case InventoryItems.antidote:
                    if (GameManager.instance.antidoteAmount > 0 && !healing)
                    {
                        StartCoroutine(nameof(AntidoteRoutine));
                    }
                    break;
                case InventoryItems.meat:
                    if (GameManager.instance.meatAmount > 0)
                    {
                        maxStamina += meatStaminaAmount;
                        stamina = maxStamina;
                        GameManager.instance.ChangeMeat(-1);
                    }
                    break;
                default:
                    break;
            }
        }
    }

    private IEnumerator HealRoutine()
    {
        movement.body.velocity = Vector2.zero;
        animator.Play("Heal");
        healSound.Play();
        GameManager.instance.ChangePotion(-1);
        healing = true;
        spear.SetActive(false);

        if (healing) yield return new WaitForSeconds(healDelay);

        for (int i = 0; i < healAmount; i++)
        {
            if (!healing) break;

            health = Mathf.Clamp(health + 1, 0, maxHealth);

            yield return new WaitForSeconds(healSpeed);
        }

        if (healing) yield return new WaitForSeconds(healDownTime);

        spear.SetActive(true);
        healing = false;
    }

    private IEnumerator AntidoteRoutine()
    {
        movement.body.velocity = Vector2.zero;
        animator.Play("Antidote");
        healing = true;
        spear.SetActive(false);

        if (healing) yield return new WaitForSeconds(antidoteDelay);

        isPoisoned = false;
        GameManager.instance.ChangeAntidote(-1);

        if (healing) yield return new WaitForSeconds(antidoteDownTime);

        spear.SetActive(true);
        healing = false;
    }

    public void CollectHerb(GameObject herb)
    {
        if (!herbing)
        {
            herbRoutine = CollectHerbRoutine(herb);
            StartCoroutine(herbRoutine);
        }
    }

    private IEnumerator CollectHerbRoutine(GameObject herb)
    {
        movement.body.velocity = Vector2.zero;
        animator.Play("CollectHerb");
        herbing = true;
        noKnockback = true;
        spear.SetActive(false);

        yield return new WaitForSeconds(herbDelay);

        GameManager.instance.ChangePotion(1);

        noKnockback = false;
        spear.SetActive(true);
        Destroy(herb);
        herbing = false;
    }

    public void ChangeStamina(int value)
    {
        stamina = Mathf.Clamp(stamina + value, 0, maxStamina);
    }

    public bool TakeDamage(int damage, Vector2 knockback, float hitLength, float stun)
    {
        if (noHitTimer <= 0)
        {
            if (herbRoutine != null)
            {
                StopCoroutine(herbRoutine);
            }

            herbing = false;
            healing = false;
            noKnockback = false;
            healSound.Stop();
            spear.SetActive(true);

            health -= damage;
            noHitTimer = hitLength;
            movement.Stun(stun);
            if (!noKnockback) movement.Knockback(knockback);

            hurtSound.Play();

            if (health <= 0 && !isDead)
            {
                isDead = true;
                GameManager.instance.ChangeVillageFood(-deathVillageFoodLoss);
                GameManager.instance.ChangeMeat(-Mathf.CeilToInt(GameManager.instance.meatAmount / deathResourceLoss));
                GameManager.instance.ChangePotion(-Mathf.CeilToInt(GameManager.instance.potionAmount / deathResourceLoss));
                UIManager.instance.DayFinishScreen("You have failed to slay the beast. The village sent a team of medics to save you, at the loss of village resources. The beast ate half of your goods.");
            }
            return true;
        }

        return false;
    }
}
