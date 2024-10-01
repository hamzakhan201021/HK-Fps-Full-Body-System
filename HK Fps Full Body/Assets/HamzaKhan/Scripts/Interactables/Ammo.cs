using UnityEngine;

public class Ammo : MonoBehaviour, IInteractable
{

    [Header("Ammo Settings")]
    [SerializeField] private int _totalAmmo = 90;
    [SerializeField] private string _interactMessage;


    public void Interact(PlayerController playerController)
    {
        playerController.CurrentWeapon().TotalAmmo += _totalAmmo;
        Destroy(gameObject);
    }

    public bool CanInteract(PlayerController playerController)
    {
        return true;
    }

    public string GetMessage()
    {
        return _interactMessage;
    }
}
