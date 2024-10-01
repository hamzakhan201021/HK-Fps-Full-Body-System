using UnityEngine;

// CHANGES NEW SCRIPT
public class ItemBase : MonoBehaviour
{
    public string itemName;
    public string description;
    public Sprite icon;

    // Called when the player first presses the Use button.
    public virtual void StartUse(PlayerController controller)
    {
    }

    // Called if the Use button is held down.
    public virtual void HoldUse(PlayerController controller)
    {
    }

    // Called when the player releases the Use button.
    public virtual void ReleaseUse(PlayerController controller)
    {   
    }

    // Called if the player cancels the use action.
    public virtual void CancelUse(PlayerController controller)
    {   
    }
}
