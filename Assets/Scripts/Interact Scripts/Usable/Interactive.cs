using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactive : OutlineObject
{
    // public variables
    public bool pauseOnInteract = true;
    public string textId;
    public bool loopLast;
    public bool highlightEnds = false;

    public LockedBehavior lockable;

    // private trackers

    // dialogue vars
    private InteractiveInfo[] interactivesText;
    private InteractiveUIController UI;
    private bool triggered = false;

    new public void OnTriggerEnter2D(Collider2D collider)
    {
        if(OutlineEnabled())
            base.OnTriggerEnter2D(collider);
    }

    public void Update()
    {
        if(IsTriggered())
        {
            if(!UIActive()) triggered = true;
            TriggerDialogue();
            Debug.Log("triggered = true");
        }
    }

    new public void Awake()
    {
        base.Awake();
    }

    new public void Start()
    {
        base.Start();

        UI = FindObjectOfType<InteractiveUIController>();

        interactivesText = GetText(textId);
        if(interactivesText == null)
            interactivesText = new InteractiveInfo[0];

        if(lockable.isLocked)
            lockable.SetText(GetText(lockable.lockedTextID));
    }

    public InteractiveInfo[] GetText(string id)
    {
        return InteractiveInfo.ParseData(InteractiveTextDatabase.GetText(id));
    }

    public void TriggerDialogue()
    {
        if(lockable.IsUnlocked())
            TriggerDialogue(textId, interactivesText, loopLast);
        else
            TriggerDialogue(lockable.lockedTextID, lockable.GetText(), lockable.loopLast);

        if(!OutlineEnabled())
            DisableOutline();
    }

    public void TriggerDialogue(string script_id, InteractiveInfo[] allScripts, bool loop_last)
    {
        if(script_id == null) return;

        // don't trigger if dialogue is currently active
        if(UI.IsActive()) return;

        // pause now if I've made it this far

        int index = UIManager.GetInteractiveIndex(script_id);

        if(index >= allScripts.Length) return;

        if(pauseOnInteract) EntityManager.DialoguePause();
        UI.StartInteractive(allScripts[index], pauseOnInteract);

        if(!loop_last || index + 1 < allScripts.Length) index++;

        UIManager.SetInteractiveIndex(script_id, index);
    }

    bool OutlineEnabled()
    {
        int index = UIManager.GetInteractiveIndex(textId);

        return !highlightEnds || index < interactivesText.Length || !lockable.IsUnlocked();
    }

    public bool IsTriggered()
    {
        return IsPlayerNear() && InputManager.InteractKeyDown();
    }

    public bool UIActive()
    {
        if(UI == null) return false;

        return UI.IsActive();
    }

    public bool ActivateBehavior()
    {
        if(!triggered || UIActive() || !lockable.IsUnlocked()) return false;

        triggered = false;
        return true;
    }
}
