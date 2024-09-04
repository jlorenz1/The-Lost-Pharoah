using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class Projectile : MonoBehaviour
{
   [SerializeField] float speed;
   [SerializeField] float lifetime;
   [SerializeField] bool IsNormal;
   [SerializeField] bool IsSlow;
   [SerializeField] bool IsGround;
   [SerializeField] bool IsBossAttack;
   [SerializeField] int projectileDamage;
    [SerializeField] float followtime;
    [SerializeField] AudioSource ProjectileAudio;

    [SerializeField] AudioClip ZombieProjectileAudio;
    [SerializeField, Range(0f, 1f)] float ZombieProjectileAudioVol;

    Rigidbody rb;
    Transform playerTransform;

    bool followPlayer;
    float OriginalSpeed;
    int OriginalJumpCount;
    Vector3 currentDirection;
    float Nerf;
    GameObject player;
    int NerfTimer;
    int round;
    void Start()
    {
        followPlayer = true;
        int projectileLayer = gameObject.layer;
        int zombieLayer = LayerMask.NameToLayer("Zombie");
        Physics.IgnoreLayerCollision(projectileLayer, zombieLayer);

        if (ProjectileAudio != null)
        {
            ProjectileAudio.clip = ZombieProjectileAudio;
            ProjectileAudio.volume = ZombieProjectileAudioVol;
            ProjectileAudio.loop = true; // Enable looping
            ProjectileAudio.Play(); // Start playing the audio
        }

        OriginalSpeed = gameManager.gameInstance.playerScript.GetSpeed();
        OriginalJumpCount = gameManager.gameInstance.playerScript.GetJumpCount();
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
        SetNerfTimer();
        SetBuffStrength();
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

        HandleNonBossAttack(other);
    
   
}

void HandleNonBossAttack(Collider other)
{
    if (!other.CompareTag("Player"))
    {
        Destroy(gameObject);
        return;
    }
        gameManager.gameInstance.playerScript.takeDamage(projectileDamage);
    if (IsNormal)
    {
        Destroy(gameObject);
    }
    else
    {
        StickToPlayer(other);
    }
}

void StickToPlayer(Collider other)
{
    player = other.gameObject;
    transform.parent = player.transform;
        ProjectileAudio.Stop();
        ApplyDebufs();
    StartCoroutine(StatReset());
}



    void ApplyDebufs()
    {
        if(IsSlow)
        {
            if (OriginalSpeed / Nerf >= 1)
            {
                gameManager.gameInstance.playerScript.SetSpeed(OriginalSpeed / Nerf);
            }
            else
                gameManager.gameInstance.playerScript.SetSpeed(1);
        }

        if (IsGround)
        {
            gameManager.gameInstance.playerScript.SetJumpCount(0);
        }
       
    }

    IEnumerator StatReset()
    {

        yield return new WaitForSeconds(NerfTimer);

        gameManager.gameInstance.playerScript.SetSpeed(OriginalSpeed);
        gameManager.gameInstance.playerScript.SetJumpCount(OriginalJumpCount);
    }

    public void SetBuffStrength()
    {
        Nerf = round * 2;
    }

    public void SetNerfTimer()
    {
        if (round * 3 >= 10)
        {
            NerfTimer = round * 3;
        }
        else if(round * 3 >= 60)
        {
            NerfTimer = 60;
        }
        else if(round * 3 < 10)
        {
            NerfTimer = 10;
        }

    }

   
}
