using UnityEngine.Events;
using UnityEngine;

public class HKPlayerItemSystem : MonoBehaviour
{

    #region Fields

    [Space]
    [Header("Prediction Line")]
    public bool UsePredictionLine = false;
    public LayerMask LineCollisionLayerMask;
    [Space]
    public LineRenderer LineRenderer = null;
    public Transform ReleasePosition = null;
    [Space]
    [Range(10, 100)] public int LinePoints = 25;
    [Range(0.01f, 0.25f)] public float TimeBetweenPoints = 0.1f;
    [Space]
    public float ThrowStrength = 20f;
    public float RayOverlap = 1.1f;

    [Space]
    [Header("Events")]
    [Space]
    public UnityEvent<ItemBase> OnItemStartUse;
    public UnityEvent<ItemBase> OnItemReleaseUse;
    public UnityEvent<ItemBase> OnItemUseCanceled;
    public UnityEvent<ItemBase> OnItemUseComplete;
    [Space]
    public UnityEvent OnOpenItemWheel;
    public UnityEvent OnCloseItemWheel;
    [Space]
    public UnityEvent<float> OnAddHealth;

    private InventoryItem _currentInventoryItem;

    // Button State Enum
    private enum ButtonState
    {
        performed,
        rest,
    }

    private ButtonState _itemButtonState = ButtonState.rest;

    #endregion

    #region Main/General

    public void UpdateItemSystem(bool switchInputPerformed, bool performingSwitch, bool reloading, InventoryItem currentInventoryItem,
        bool useItemTriggered, bool useItemReleased, bool useItemPressed,
        bool openItemWheelTriggered, bool openItemWheelReleased, bool cancelUseItemTriggered, bool detonateTriggered)
    {
        HandleItems(switchInputPerformed, performingSwitch, reloading, currentInventoryItem, useItemTriggered, useItemReleased,
            useItemPressed, openItemWheelTriggered, openItemWheelReleased, cancelUseItemTriggered, detonateTriggered);
    }

    public void UpdatePredictionLine(Rigidbody rb)
    {
        Vector3 velocity = ReleasePosition.forward * (ThrowStrength / rb.mass);
        Vector3 position = ReleasePosition.position;
        Vector3 nextPosition;

        float overlap;

        UpdatePredictionLineRender(LinePoints, (0, position));

        for (int i = 1; i < LinePoints; i++)
        {
            // Estimate velocity and update next predicted position
            velocity = CalculatePredictionNewVelocity(velocity, rb.linearDamping, TimeBetweenPoints);
            nextPosition = position + velocity * TimeBetweenPoints;

            // Overlap our rays by small margin to ensure we never miss a surface
            overlap = Vector3.Distance(position, nextPosition) * RayOverlap;

            // When hitting a surface we want to show the surface marker and stop updating our line
            if (Physics.Raycast(position, velocity.normalized, out RaycastHit hit, overlap))
            {
                UpdatePredictionLineRender(i, (i - 1, hit.point));
                break;
            }

            // If nothing is hit, continue rendering the arc without a visual marker
            position = nextPosition;
            UpdatePredictionLineRender(LinePoints, (i, position)); //Unneccesary to set count here, but not harmful
        }
    }

    //public void PerformThrowAnimation(Rigidbody rb)
    //{
    //    OnThrowItemStart.Invoke();
    //}

    //public void CancelThrow()
    //{
    //    OnItemThrowCanceled.Invoke();
    //}

    public void SetPredictionLineVisible(bool visible)
    {
        //if (visible) OnShowPredictionLine.Invoke();

        LineRenderer.enabled = visible;
    }

    #endregion

    #region Calculation/Management

    private void HandleItems(bool switchInputPerformed, bool performingSwitch, bool reloading, InventoryItem currentInventoryItem,
        bool useItemTriggered, bool useItemReleased, bool useItemPressed,
        bool openItemWheelTriggered, bool openItemWheelReleased, bool cancelUseItemTriggered, bool detonateTriggered)
    {
        if (!switchInputPerformed && !reloading && currentInventoryItem.Amount > 0)
        {
            if (useItemTriggered)
            {
                StartUse(currentInventoryItem);
            }

            if (useItemReleased && _itemButtonState == ButtonState.performed)
            {
                ReleaseUse(currentInventoryItem);
            }
        }

        if (useItemPressed && _itemButtonState == ButtonState.performed)
        {
            HoldUse(currentInventoryItem, cancelUseItemTriggered);
        }

        // Item Wheel Active Handling
        if (openItemWheelTriggered && !switchInputPerformed && !performingSwitch)
        {
            OnOpenItemWheel.Invoke();
        }

        if (openItemWheelReleased)
        {
            OnCloseItemWheel.Invoke();
        }

        if (detonateTriggered)
        {
            TryDetonateCurrentItem();
        }
    }

    /// <summary>
    /// Allows us to set line count and an induvidual position at the same time
    /// </summary>
    /// <param name="count">Number of points in our line</param>
    /// <param name="pointPos">The position of an induvidual point</param>
    private void UpdatePredictionLineRender(int count, (int point, Vector3 pos) pointPos)
    {
        LineRenderer.positionCount = count;
        LineRenderer.SetPosition(pointPos.point, pointPos.pos);
    }

    private Vector3 CalculatePredictionNewVelocity(Vector3 velocity, float drag, float increment)
    {
        velocity += Physics.gravity * increment;
        velocity *= Mathf.Clamp01(1f - drag * increment);
        return velocity;
    }

    private void TryDetonateCurrentItem()
    {
        IDetonatable detonatable = _currentInventoryItem.Item.GetComponent<IDetonatable>();

        detonatable?.TriggerDetonation();
    }

    #endregion

    #region Item Input Functions

    private void StartUse(InventoryItem currentInventoryItem)
    {
        _currentInventoryItem = currentInventoryItem;
        currentInventoryItem.Item.StartUse(this);
        currentInventoryItem.Item.gameObject.SetActive(true);

        _itemButtonState = ButtonState.performed;

        OnItemStartUse.Invoke(_currentInventoryItem.Item);
    }

    private void ReleaseUse(InventoryItem currentInventoryItem)
    {
        _currentInventoryItem = currentInventoryItem;
        currentInventoryItem.Item.ReleaseUse(this);
        currentInventoryItem.Amount -= 1;

        _itemButtonState = ButtonState.rest;

        OnItemReleaseUse.Invoke(_currentInventoryItem.Item);
    }

    private void HoldUse(InventoryItem currentInventoryItem, bool cancelUseItemTriggered)
    {
        _currentInventoryItem = currentInventoryItem;
        currentInventoryItem.Item.HoldUse(this);

        if (cancelUseItemTriggered)
        {
            CancelUse(currentInventoryItem);
        }
    }

    private void CancelUse(InventoryItem currentInventoryItem)
    {
        _currentInventoryItem = currentInventoryItem;
        currentInventoryItem.Item.CancelUse(this);
        currentInventoryItem.Item.gameObject.SetActive(false);

        _itemButtonState = ButtonState.rest;

        OnItemUseCanceled.Invoke(_currentInventoryItem.Item);
    }

    public void OnThrowableThrow()
    {
        _currentInventoryItem.Item.TryGetComponent(out IThrowable iThrowable);

        iThrowable?.ThrowObj(this, ReleasePosition, ThrowStrength);
    }

    public void AddHealth(float healthToAdd)
    {
        OnAddHealth.Invoke(healthToAdd);
    }

    public void OnUseComplete()
    {
        OnItemUseComplete.Invoke(_currentInventoryItem.Item);
    }

    #endregion
}