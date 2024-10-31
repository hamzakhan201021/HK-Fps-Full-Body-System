using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{

    [Header("Settings")]
    [SerializeField] private float _maxDestructionTime = 5f;

    private Rigidbody rb;
    private HealthDamageData _healthDamageData;

    public void Fire(float fireForce, HealthDamageData healthDamageData)
    {
        rb = GetComponent<Rigidbody>();

        rb.AddForce(transform.forward * fireForce, ForceMode.Impulse);

        _healthDamageData = healthDamageData;

        Destroy(gameObject, _maxDestructionTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // get the IHitable interface reference.
        IHitable iHitable = collision.transform.GetComponent<IHitable>();

        // check if ihitable is not null.
        if (iHitable != null) iHitable.Hit(collision.transform.gameObject, collision.GetContact(0).point, collision.GetContact(0).normal, _healthDamageData);

        // destroy.
        Destroy(gameObject);
    }
}
