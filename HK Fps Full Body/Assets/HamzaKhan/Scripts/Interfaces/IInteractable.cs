public interface IInteractable
{
    string GetMessage();
    // CHANGES (
    //bool CanInteract(PlayerController playerController);
    bool CanInteract(HKPlayerInteractionBase interactionController);
    // CHANGES )
    // CHANGES (
    //void Interact(PlayerController playerController);
    void Interact(HKPlayerInteractionBase interactionController);
    // CHANGES )
}
