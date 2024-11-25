using UnityEngine;

namespace HKFps
{
    public class Ammo : MonoBehaviour, IInteractable
    {

        [Header("Ammo Settings")]
        [SerializeField] private int _totalAmmo = 90;
        [SerializeField] private string _interactMessage;

        public void Interact(HKPlayerInteractionBase playerController)
        {
            playerController.AddAmmo(_totalAmmo);
            Destroy(gameObject);
        }

        public bool CanInteract(HKPlayerInteractionBase interactionController)
        {
            return true;
        }

        public string GetMessage()
        {
            return _interactMessage;
        }
    }
}