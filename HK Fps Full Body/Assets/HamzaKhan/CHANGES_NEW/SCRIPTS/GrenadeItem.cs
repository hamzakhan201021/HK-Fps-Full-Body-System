using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Cinemachine;

// CHANGES NEW SCRIPT
public class GrenadeItem : ItemBase, IThrowable
{

    #region Fields

    [Space]
    [Header("Explosion Refs")]
    public GameObject explosionPrefab;
    public Rigidbody _rb = null;
    public List<AudioClip> explosionAudioClips;
    [Header("Explosion Camera Shake")]
    public CinemachineImpulseSource explosionImpulseSource;
    public CameraShakeProfile CameraShakeProfile;

    [Space]
    [Header("Explosion Audio Settings")]
    [Range(0, 1)] public float explosionVolume = 1;
    [Range(-3, 3)] public float pitch = 1;
    [Range(0, 1)] public float spatialBlend = 1;
    public AudioRolloffMode audioRolloffMode = AudioRolloffMode.Logarithmic;
    public float minDistance = 10f;
    public float maxDistance = 500;

    [Space]
    [Header("Explosion Settings")]
    public LayerMask explodableLayerMask;
    public LayerMask obstructionLayerMask;
    public QueryTriggerInteraction explosionTriggerInteraction;
    [Space]
    public float explosionRadius = 2.5f;
    public float delayBeforeExplosion = 3;
    public float explosionForce = 800;
    public float maxDamage = 100f;

    [Space]
    [Header("Grenade Throwing Object Settings")]
    public bool UseSeperateThrownObject;
    public GameObject ThrownObject;

    protected bool _canInteract = true;

    #endregion

    #region Overrides
    public override void StartUse(HKPlayerItemSystem itemSystem)
    {
        ShowAimingIndicators(itemSystem);  // Start aiming visuals.
    }

    public override void HoldUse(HKPlayerItemSystem itemSystem)
    {
        UpdateAimingIndicators(itemSystem);  // Keep updating the aiming visuals.
    }

    public override void ReleaseUse(HKPlayerItemSystem itemSystem)
    {
        ThrowGrenade(itemSystem);  // Execute the throw when the button is released.
    }

    public override void CancelUse(HKPlayerItemSystem itemSystem)
    {
        HideAimingIndicators(itemSystem);  // Cancel and hide aiming visuals.
    }

    public override bool CanInteract(HKPlayerInteractionBase interactionController)
    {
        return _canInteract;
    }

    public override void Interact(HKPlayerInteractionBase interactionController)
    {
        interactionController.AddNewItem(this);
    }

    #endregion

    #region Events
    protected void ShowAimingIndicators(HKPlayerItemSystem itemSystem)
    {
        itemSystem.SetPredictionLineVisible(true);
        itemSystem.UpdatePredictionLine(_rb);
    }

    protected void UpdateAimingIndicators(HKPlayerItemSystem itemSystem)
    {
        itemSystem.UpdatePredictionLine(_rb);
    }

    protected void HideAimingIndicators(HKPlayerItemSystem itemSystem)
    {
        itemSystem.SetPredictionLineVisible(false);
    }

    protected void ThrowGrenade(HKPlayerItemSystem itemSystem)
    {
        itemSystem.SetPredictionLineVisible(false);
        //itemSystem.PerformThrowAnimation(_rb);
    }
    #endregion

    public virtual void ThrowObj(HKPlayerItemSystem controller, Transform releasePosition, float force)
    {
        // Disable current grenade
        gameObject.SetActive(false);

        // Throw a cloned grenade.
        //CloneThrowExplodeGrenade(UseSeperateThrownObject ? ThrownObject : gameObject, releasePosition.forward * force);
        GameObject clonedGrenade = CloneThrowGrenade(UseSeperateThrownObject ? ThrownObject : gameObject, releasePosition.forward * force);

        ExplodeGrenadeDelayedObj(clonedGrenade);

        controller.OnUseComplete();
    }

