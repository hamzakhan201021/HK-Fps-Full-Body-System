using UnityEngine;

public class Gate : MonoBehaviour, IInteractable
{
    
    [SerializeField] private string _interactMessage = "Open/Close Gate";
    [SerializeField] private string _openCloseTriggerName = "OpenCloseTrigger";

    private int _openCloseTriggerNameHash;

    private Animator _anim;

    // Start is called before the first frame update
    void Start()
    {
        // Get Animator.
        _anim = GetComponent<Animator>();

        // Hash trigger name.
        _openCloseTriggerNameHash = Animator.StringToHash(_openCloseTriggerName);
    }

    /// <summary>
    /// Returns the message to show on the interaction.
    /// </summary>
    /// <returns></returns>
    public string GetMessage()
    {
        return _interactMessage;
    }

    // CHANGES (
    ///// <summary>
    ///// Returns if you can interact with this object.
    ///// </summary>
    ///// <param name="playerController"></param>
    ///// <returns></returns>
    //public bool CanInteract(PlayerController playerController)
    //{
    //    return true;
    //}

    /// <summary>
    /// Returns if you can interact with this object.
    /// </summary>
    /// <param name="playerController"></param>
    /// <returns></returns>
    public bool CanInteract(HKPlayerInteractionBase interactionController)
    {
        return true;
    }

    // CHANGES )

    // CHANGES (
    ///// <summary>
    ///// Use this to perform actual interaction.
    ///// </summary>
    ///// <param name="playerController"></param>
    //public void Interact(PlayerController playerController)
    //{
    //    _anim.SetTrigger(_openCloseTriggerNameHash);
    //}

    /// <summary>
    /// Use this to perform actual interaction.
    /// </summary>
    /// <param name="playerController"></param>
    public void Interact(HKPlayerInteractionBase playerController)
    {
        _anim.SetTrigger(_openCloseTriggerNameHash);
    }
    // CHANGES )
}
