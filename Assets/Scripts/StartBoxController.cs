using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartBoxController : MonoBehaviour
{
    [SerializeField] ScenesManager.AllScenes _prevScene;
    private GameObject player;

    // Start is called before the first frame update
    protected void Awake()
    {
        // get the player
        player = GameObject.FindGameObjectWithTag("Player");
        if(player == null) 
            return;

        // move the player to the location of the attached object iff the previous scene was _prevScene
        if(ScenesManager.GetPreviousScene() == _prevScene)
        {
            player.transform.position = gameObject.transform.position;
        }
    }
}
