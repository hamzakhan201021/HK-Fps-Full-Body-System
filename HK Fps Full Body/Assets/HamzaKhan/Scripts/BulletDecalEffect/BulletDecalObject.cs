using UnityEngine;

public class BulletDecalObject : MonoBehaviour, IHitable
{

    // Decal Prefab
    [Space]
    [Header("Bullet Effect Prefab")]
    [SerializeField] private GameObject _decalPrefab;

    [SerializeField] private Vector3 _decalEffectScale = Vector3.one;

    [Tooltip("Assign a parent whoes world scale is uniform to 1, Only if the original object's scale is different.")]
    [SerializeField] private Transform _decalEffectParent;

    [Tooltip("Optional: Assign only if you want to play an audio effect when a hit occurs on this object")]
    [SerializeField] private AudioClip _hitObjectClip;
    
    [Tooltip("Audio Effect Volume, Useful if you assign the hitObjectClip")]
    [SerializeField, Range(0f, 1f)] private float _audioVolume = 1.0f;

    public void Hit(GameObject hitObject, Vector3 hitPoint, Vector3 hitNormal)
    {
        // Spawn Bullet Decal Effect.
        GameObject decal = Instantiate(_decalPrefab, hitPoint, Quaternion.LookRotation(hitNormal));

        // Set Size.
        decal.transform.localScale = _decalEffectScale;

        // Random Rotation, For more realism.
        decal.transform.Rotate(transform.forward, Random.Range(0, 360));

        // Child effect so it moves with the hit object.
        if (_decalEffectParent != null)
        {
            // Set Parent to custom.
            decal.transform.SetParent(_decalEffectParent);
        }
        else
        {
            // Set parent to object.
            decal.transform.SetParent(hitObject.transform);
        }

        // Check if audio clip is assigned.
        if (_hitObjectClip != null)
        {
            // Only then Play that clip.
            AudioSource.PlayClipAtPoint(_hitObjectClip, hitPoint, _audioVolume);
        }
    }
}
