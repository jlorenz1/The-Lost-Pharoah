using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;


public enum ProjectileAblity
{
    Slow = 0,
    Burn = 1,
    Stun = 2,
    Normal = 3,
    Special = 4,
}
public enum ProjectileType
{
    Lazer = 0,
    AOE = 1,
    Ball = 2,
}

public enum Caster
{
    Userkare,
    Sekhmet,
    NormalCaster
}


public class Projectile : MonoBehaviour
{
    //Stats Based On the user
    float speed;
    float lifetime;
    float projectileDamage;
    float followtime;



    [SerializeField] AudioSource ProjectileAudio;

    [SerializeField] AudioClip ZombieProjectileAudio;
    [SerializeField, Range(0f, 1f)] float ZombieProjectileAudioVol;
    [SerializeField] bool SekhmetAttack;
    [SerializeField] bool UserKareBaseAttack;
    [SerializeField] bool UserKareSpecialAttack1;
    [SerializeField] bool UserKareSpecialAttack2;
    Rigidbody rb;
    Transform playerTransform;

    bool followPlayer;
    float OriginalSpeed;
    int OriginalJumpCount;
    Vector3 currentDirection;
    float Nerf;
    GameObject player;
    float NerfTimer;
    int round;
    float StartSpeed;
    float Strength;
    ProjectileAblity projectileAblity;
    ProjectileType projectileType;
    AOETYPE  aoetype;
    Caster caster;
    IDamage PlayerDamage;
    [SerializeField] GameObject aoePrefab;    // Assign your AoE slow effect prefab here
    [SerializeField] GameObject ProjectileBody;
    Transform LaunchPoint;
    [SerializeField] private Color bulletColor;

    float aoeRadius = 5f;    // Radius of the AoE slow effect
    float slowDuration = 5f; // How long the slow effect lasts on the player
    float EffectDuration = 3f; // How long the player stays slowed
    float AOEEffectDuration = 3f;
    float aoeStrength;


    void Start()
    {
        followPlayer = true;
        int projectileLayer = gameObject.layer;
        int zombieLayer = LayerMask.NameToLayer("Zombie");
        Physics.IgnoreLayerCollision(projectileLayer, zombieLayer);
        if (projectileType == ProjectileType.Lazer)
        {
            speed *= 2;
            CreateTracer();
        }
        if (ProjectileAudio != null)
        {
            ProjectileAudio.clip = ZombieProjectileAudio;
            ProjectileAudio.volume = ZombieProjectileAudioVol;
            ProjectileAudio.loop = true; // Enable looping
            ProjectileAudio.Play(); // Start playing the audio
        }


        // Destroy the projectile after a certain amount of time
        Destroy(gameObject, lifetime);



        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false; // Ensure gravity does not affect the projectile
        }

        // Find the player object and initialize playerTransform
        player = gameManager.gameInstance.playerScript.gameObject;
        if (player != null)
        {
            playerTransform = player.transform;
        }

