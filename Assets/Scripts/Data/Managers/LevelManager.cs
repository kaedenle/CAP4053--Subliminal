using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    // delay between when the scene loads and when the player and other entities can move
    private static float default_delay = 1F;

    // the starting scene of the current level
    private static ScenesManager.AllScenes _startScene;
    private static SubLevelManager level;


    public static bool ignorePreviousScene {get; private set;} = false;
    // bool for the cutscene triggering
    private static bool _levelEnding = false;

    // bool for player death ui
    private static bool _playerDied = false;

    // holds the location of the player and the last known location of the detective
    private static GameObject player;
    private static Vector3 spawnPosition;

    // holds all the interactive event states
    private static Dictionary<string, bool> objectState;

    public void Awake()
    {
        player = GeneralFunctions.GetPlayer();

        if(objectState == null) // not seen before
        {
            objectState = new Dictionary<string, bool>();
        }

        _levelEnding = false;
        _playerDied = false;

        EntityManager.SceneStartPause();
        EntityManager.WaitThenUnpause(default_delay);
    }

    public void Start()
    {
        ignorePreviousScene = false;
    }

    public static void SetLevel(SubLevelManager sublevel, ScenesManager.AllScenes start)
    {
        _startScene = start;
        level = sublevel;
    }

    public static void FullReset()
    {
        LevelManager.ResetAllVariables();
        ChildLevelManager.ResetVariables();
        HubManager.ResetVariables();
        MobsterLevelManager.ResetVariables();
        CastleLevelManager.ResetVariables();
    }

    public static void ResetVariables()
    {
        _playerDied = false;
        _levelEnding = false;
        objectState.Clear();
    }

    public static void ResetAllVariables()
    {
        ResetVariables();
        InventoryManager.ResetVariables();
        UIManager.ResetManager();

        if (level != null)
            level.TriggerReset();
    }

    public static void TriggerEnd()
    {
        _levelEnding = true;
    }

    public static void EndLevel()
    {
        // GameState game = GameState.LoadGame();
        // game.IncrementStateAndSave();
        GameData.GetInstance().IncrementLevel();
        ResetAllVariables();

        ScenesManager.LoadScene(ScenesManager.AllScenes.CentralHub);
    }

    public static void EndGame()
    {
        // reset game data
        GameData.GetInstance().ResetData();
        GameData.GetInstance().SetCompletion();
        GameData.GetInstance().SetScene(ScenesManager.AllScenes.Menu);
        GameData.GetInstance().SaveCurrentData(false);

        // go to credits
        ScenesManager.LoadScene(ScenesManager.AllScenes.Credits);
    }

    public static bool IsEndOfLevel()
    {
        return _levelEnding;
    }

    public static bool PauseEnabled()
    {
        // current behavior: can't pause during cutscene or during player death
        return !IsEndOfLevel() && !PlayerDead(); 
    }

    public static void TriggerPlayerDeath()
    {
        _playerDied = true;

        if(!EntityManager.IsPaused())
            EntityManager.Pause();
    }

    public static bool PlayerDead()
    {
        return _playerDied;
    }

    public static void RestartLevel()
    {   
        ResetAllVariables();

        // reset from player death
        GameData.GetInstance().SaveAfterSceneChange();

        ignorePreviousScene = true;

        if (level != null)
            ScenesManager.LoadScene(_startScene);
    }

    public static void CheckpointButton()
    {
        ResetAllVariables();
        ignorePreviousScene = true;

        GameData.GetInstance().RevertAfterDeath();
    }

    public static void ReloadScene()
    {
        SetRespawn();
        ScenesManager.ReloadScene();
    }

    public static void SetRespawn()
    {
        spawnPosition = player.transform.position;
    }

    public static Vector3 GetRespawnPosition()
    {
        return spawnPosition;
    }

    public static void ToggleInteractiveState(string id)
    {
        if(id == null) return;

        if(!objectState.ContainsKey(id))
            objectState.Add(id, false);

        objectState[id] = !objectState[id];
    }

    public static bool GetInteractiveState(string id)
    {
        if(id == null) return false;
        
        return objectState.GetValueOrDefault(id, false) ;
    }

    public static Dictionary<string, bool> GetObjectStates()
    {
        if(objectState == null) return null;
        return new Dictionary<string, bool> ( objectState );
    }

    public static void SetObjectStates(Dictionary<string, bool> os)
    {
        if(os == null) return;
        objectState = new Dictionary<string, bool> ( os );
    }

    public static void CityPaperSave()
    {
        // if(GameData.GetInstance().GetDifficulty() == GameData.Difficulty.Hard) return; // don't save any papers on Hard mode

        // update all the manager variables
        GameData.GetInstance().StoreManagerVariables();
        GameData.GetInstance().SaveCurrentData(false);
    }

    public static void CastlePaperSave(InventoryManager.AllItems letter)
    {
        // if(GameData.GetInstance().GetDifficulty() == GameData.Difficulty.Hard) return; // don't save papers on Hard mode
        
        GameData.GetInstance().AddSingleItem(letter);
        GameData.GetInstance().SaveCurrentData(false);
    }

    /*
    Non Static Methods
    */
    public virtual void TriggerReset() { } // used abstractly; kinda sketchy
}
