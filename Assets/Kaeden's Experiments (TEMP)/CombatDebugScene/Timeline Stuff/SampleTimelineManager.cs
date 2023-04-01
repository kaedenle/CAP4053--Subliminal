using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

public class SampleTimelineManager : MonoBehaviour
{
    private int step = -1;
    private int dummy;
    public PlayableDirector[] eventList;
    private GameObject player;
    private PlayerMetricsManager pmm;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("player");
        pmm = PlayerMetricsManager.GetManager();
        EntityManager.DisableMovement();
        EntityManager.DisableAttack();
    }
    public void EnablePlayer()
    {
        EntityManager.EnableAttack();
        EntityManager.EnableMovement();
    }
    public void PlayEvent()
    {
        if (step >= eventList.Length) return;
        if (step >= 0)
            eventList[step].Play();
        step++;
        PresetVariables();
    }
    private void PresetVariables()
    {
        if (step == 0) dummy = pmm.GetMetricInt("killed");
    }
    private void CheckCondition()
    {
        //if player killed one thing
        Debug.Log(pmm.GetMetricInt("killed"));
        if (step == 0 && dummy < pmm.GetMetricInt("killed"))
            PlayEvent();
    }
    // Update is called once per frame
    void Update()
    {
        CheckCondition();
    }
}
