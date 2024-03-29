using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeEnemyScript : MonoBehaviour, IUnique
{
    private Animator animator;
    public void EffectManager(string funct)
    {
    }
    public void onDeath()
    {
        foreach (ItemDrop item in gameObject.GetComponents<ItemDrop>())
        {
            item.AttemptDrop();
        }

        HealthTracker healthTracker = GetComponent<HealthTracker>();
        //Debug.Log("Dead Ooze boy");
        PlayerMetricsManager.IncrementKeeperInt("killed");
        Destroy(healthTracker.bar.gameObject);
        Destroy(gameObject);
    }

    public void HitStunAni()
    {
        //TEMPORARY
        animator.Play("SlimeIdle");
    }
    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
