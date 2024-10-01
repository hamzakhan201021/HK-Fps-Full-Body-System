public interface IInteractable
{
    string GetMessage();
    bool CanInteract(PlayerController playerController);
    void Interact(PlayerController playerController);
}
