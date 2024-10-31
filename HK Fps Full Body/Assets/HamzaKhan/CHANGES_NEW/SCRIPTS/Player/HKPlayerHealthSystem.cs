using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class HKPlayerHealthSystem : MonoBehaviour, IDamageable
{

    #region Variables

    [Space]
    [Header("Health Settings")]
    [SerializeField] private float _maxHealth = 100;
    [SerializeField] private float _deathThreshold = 0;
    [SerializeField] private float _health = 100;
    [SerializeField] private float _constantHealthIncreaseRate = 5f;
    [SerializeField] private float _minVelocityHitHealthDeduct = 10;
    [SerializeField] private float _hitTheGroundHealthDeductMulti = 1;
    [Space]
    [Header("Stamina Settings")]
    [SerializeField] private float _maxStamina = 100;
    [SerializeField] private float _staminaEndThreshold = 0;
    [SerializeField] private float _stamina = 100;
    [SerializeField] private float _constantStaminaIncreaseRate = 10f;
    [SerializeField] private float _sprintingStaminaDecreaseRate = 10;
    [SerializeField] private bool _increaseStaminaWhenDecreasing = false;
    [Space]
    [Header("Death Settings")]
    [Header("Animator")]
    [SerializeField] private Animator _animator;
    [Space]
    [Header("Ragdoll Rigidbodies")]
    [SerializeField] private List<Rigidbody> _ragdollRigidbodies;
    [Space]
    [SerializeField] private List<GameObject> _objToDisableOnDeath;
    [SerializeField] private List<Behaviour> _behavioursToDisableOnDeath;
    [Space]
    [SerializeField] private CharacterController _controllerToDisableOnDeath;
    [Space]
    [Header("Events")]
    [Space]
    public UnityEvent OnDeath;
    public UnityEvent OnRevive;
    public UnityEvent<float, float> OnUpdateHealth;
    public UnityEvent<float, float> OnUpdateStamina;
    public UnityEvent<float, float> OnDeductHealth;

    private HealthState _healthState = HealthState.Alive;

    public enum HealthState
    {
        Alive,
        Dead,
    }

    #endregion

    #region General

    // Start is called before the first frame update
    void Start()
    {
        Setup();
    }

    private void Setup()
    {
        for (int i = 0; i < _ragdollRigidbodies.Count; i++)
        {
            _ragdollRigidbodies[i].isKinematic = true;
        }
    }

    public void UpdateHealthSystem(bool sprinting, bool reviveTriggered)
    {
        if (reviveTriggered)
        {
            TryRevivePlayer();
        }


        if (_healthState == HealthState.Dead) return;

        UpdateHealth();
        UpdateStamina(sprinting);
    }

    private void UpdateHealth()
    {
        if (_health < _maxHealth)
        {
            _health += _constantHealthIncreaseRate * Time.deltaTime;
            OnUpdateHealth.Invoke(_health, _maxHealth);
        }
        else if (_health > _maxHealth)
        {
            _health = _maxHealth;
            OnUpdateHealth.Invoke(_health, _maxHealth);
        }
    }

    private void UpdateStamina(bool sprinting)
    {
        if (sprinting)
        {
            _stamina -= _sprintingStaminaDecreaseRate * Time.deltaTime;
            OnUpdateStamina.Invoke(_stamina, _maxStamina);
        }

        if (!sprinting || _increaseStaminaWhenDecreasing)
        {
            if (_stamina < _maxStamina)
            {
                _stamina += _constantStaminaIncreaseRate * Time.deltaTime;
                OnUpdateStamina.Invoke(_stamina, _maxStamina);
            }
            else if (_stamina > _maxStamina)
            {
                _stamina = _maxStamina;
                OnUpdateStamina.Invoke(_stamina, _maxStamina);
            }
        }
    }

    #endregion

    #region Event Callbacks

    public void BodyReceiveDamage(HealthDamageData healthDamageData, BodyPart bodyPart, float damage)
    {
        float damageAmount = 0;

        switch (bodyPart)
        {
            case BodyPart.Head:
                damageAmount = damage * healthDamageData.HeadShotMultiplier;
                break;
            case BodyPart.Body:
                damageAmount = damage * healthDamageData.BodyShotMultiplier;
                break;
            case BodyPart.Arm:
                damageAmount = damage * healthDamageData.ArmShotMultiplier;
                break;
            case BodyPart.Leg:
                damageAmount = damage * healthDamageData.LegShotMultiplier;
                break;
        }

        DeductHealth(damageAmount);
    }

    public void OnHitTheGround(float velocity)
    {
        if (-velocity >= _minVelocityHitHealthDeduct)
        {
            // Deduct Health
            DeductHealth(-velocity * _hitTheGroundHealthDeductMulti);
        }
    }

    public void DeductHealth(float damage)
    {
        if (_healthState == HealthState.Dead) return;

        float previousHealth = _health;

        _health -= damage;

        OnUpdateHealth.Invoke(_health, _maxHealth);

        OnDeductHealth.Invoke(previousHealth, _health);

        if (_health <= _deathThreshold)
        {
            KillPlayer();
        }
    }

    public void AddHealth(float healthToAdd)
    {
        if ((_health + healthToAdd) <= _maxHealth)
        {
            _health += healthToAdd;
        }
        else
        {
            _health = _maxHealth;
        }
    }

    private void KillPlayer()
    {
        _animator.enabled = false;

        for (int i = 0; i < _ragdollRigidbodies.Count; i++)
        {
            _ragdollRigidbodies[i].isKinematic = false;
        }

        for (int i = 0; i < _objToDisableOnDeath.Count; i++)
        {
            _objToDisableOnDeath[i].SetActive(false);
        }

        for (int i = 0; i < _behavioursToDisableOnDeath.Count; i++)
        {
            _behavioursToDisableOnDeath[i].enabled = false;
        }

        _controllerToDisableOnDeath.enabled = false;

        _healthState = HealthState.Dead;

        OnDeath.Invoke();
    }

    private void TryRevivePlayer()
    {
        if (_healthState == HealthState.Alive) return;

        _animator.enabled = true;

        for (int i = 0; i < _ragdollRigidbodies.Count; i++)
        {
            _ragdollRigidbodies[i].isKinematic = true;
        }

        for (int i = 0; i < _objToDisableOnDeath.Count; i++)
        {
            _objToDisableOnDeath[i].SetActive(true);
        }

        for (int i = 0; i < _behavioursToDisableOnDeath.Count; i++)
        {
            _behavioursToDisableOnDeath[i].enabled = true;
        }

        _controllerToDisableOnDeath.enabled = true;

        _health = _maxHealth;
        _stamina = _maxStamina;

        _healthState = HealthState.Alive;

        OnRevive.Invoke();
    }

    #endregion

    #region System State Returners

    public bool HasStamina()
    {
        return _stamina > _staminaEndThreshold;
    }

    public HealthState GetHealthState()
    {
        return _healthState;
    }

    public void Damage(float damageAmount)
    {
        // Perform damage
        DeductHealth(damageAmount);
    }

    #endregion
}