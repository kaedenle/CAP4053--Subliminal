using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WeaponManager : MonoBehaviour, IScriptable
{

    public Sprite[] spriteList;
    public int weaponID;
    public SpriteRenderer sr;
    private Animator animator;
    public bool equiped;
    public AttackManager.WeaponList wpnList = new AttackManager.WeaponList();
    public Hurtbox hrtbx;
    private SpriteRenderer onhand;
    public int BufferWeaponID = 0;
    // Start is called before the first frame update

    void Start()
    {
        weaponID = 0;
        animator = gameObject.GetComponent<Animator>();
        wpnList = gameObject.GetComponent<AttackManager>().wpnList;
        hrtbx = gameObject?.GetComponent<Hurtbox>();
        onhand = gameObject.transform.Find("Right Arm").Find("On-Hand").GetComponent<SpriteRenderer>();
    }
    
    //scripts to be disabled/enabled when attacking
    public void ScriptHandler(bool flag)
    {
        //this.enabled = flag;
    }
    //what happens when disable
    public void EnableByID(int ID)
    {
        if (ID == 1)
            this.enabled = true;
    }
    public void DisableByID(int ID)
    {
        if (ID == 1)
            this.enabled = false;
    }

    public void SetSprite()
    {
        //set the visual of the weapon from sprite list
        sr.sprite = spriteList[wpnList.index % spriteList.Length];
    }

    // Update is called once per frame
    void Update()
    {
        equiped = animator.GetBool("equiped");

        //toggle between not equiped and equiped
        if(InputManager.EquipKeyDown() && animator.GetFloat("attack") == 0){
            animator.SetBool("equiped", !equiped);
            //set animation to follow weapon's ID
            animator.SetFloat("weapon", wpnList.weaponlist[wpnList.index].ID);
        }
            
        //disable weapon from rendering if it's not out  
        if (!equiped)
        {
            onhand.enabled = false;
            sr.sprite = null;
        }
        else if(equiped){
            onhand.enabled = true;
            if (InputManager.SwapKeyDown())
            {
                wpnList.index += 1;
                wpnList.index %= wpnList.weaponlist.Length;
                //change speed if heavy weapon
                Player_Movement movementScript = gameObject.GetComponent<Player_Movement>();
                if (weaponID == 1)
                    gameObject.GetComponent<Player_Movement>().speed = 10;
                else
                    gameObject.GetComponent<Player_Movement>().speed = movementScript.MAX_SPEED;
            }

            //only swap if not attacking
            if (animator.GetFloat("attack") == 0)
            {
                SetSprite();
                //set animation to follow weapon's ID
                if (animator.GetFloat("weapon") != wpnList.weaponlist[wpnList.index].ID)
                    animator.SetFloat("weapon", wpnList.weaponlist[wpnList.index].ID);
            }

        }

        //input for the player
        if(equiped){
            //make it move cancellable
            if(gameObject.GetComponent<Player_Movement>().move_flag && animator.GetFloat("movement") > 0 && animator.GetFloat("attack") > 0){
                gameObject.GetComponent<AttackManager>().DestroyPlay();
                gameObject.GetComponent<Player_Movement>().move_flag = false;
            }


            bool hitStunVar = hrtbx != null ? hrtbx.inHitStun : false;
            //if in hitstun, don't read input (put pausing mechanics in here)
            if (!hitStunVar)
            {
                //Swing when press left click
                if (InputManager.Hit1KeyDown() && wpnList.weaponlist[wpnList.index].attack1 != 0)
                {
                    //special case for shooting
                    if(animator.GetFloat("shooting") > 0)
                    {
                        PlayerScript ps = GetComponent<PlayerScript>();
                        if (ps.ShootAgain)
                            ps.cancel = true;
                    }
                    //call attack from integer (defined by Attack blend tree in animator)
                    else if (animator.GetFloat("attack") != 0)
                    {
                        BufferWeaponID = wpnList.index;
                        GetComponent<AttackManager>().bufferCancel = wpnList.weaponlist[wpnList.index].attack1;
                    }   
                    else
                        GetComponent<AttackManager>().InvokeAttack(wpnList.weaponlist[wpnList.index].attack1);

                    //damage self by 5 points
                    //gameObject.GetComponent<HealthTracker>().healthSystem.Damage(5);
                }
                //Swing when press right click
                else if (InputManager.Hit2KeyDown() && wpnList.weaponlist[wpnList.index].attack2 != 0 && animator.GetFloat("shooting") == 0)
                {
                    //call attack from integer (defined by Attack blend tree in animator)
                    if (animator.GetFloat("attack") != 0)
                    {
                        BufferWeaponID = wpnList.index;
                        GetComponent<AttackManager>().bufferCancel = wpnList.weaponlist[wpnList.index].attack2;
                    }
                    else
                        GetComponent<AttackManager>().InvokeAttack(wpnList.weaponlist[wpnList.index].attack2);
                }
                if (animator.GetFloat("attack") != 0 && animator.GetBool("Attack") == false)
                {
                    animator.SetTrigger("Attack");
                }
            }   
        }

        if (animator.GetFloat("attack") == 0 && animator.GetBool("Attack") == true)
            animator.ResetTrigger("Attack");
        
    }

    
}
