using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBoxAttack : MonoBehaviour
{
    [SerializeField] private int _attackPower = 1;

    private bool can_attack;
    private float timeUntilNextAttack = 0.5f;
    private float tempTime;
    private void OnEnable()
    {
        can_attack = true;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
       Debug.Log("cringer");
        if(other.transform.name == "Player")
        {
            //Debug.Log("HITTERHITTER");
            var hit = other.GetComponent<HealthTracker>();
            if(hit != null && can_attack)
            {
                AttackData ad = new AttackData(_attackPower, 10, new Vector3(0, 0, 0));
                ad.setAux(10, 0, 0, null);
                hit.damage(ad);
                can_attack = false;
                tempTime = Time.time;
            }
        }
        else
        {
            Debug.Log("notplayer");
        }
        
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        hasEnoughTimePassed();
    }
    private void hasEnoughTimePassed()
    {
        if(Time.time >= tempTime + timeUntilNextAttack)
        {
            can_attack = true;
        }
    }
}
