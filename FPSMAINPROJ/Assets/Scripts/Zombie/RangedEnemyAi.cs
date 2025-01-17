using System.Collections;
using UnityEngine;

public class RangedEnemy : EnemyAI
{
    public GameObject projectilePrefab;
    public float fireRate = 1f;
    private float nextFireTime = 0f;

    [SerializeField] Transform launchPoint;
    [SerializeField] GameObject CastPortal1;
    [SerializeField] GameObject CastPortal2;

    [SerializeField] int castAmount;

    float castDelay = 1;
    [SerializeField] float castRange;
    bool canAttack;
  


    [SerializeField] bool HasRangedAttacks;
    
    [SerializeField] float ProjectileSpeed;
    [SerializeField] float ProjectileLifeTime;
    [SerializeField] float ProjectileDamage;
    [SerializeField] float ProjectileFollowTime;
    [SerializeField] ProjectileType Type;
    [SerializeField] ProjectileAblity projectileAblity;
    [SerializeField] float AbilityStrength;
    [SerializeField] float AbilityDuration;

    [SerializeField] float effectDuration;
    [SerializeField] float AoeStrength;
    [SerializeField] float radius;
    [SerializeField] AOETYPE type;


    [SerializeField] Color BulletColor;
    [SerializeField] Material BulletMaterial;
    float LazerSpeed;

    Caster caster;
  
    protected override void Start()
    {
        base.Start();
        agent.stoppingDistance = castRange / 2;
        canAttack = true;
    
        caster = Caster.NormalCaster;
        LazerSpeed = ProjectileSpeed * 2;

    }

    public void ToggleCastPortal()
    {

        if (CastPortal1 != null)
        {
            CastPortal1.SetActive(!CastPortal1.activeSelf);
        }

        if (CastPortal2 != null)
        {
            CastPortal2.SetActive(!CastPortal2.activeSelf);
        }
    }

    protected override void Update()
    {
        base.Update();


        if (Type == ProjectileType.Lazer)
        {
            castAmount = 1;
            ProjectileSpeed = LazerSpeed;
        }


        if (PlayerinAttackRange && canAttack && PlayerInSIte)
        {
            canAttack = false;
            animator.SetTrigger("Shoot");

        }
    }


    public void CastBaseAttack()
    {
            GameObject projectile = Instantiate(projectilePrefab, launchPoint.position, Quaternion.identity);
            Projectile projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript == null)
            {
                projectileScript = projectile.AddComponent<Projectile>();
            }
            if (projectileScript != null)
            {
                projectileScript.SetStats(ProjectileSpeed, ProjectileLifeTime, ProjectileDamage, ProjectileFollowTime, Type, projectileAblity, AbilityStrength, 1f, caster);

                projectileScript.SetColor(BulletColor, BulletMaterial);
                if (Type == ProjectileType.AOE)
                {
                    projectileScript.AoeStats(effectDuration, AoeStrength, radius, type);
                }
                else
                    projectileScript.AoeStats(0, 0, 0, AOETYPE.Damage);
            }
    }




    public void EnableAttack()
    {
        canAttack = true;
    }


    protected override void Die()
    {
        // Additional ranged-specific death logic (if any)
        base.Die();
    }


 


}
