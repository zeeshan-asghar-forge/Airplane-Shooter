using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PoolManager))]
public class Shooter : MonoBehaviour
{
    public float delay = 1f;           // Time between shots
    public float speed = 10f;            // Bullet speed
    public Transform customParent;       // Optional parent
    public float bulletLifetime = 3f;    // Auto-return time

    private float nextFireTime;
    private PoolManager poolManager;

    void Start()
    {
        Random.InitState(System.DateTime.Now.Millisecond);
        poolManager = GetComponent<PoolManager>();
        nextFireTime = Time.time + delay;
    }

    public bool Fire()
    {
        if (Time.time < nextFireTime) return false;

        GameObject newBullet = poolManager.GetRandomObject();
        if (newBullet == null) return false;

        newBullet.tag = "Bullet";

        // Ignore collisions with plane
        if (transform.parent != null)
        {
            Collider planeCollider = transform.parent.GetComponent<Collider>();
            Collider bulletCollider = newBullet.GetComponent<Collider>();
            if (planeCollider && bulletCollider)
                Physics.IgnoreCollision(bulletCollider, planeCollider, true);
        }

        // Set initial position and rotation
        newBullet.transform.position = transform.position;
        newBullet.transform.rotation = transform.rotation;

        // Parent bullets safely for organization
        if (customParent)
            newBullet.transform.parent = customParent;

        newBullet.SetActive(true);

        // ✅ Kinematic Rigidbody to prevent physics feedback
        Rigidbody rb = newBullet.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // ✅ Move bullet forward over time using a simple coroutine
        newBullet.GetComponent<MonoBehaviour>().StartCoroutine(MoveBulletForward(newBullet, speed, bulletLifetime));

        // Auto-return bullet after lifetime (PoolManager)
        poolManager.AutoReturn(newBullet, bulletLifetime);

        nextFireTime = Time.time + delay;
        return true;
    }

    // Coroutine to move bullets forward
    private IEnumerator MoveBulletForward(GameObject bullet, float speed, float duration)
    {
        float timer = 0f;
        while (timer < duration && bullet.activeInHierarchy)
        {
            bullet.transform.position += bullet.transform.forward * speed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }
    }


    public void SetUpgradeLevel(int level)
    {
        delay = delay / (level + 1);
        nextFireTime = 0;
    }

}