        StartCoroutine(FollowPlayer(followtime));

    }
    void Update()
    {
        round = gameManager.gameInstance.GetGameRound();
        // SetNerfTimer();
        // SetBuffStrength();
        if (followPlayer && player != null)
        {
            // During the first second, follow the player
            currentDirection = (player.transform.position - transform.position).normalized;
        }

        // Move the projectile in the current direction
        if (rb != null)
        {
            rb.velocity = currentDirection * speed;
        }
        else
        {
            transform.Translate(currentDirection * speed * Time.deltaTime);
        }
    }

    IEnumerator FollowPlayer(float seconds)
    {
        // Follow the player for 1 second
        yield return new WaitForSeconds(seconds);

        // Stop following the player and continue in the last known direction
        followPlayer = false;
    }

    void OnDestroy()
    {
        // Stop the audio when the object is destroyed
        if (ProjectileAudio != null)
        {
            ProjectileAudio.Stop();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger || other.CompareTag("Zombie"))
        {
            return;
        }

        else if (other.CompareTag("Player"))
        {
            PlayerDamage = other.GetComponent<IDamage>();
            if (PlayerDamage != null)
            {

                if (caster == Caster.Sekhmet)
                {
                    stun();
                    gameManager.gameInstance.LightGautlening = true;
                }

                else if (caster == Caster.Userkare)
                {
                    if (projectileAblity == ProjectileAblity.Special)
                    {
                        stun();
                        gameManager.gameInstance.BlinkingJab = true;
                    }
                    if (projectileAblity != ProjectileAblity.Special)
                    {
                        PlayerDamage.takeDamage(projectileDamage);
                    }
                }

                if (projectileAblity == ProjectileAblity.Stun || projectileAblity == ProjectileAblity.Slow && projectileType == ProjectileType.AOE) ;
                {
                    stun();
                }

                if (projectileAblity == ProjectileAblity.Slow)
                {
                    slow();
                }

                if (projectileAblity == ProjectileAblity.Burn)
                {
                    burn();
                }

                if (projectileAblity == ProjectileAblity.Normal)
                {
                    PlayerDamage.takeDamage(projectileDamage);
                }

                Destroy(gameObject);
            }
        }
        else if (projectileType == ProjectileType.AOE && !other.CompareTag("Player") && !other.CompareTag("Zombie"))
        {
            CreateAOE(gameObject.transform.position);
            Destroy(gameObject);

        }


    }

    void stun()
    {
        PlayerDamage.takeDamage(projectileDamage);
        PlayerDamage.CutSpeed(slowDuration, 100);
    }

    void slow()
    {
        PlayerDamage.CutSpeed(slowDuration, Strength);
    }

    void burn()
    {
        float TickStrength = Strength / 10;
        float tickrate = EffectDuration / 2 * Strength;
        PlayerDamage.TickDamage(EffectDuration, TickStrength, tickrate);
    }

    void CreateAOE(Vector3 position)
    {
        // Instantiate the AoE prefab at the position passed (the player's position or other)
        GameObject aoe = Instantiate(aoePrefab, position, Quaternion.identity);

        AOEDamage AoeOBject = aoe.GetComponent<AOEDamage>();
        if (AoeOBject != null)
        {
            AoeOBject = aoe.AddComponent<AOEDamage>();
        }

        AoeOBject.SetStats(AOEEffectDuration, aoeStrength, aoeRadius, aoetype);

        // Set the AoE effect to destroy itself after the slow duration ends
        Destroy(aoe, slowDuration);
    }


    public void SetStats(float Speed, float LifeTime, float Damage, float FollowTime, ProjectileType Type, ProjectileAblity projectileAblity, float AblityStrength, float AbilityDuration, Caster User)
    {
        speed = Speed;
        lifetime = LifeTime;
        projectileDamage = Damage;
        followtime = FollowTime;
        projectileType = Type;
        Strength = AblityStrength;
        EffectDuration = AbilityDuration;
        caster = User;
    }




    public void AoeStats(float effectDuration, float AoeStrength, float radius, AOETYPE type)
    {

    aoeRadius = radius;
        AOEEffectDuration = effectDuration;
    aoeStrength = AoeStrength;
        aoetype = type;

    }

    private void CreateTracer()
    {
        if (ProjectileBody != null)
        {
            // Instantiate the tracer
            GameObject tracer = Instantiate(ProjectileBody, transform.position, Quaternion.identity);

            // Configure the tracer
            LineRenderer lineRenderer = tracer.GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                // Set tracer color to match the bullet
                lineRenderer.startColor = bulletColor;
                lineRenderer.endColor = bulletColor;

                // Set tracer length (example)
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, transform.position + transform.forward * 100); // Adjust length as needed
            }

            // Destroy the tracer after a short time if needed
            Destroy(tracer, 0.1f);
        }
    }
}


