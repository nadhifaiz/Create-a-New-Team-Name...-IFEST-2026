using UnityEngine;

// Dipakai ulang sama SEMUA projectile (lightning, fireball, shadow bolt) —
// gak perlu bikin controller terpisah per karakter.
// Gerak ke target pakai pola yang sama kayak dash di FighterBase (MoveTowards +
// distance-check), jadi gak butuh Collider/Physics2D sama sekali.
public class ProjectileController : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float hitDistance = 0.2f;
    [SerializeField] private float lifeTime = 5f; // failsafe kalau target ilang/meleset jauh

    private Transform target;
    private int damage;

    public void SetTarget(Transform target, int damage)
    {
        this.target = target;
        this.damage = damage;

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, target.position) < hitDistance)
        {
            HitTarget();
        }
    }

    private void HitTarget()
    {
        FighterBase targetFighter = target.GetComponent<FighterBase>();
        if (targetFighter != null)
        {
            targetFighter.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}