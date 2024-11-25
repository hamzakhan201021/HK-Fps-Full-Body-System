namespace HKFps
{
    public interface IInteractable
    {
        string GetMessage();

        bool CanInteract(HKPlayerInteractionBase interactionController);

        void Interact(HKPlayerInteractionBase interactionController);
    }
}