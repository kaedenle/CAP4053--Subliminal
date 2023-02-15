using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobsterLevelManager : MonoBehaviour
{
    static bool _alleyGateOpen = false;

    // Start is called before the first frame update
    public static bool AlleyGateUnlocked()
    {
        return _alleyGateOpen;
    }

    // returns true if the gate is already unlocked or if the player has the key
    public static bool UnlockAlleyGate(InventoryManager.AllItems key)
    {
        if (AlleyGateUnlocked()) return true;

        if(InventoryManager.HasItem(key))
        {
            Debug.Log("Alley Gate Has Been Opened");
            InventoryManager.RemoveItem(key);
            _alleyGateOpen = true;
            return true;
        }

        return false;
    }

    // check if the key has been picked up or used
    public static bool usedKey(InventoryManager.AllItems key)
    {
        if (key == InventoryManager.AllItems.MobsterKeyDemo && _alleyGateOpen)
            return true;

        return false;
    }
}