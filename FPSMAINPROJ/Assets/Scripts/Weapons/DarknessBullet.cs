using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarknessBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 10f; // Speed of the bullet
    [SerializeField] private float destroyTime = 5f; // Time before the bullet is destroyed
    [SerializeField] private int damage = 10; // Damage dealt by the bullet
    [SerializeField] private float maxDist;
    [SerializeField] private float blindDuration;
    [SerializeField] private LayerMask damageableLayer;

    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = transform.forward * speed;
        }

        Destroy(gameObject, destroyTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger || other.CompareTag("Weapon") || other.CompareTag("Player"))
        {
            return;
        }

        IEnemyDamage damageable = other.GetComponentInParent<IEnemyDamage>();
        if(damageable != null)
        {
            //damageable.takeDamage(damage);
            damageable.Blind(blindDuration);
            
        }

        Destroy(gameObject);
    }
}