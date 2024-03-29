using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour, IUnique
{
    //Object References
    private Animator animator;
    private Rigidbody2D body;
    private AttackManager am;
    private WeaponManager wm;
    private EffectsManager fxm;
    private Player_Movement pm;
    private Hurtbox hb;
    public static GameObject OnePlayer;
    private GameManager gm;

    //Death Handling
    private bool killPlayer = false;
    private float DeathDelayTimer;
    private float MAX_DEATH_TIMER = 1.5f;

    //Projectile Variables
    public int MaxAmmo;
    public GameObject bullet;
    public GameObject knife;
    private int Ammo;

    //cancel: "did player press?"
    //ShootAgain: "is my window open?"
    [HideInInspector]
    public bool cancel = false;
    [HideInInspector]
    public bool ShootAgain = false;

    //Variables to handle bug that sees you locked in place (very scuffed)
    private bool ShootEnd = true;
    private float ShootEndTimer;
    private float MAX_SHOOT_TIMER = 10f;

    //invincibillity after hit
    private float MAX_INVIN_TIME = 0.75f;
    private float InvinTimer;
    private GameObject[] wpnObjs;
    //METHODS THAT TIMELINE CAN ACCESS
    public void DisableHealthUI()
    {
        UIManager.DisableHealthUI();
    }
    public void EnableHealthUI()
    {
        UIManager.EnableHealthUI();
    }
    public void DisableWeaponUI()
    {
        WeaponUI.DisableWeaponUI();
    }
    public void EnableWeaponUI()
    {
        WeaponUI.EnableWeaponUI();
    }
    public void DefaultFlip()
    {
        GetComponent<Player_Movement>().flipped = false;
    }
    public void OtherFlip()
    {
        GetComponent<Player_Movement>().flipped = true;
    }
    public void TurnAround()
    {
        GetComponent<Player_Movement>().flipped = !GetComponent<Player_Movement>().flipped;
    }
    public void Dequip()
    {
        wm.equiped = false;
        animator.SetBool("equiped", false);
    }
    public void EffectManager(string funct)
    {
        //call function via string reference
        if (funct == "") return;
        Invoke(funct, 0f);
    }
    public void StartShoot()
    {
        am.ScriptActivate(AttackManager.ScriptTypes.Movement);
        if (Ammo > 0 && animator.GetFloat("shooting") == 0)
        {
            EntityManager.DisableEquip();
            EntityManager.DisableSwap();
            animator.SetFloat("shooting", 1);
            ShootAgain = false;
            cancel = false;
        }
        //animator.SetFloat("attack", 0);
    }
    public void KnifeThrow()
    {
        int turn = animator.GetBool("flipped") ? 1 : -1;
        GameObject proj = Instantiate(knife, transform.Find("Right Arm").Find("On-Hand").transform.position - new Vector3((-1 * turn), 0, 0), Quaternion.identity);
        Bullet bulScript = proj?.GetComponent<Bullet>();
        if (bulScript != null)
        {
            proj.GetComponent<AttackManager>().ProjectileOwner = gameObject;
            bulScript.InitBullet(gameObject);
        }
    }
    public void Shoot()
    {
        if (Ammo > 0)
        {
            Ammo -= 1;
            int turn = animator.GetBool("flipped") ? 1 : -1;
            GameObject proj = Instantiate(bullet, transform.Find("Right Arm").Find("On-Hand").transform.position - new Vector3((-1 * turn), 0, 0), Quaternion.identity);
            Bullet bulScript = proj?.GetComponent<Bullet>();
            if (bulScript != null)
            {
                proj.GetComponent<AttackManager>().ProjectileOwner = gameObject;
                bulScript.InitBullet(gameObject);
            }
        }
    }
    public void UnShoot()
    {
        if (Ammo > 0)
        {
            EntityManager.EnableSwap();
            EntityManager.EnableEquip();
            animator.SetFloat("shooting", 0);
            am.OffsenseDamageMultiplier = 1;
        }
        else
        {
            //disable movement and set to reload
            am.ScriptDeactivate(AttackManager.ScriptTypes.Movement);
            animator.SetFloat("shooting", 2);
            animator.Play("Buffer", 1);

            //just in case rare bug happens
            ShootEnd = false;
            ShootEndTimer = MAX_SHOOT_TIMER;
        }
        
    }
    public void Immobilize()
    {
        am.ScriptDeactivate(AttackManager.ScriptTypes.Movement);
    }
    //set variables associated with reload animation
    public void CleanShoot()
    {
        am.ScriptActivate(AttackManager.ScriptTypes.Movement);
        EntityManager.EnableSwap();
        EntityManager.EnableEquip();
        animator.SetFloat("shooting", 0);
        animator.Play("Idle", 1);
        Ammo = MaxAmmo;
        ShootEnd = true;
        cancel = false;
    }
    public void CancelShoot()
    {
        ShootAgain = true;
    }
    private void Sword1()
    {
        bool flipped = animator.GetBool("flipped");
        Vector3 move = new Vector3(30, 0, 0);
        if (flipped) move *= -1;
        body.AddForce(move, ForceMode2D.Impulse);
    }
    private void GunShake()
    {
        if (fxm != null) fxm.ShakeCam(0.1f, 0.75f);
    }
    private void QuakeShake()
    {
        if (fxm != null) fxm.ShakeCam(0.1f, 0.75f);
    }
    private void Tackle(int speed)
    {
        Vector3 temp = pm.direction();
        //bool flipped = animator.GetBool("flipped");
        Vector3 move = temp.normalized * speed;
        //if (flipped) move *= -1;
        body.AddForce(move);
    }
    private void XTackle(int speed)
    {
        bool flipped = animator.GetBool("flipped");
        Vector3 move = new Vector3(1, 0, 0) * speed;
        if (flipped) move *= -1;
        body.AddForce(move);
    }

    public void onDeath()
    {
        //Debug.Log("You've died!");

        //disable all scripts
        Hurtbox hrt = gameObject?.GetComponent<Hurtbox>();
        IScriptable[] list = hrt != null ? hrt.scriptableScripts : GetComponents<IScriptable>();
        //reset projectile variables
        animator.SetLayerWeight(1, 0);
        animator.SetFloat("shooting", 0);
        //disable all your inputs
        EntityManager.DisableSwap();
        EntityManager.DisableEquip();
        EntityManager.DisableAttack();

        //disable own collider on Body
        transform.Find("Body").GetComponent<BoxCollider2D>().enabled = false;

        foreach (IScriptable script in list)
            script.ScriptHandler(false);

        //play death animation
        animator.Play("Death");
    }

    //AnimatorOnly Function called when death animation finishes
    public void AnimatorPlayerDeath()
    {
        //set flag to freeze game and pull up Death UI
        killPlayer = true;
        DeathDelayTimer = MAX_DEATH_TIMER;
    }

    public void HitStunAni()
    {
        //if current clip is death don't play below
        //GetComponent<AttackManager>().DestroyPlay();
        //TEMPORARY
        animator.SetBool("Skid", false);
        animator.SetBool("Hitstun", true);
        animator.Play("Hitstun");
        InvinTimer = MAX_INVIN_TIME;
        //getting hit while reloading (reload for you then transition)
        if (animator.GetFloat("shooting") > 0) CleanShoot();
    }

    void Start()
    {
        wpnObjs = GameObject.FindGameObjectsWithTag("Weapon");
        //force On-Hand to be first
        if (wpnObjs[0].name != "On-Hand")
        {
            GameObject onHand = wpnObjs[0];
            int index = 0;
            for (int i = 1; i < wpnObjs.Length; i++)
            {
                if (wpnObjs[i].name == "On-Hand")
                {
                    onHand = wpnObjs[i];
                    index = i;
                    break;
                }
            }
            wpnObjs[index] = wpnObjs[0];
            wpnObjs[0] = onHand;
        }
        body = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.GetComponent<Animator>();
        wm = gameObject.GetComponent<WeaponManager>();
        am = gameObject.GetComponent<AttackManager>();
        Ammo = MaxAmmo;
        fxm = FindObjectOfType<EffectsManager>();
        pm = gameObject.GetComponent<Player_Movement>();
        hb = gameObject.GetComponent<Hurtbox>();
        //GameObject gmo = GameObject.Find("GameManager");
        GameObject gmo;
        if (GameManager.OneGM != null)
            gmo = GameManager.OneGM;
        else
        {
            gmo = Instantiate(Resources.Load("Prefabs/GameManager")) as GameObject;
            gmo.name = "GameManager";
        }
            
        gm = gmo.GetComponent<GameManager>();
    }

    private void InvinClock()
    {
        //for player invincibillity (usually right after they get up from a hit)
        if (InvinTimer > 0)
        {
            InvinTimer -= Time.deltaTime;
            hb.invin = true;
            hb.InvokeFlash(0.1f, new Color(0.39f, 0.59f, 1f), false, false, 100, 0.1f);
        }
        if (InvinTimer <= 0 && hb.invin)
        {
            hb.invin = false;
            hb.CancelFlash();
        }
    }
    // Update is called once per frame
    void Update()
    {
        //fix bug where shoot value is active but weapon is not gun
        if (animator.GetFloat("shooting") > 0 && wm.wpnList.index != 2)
        {
            UnShoot();
        }
        //play death animation when dead if haven't already (fixes bug that plays idle_engage before death can start playing)
        if ((animator.GetCurrentAnimatorClipInfo(0).Length > 0 && animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != "Death") && GetComponent<HealthTracker>().healthSystem.getHealth() <= 0)
            onDeath();

        //if flag is true and HP bar is all the way down
        if (killPlayer && GetComponent<HealthTracker>().GetTrueFillAmount() <= 0)
        {
            if(DeathDelayTimer > 0)
                DeathDelayTimer -= Time.deltaTime;
            if(DeathDelayTimer <= 0)
            {
                EntityManager.PlayerDied();
                gm.ResetManager();
                killPlayer = false;
            }
                
        }

        //This code fixes a rare bug that locks player in place after shooting
        if (!ShootEnd && ShootEndTimer > 0) ShootEndTimer -= Time.deltaTime;
        if(!ShootEnd && ShootEndTimer <= 0)
        {
            CleanShoot();
        }

        //if detect cancel
            if(cancel && ShootAgain)
        {
            cancel = false;
            ShootAgain = false;
            UnShoot();
            am.InvokeAttack(5);
        }

        //track if you've picked up heart
        if(InventoryManager.HasItem(InventoryManager.AllItems.HealingHeart))
        {
            InventoryManager.RemoveItem(InventoryManager.AllItems.HealingHeart);
            GetComponent<HealthTracker>().healthSystem.Heal(15);
            hb.InvokeFlash(0.15f, new Color(0, 1f, 0), false);
        }

        if (InventoryManager.HasItem(InventoryManager.AllItems.Full_Heart))
        {
            InventoryManager.RemoveItem(InventoryManager.AllItems.Full_Heart);
            GetComponent<HealthTracker>().healthSystem.Heal(100);
            hb.InvokeFlash(0.15f, new Color(0.77f, 0.71f, 0.33f), true);
        }

        InvinClock();

    }
    void LateUpdate()
    {
        bool flipped = animator.GetBool("flipped");
        int temp = flipped ? 180 : 0;

        //update the Weapon tag to rotate correctly (all will be animated the same)
        
        foreach (GameObject wpn in wpnObjs)
        {
            GameObject parent = wpn.transform.parent.gameObject;
            //flip and flip to default
            if (wpn.transform.rotation.eulerAngles.y < 180 && flipped)
                wpn.transform.RotateAround(parent.transform.position, Vector3.up, 180);
            else if (wpn.transform.rotation.eulerAngles.y >= 180 && !flipped)
                wpn.transform.RotateAround(parent.transform.position, Vector3.up, 0);
        }
    }
}
