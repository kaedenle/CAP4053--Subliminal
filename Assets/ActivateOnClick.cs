using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateOnClick : MonoBehaviour
{
    public GameObject LoadButton;
    public GameObject DifficultySelect;
    public MenuManager CreateNewGame;
    // Update is called once per frame
    public void Activate()
    {
        if(LoadButton != null && GameData.GetInstance().HasLoadData())
            this.gameObject.SetActive(true);
        else
        {
           CreateNewGame.NewGame();
           DifficultySelect.SetActive(true);
        }
        
    }
}
