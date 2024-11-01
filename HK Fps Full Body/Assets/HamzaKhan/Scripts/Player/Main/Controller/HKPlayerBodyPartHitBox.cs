using UnityEngine;

public class HKPlayerBodyPartHitBox : MonoBehaviour, IHitable
{

    [Space]
    [Header("Main")]
    public HKPlayerHealthSystem HKPlayerHealthSystem;
    public GameObject EffectPrefab;
    public Vector3 EffectScale = Vector3.one;
    [Header("Health")]
    public float HealthCut;
    public BodyPart BodyPart;

    public void Hit(GameObject hitObject, Vector3 hitPoint, Vector3 hitNormal, HealthDamageData healthDamageData)
    {
        // Damage player
        HKPlayerHealthSystem.BodyReceiveDamage(healthDamageData, BodyPart, HealthCut);

        // Create effect
        GameObject effect = Instantiate(EffectPrefab, hitPoint, Quaternion.LookRotation(hitNormal));

        effect.transform.localScale = EffectScale;

        effect.transform.Rotate(transform.forward, Random.Range(0, 360));

        effect.transform.SetParent(transform);
    }
}
