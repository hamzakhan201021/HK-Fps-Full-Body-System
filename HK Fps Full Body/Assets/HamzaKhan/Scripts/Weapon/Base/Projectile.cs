using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{

    [Header("Settings")]
    [SerializeField] private float _maxDestructionTime = 5f;

    private Rigidbody rb;

    public void Fire(float fireForce)
    {
        rb = GetComponent<Rigidbody>();

        rb.AddForce(transform.forward * fireForce, ForceMode.Impulse);

        Destroy(gameObject, _maxDestructionTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // get the IHitable interface reference.
        IHitable iHitable = collision.transform.GetComponent<IHitable>();

        // check if ihitable is not null.
        if (iHitable != null) iHitable.Hit(collision.transform.gameObject, collision.GetContact(0).point, collision.GetContact(0).normal);

        // destroy.
        Destroy(gameObject);
    }
}