    protected GameObject CloneThrowGrenade(GameObject grenade, Vector3 force)
    {
        GameObject grenadeObj = Instantiate(grenade, transform.position, transform.rotation);

        grenadeObj.TryGetComponent(out Rigidbody grenadeRb);

        grenadeObj.SetActive(true);

        if (grenadeRb != null)
        {
            grenadeRb.isKinematic = false;
            grenadeRb.AddForce(force, ForceMode.Impulse);
        }

        return grenadeObj;
    }

    protected void ExplodeGrenadeObj(GameObject grenadeObj)
    {
        grenadeObj.TryGetComponent(out GrenadeItem grenadeItem);

        grenadeItem?.ExplodeGrenade();
    }

    protected void ExplodeGrenadeDelayedObj(GameObject grenadeObj)
    {
        grenadeObj.TryGetComponent(out GrenadeItem grenadeItem);

        grenadeItem?.ExplodeGrenadeDelay();
    }

    #region Explosion

    public void ExplodeGrenadeDelay()
    {
        StartCoroutine(ExplodeGrenadeCoroutine());
    }

    protected IEnumerator ExplodeGrenadeCoroutine()
    {
        _canInteract = false;
        yield return new WaitForSeconds(delayBeforeExplosion);

        ExplodeGrenade();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (ItemType == ItemType.StickyGrenade)
        {
            _rb.isKinematic = true;
        }
    }

    // Triggered when grenade explodes
    public void ExplodeGrenade()
    {
        // Clone Explosion Effect
        Instantiate(explosionPrefab, transform.position, Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)), null);

        // Play Audio Clip With Settings.
        AudioSource audioSource = new GameObject("One Shot Audio").AddComponent<AudioSource>();

        audioSource.pitch = pitch;
        audioSource.spatialBlend = spatialBlend;
        audioSource.rolloffMode = audioRolloffMode;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;

        AudioClip clip = GetClipFromClips(explosionAudioClips);

        audioSource.PlayOneShot(clip, explosionVolume);

        // Destroy Audio Clip after the clip ends.
        Destroy(audioSource.gameObject, clip.length);

        // Get all colliders in the radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, explodableLayerMask, explosionTriggerInteraction);
        List<Collider> affectedObjects = new List<Collider>();

        // Check line of sight to determine if objects are obstructed
        for (int i = 0; i < colliders.Length; i++)
        {
            if (IsObjectVisible(colliders[i]))
            {
                affectedObjects.Add(colliders[i]);
            }
        }

        // Apply damage and force to each affected object
        for (int i = 0; i < affectedObjects.Count; i++)
        {
            // Calculate damage based on distance
            float distance = Vector3.Distance(transform.position, affectedObjects[i].transform.position);

            float damage = CalculateDamage(distance);

            // Try to apply damage if object has a damageable component
            IDamageable damageable = affectedObjects[i].GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.Damage(damage);
            }

            // Try to add explosion force if object has a Rigidbody
            Rigidbody rb = affectedObjects[i].GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }

        CameraShakeManager.instance.CameraShakeFromProfile(CameraShakeProfile, explosionImpulseSource);

        Destroy(gameObject);
    }

    // Calculates damage based on the distance from explosion
    protected float CalculateDamage(float distance)
    {
        // Example: linear falloff
        float damage = maxDamage * (1 - distance / explosionRadius);
        return Mathf.Max(0, damage); // Ensure damage is non-negative
    }

    // Determines if an obj is visible from this obj's position
    protected bool IsObjectVisible(Collider obj)
    {
        float distance = Vector3.Distance(transform.position, obj.transform.position);

        if (!Physics.Raycast(transform.position, (obj.transform.position - transform.position).normalized, distance, obstructionLayerMask))
        {
            return true;
        }

        return false;
    }

    protected AudioClip GetClipFromClips(List<AudioClip> audioClips)
    {
        int clipIndex = Random.Range(0, audioClips.Count - 1);

        return audioClips[clipIndex];
    }

    #endregion

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
    #endregion
}