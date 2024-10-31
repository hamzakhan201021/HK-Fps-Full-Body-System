using UnityEngine;

public class Ammo : MonoBehaviour, IInteractable
{

    [Header("Ammo Settings")]
    [SerializeField] private int _totalAmmo = 90;
    [SerializeField] private string _interactMessage;

    // CHANGES (
    //public void Interact(PlayerController playerController)
    //{
    //    playerController.CurrentWeapon().TotalAmmo += _totalAmmo;
    //    Destroy(gameObject);
    //}

    public void Interact(HKPlayerInteractionBase playerController)
    {
        playerController.AddAmmo(_totalAmmo);
        Destroy(gameObject);
    }
    // CHANGES )

    // CHANGES (
    //public bool CanInteract(PlayerController playerController)
    //{
    //    return true;
    //}

    public bool CanInteract(HKPlayerInteractionBase interactionController)
    {
        return true;
    }
    // CHANGES )

    public string GetMessage()
    {
        return _interactMessage;
    }
}
