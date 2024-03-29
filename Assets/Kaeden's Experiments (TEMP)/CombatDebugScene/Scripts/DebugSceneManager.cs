using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class DebugSceneManager : MonoBehaviour
{
    private GameObject player;
    private GameObject BlackFade;
    public GameObject book;
    public GameObject testTable;
    private void SceneInputs()
    {
        //fade in then reset level
        if ((!EntityManager.IsPaused() || player.GetComponent<HealthTracker>().healthSystem.getHealth() == 0) && Input.GetKeyDown(KeyCode.RightShift))
        {
            BlackFade.GetComponent<DontDestroy>().FadeIn();
        }    
        //kill yourself
        if (Input.GetKeyDown(KeyCode.K))
            player.GetComponent<HealthTracker>().healthSystem.Damage(10000);
        //open book animation
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            book.SetActive(true);
            book.GetComponent<BookUI>().ToggleBook();
        }
        if (Input.GetKeyDown(KeyCode.P))
            PlayerMetricsManager.ReturnKeeperInt();
        if (Input.GetKeyDown(KeyCode.O))
            UIManager.EnableHealthUI();
        if (Input.GetKeyDown(KeyCode.I))
            UIManager.DisableHealthUI();
        if (Input.GetKeyDown(KeyCode.U))
            testTable.SetActive(!testTable.activeSelf);

    }
    void Start()
    {
        //UIManager.DisableHealthUI();
        player = GameObject.Find("Player");
        BlackFade = GameObject.Find("BlackFade");
    }

    // Update is called once per frame
    void Update()
    {
        SceneInputs();
    }
}
