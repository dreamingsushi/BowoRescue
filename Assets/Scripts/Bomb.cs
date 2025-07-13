using UnityEngine;
using System.Collections;

public class Bomb : MonoBehaviour
{
    public float explosionDelay = 5f;
    public float damageAmount = 30f;
    public float explosionRadius = 5f;
    public GameObject explosionEffect;

    public MeshRenderer meshRenderer; // Assign in Inspector
    public Color flashColor = Color.red;
    public Color baseColor = Color.white;
    public float flashInterval = 0.3f;
    public GameObject indicatorPrefab;
    private GameObject spawnedIndicator;


    private void Start()
    {
        StartCoroutine(FlashEffect());
        Invoke(nameof(Explode), explosionDelay);

        if (indicatorPrefab != null)
        {
            // Spawn the indicator at the bomb’s position, flat on ground
            Quaternion flatRotation = Quaternion.Euler(-90f, 0f, 0f);
            spawnedIndicator = Instantiate(indicatorPrefab, transform.position, flatRotation);

            // Make sure it's not parented to the bomb (so it doesn't rotate)
            spawnedIndicator.transform.SetParent(null);
        }
    }
    private void Update()
    {
        if (spawnedIndicator != null)
        {
            Vector3 bombPos = transform.position;
            bombPos.y = -1.38f; // 👈 Lock Y position to -1.38

            spawnedIndicator.transform.position = bombPos;
            spawnedIndicator.transform.rotation = Quaternion.Euler(-90f, 0f, 0f); // keep flat
        }
    }

    private IEnumerator FlashEffect()
    {
        float timer = 0f;
        bool useFlashColor = true;

        while (timer < explosionDelay)
        {
            meshRenderer.material.color = useFlashColor ? flashColor : baseColor;
            useFlashColor = !useFlashColor;
            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;
        }
    }

    private void Explode()
    {
        // Optional VFX
        if (explosionEffect != null)
        {
            GameObject vfx = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(vfx, 2f);
        }

        // Damage nearby objects
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out IDamageable target))
            {
                target.TakeDamage(damageAmount);
            }

            // Check if it's the Boss and break shield if in Phase2
            if (hit.TryGetComponent<Boss>(out Boss boss))
            {
                if (boss.currentPhase == Boss.BossPhase.Phase2)
                {
                    boss.BreakShieldFromBomb();

                }
            }
        }
        AudioManager.Instance.PlaySFX("Explosion");
        CameraShakeManager.Instance?.Shake(2.0f); 
        HitStopManager.Instance?.DoHitStop(0.1f);

        if (spawnedIndicator != null)
            Destroy(spawnedIndicator);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
