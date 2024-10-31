using UnityEngine;

public class BasicEnemyTurret : MonoBehaviour, IHitable
{

    [Space]
    [Header("Basic Enemy Settings")]
    public float Health = 100;
    public float PerShot = 20;
    [Space]
    [Header("Projectile")]
    public GameObject Projectile;
    public Transform ShootPoint;
    public Transform PivotPoint;
    [Space]
    public float EnemySightRange = 10;
    public LayerMask TargetLayerMask;
    [Space]
    public HealthDamageData HealthDamageData;
    [Space]
    public float TimeBetweenShot = 0.2f;
    public float FireForce = 10;

    private float ShotTimer = 0;

    private Transform _target;

    // Update is called once per frame
    void Update()
    {
        ScanForTarget();

        if (_target != null)
        {
            if (ShotTimer <= 0)
            {
                SpawnProjectile();
                ShotTimer = TimeBetweenShot;
            }
            else
            {
                ShotTimer -= Time.deltaTime;
            }

            PivotPoint.LookAt(_target);
        }
        else PivotPoint.rotation = Quaternion.identity;
    }

    private void ScanForTarget()
    {
        _target = null;

        Collider[] colliders = Physics.OverlapSphere(transform.position, EnemySightRange, TargetLayerMask);

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                _target = colliders[i].transform;
            }
        }
    }

    private void SpawnProjectile()
    {
        Projectile projectile = Instantiate(Projectile, ShootPoint.position, ShootPoint.rotation).GetComponent<Projectile>();

        projectile.Fire(FireForce, HealthDamageData);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, EnemySightRange);
    }

    public void Hit(GameObject hitObject, Vector3 hitPoint, Vector3 hitNormal, HealthDamageData healthDamageData)
    {
        Health -= PerShot;

        if (Health <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}