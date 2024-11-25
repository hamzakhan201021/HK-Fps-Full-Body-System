using UnityEngine;

namespace HKFps
{
    public class ItemBase : MonoBehaviour, IInteractable
    {
        public string ItemName;
        [TextArea]
        public string Description;
        public string InteractMessage;

        [Tooltip("You can think of this as the true identity of the item")]
        public ItemType ItemType;
        [Tooltip("This Item ID is used for Animations")]
        public int ItemID = 1;

        // Called when the player first presses the Use button.
        public virtual void StartUse(HKPlayerItemSystem itemSystem)
        {
        }

        // Called if the Use button is held down.
        public virtual void HoldUse(HKPlayerItemSystem itemSystem)
        {
        }

        // Called when the player releases the Use button.
        public virtual void ReleaseUse(HKPlayerItemSystem itemSystem)
        {
        }

        // Called if the player cancels the use action.
        public virtual void CancelUse(HKPlayerItemSystem itemSystem)
        {
        }

        public virtual string GetMessage()
        {
            return InteractMessage;
        }

        public virtual bool CanInteract(HKPlayerInteractionBase interactionController)
        {
            return true;
        }

        public virtual void Interact(HKPlayerInteractionBase interactionController)
        {
        }
    }
}