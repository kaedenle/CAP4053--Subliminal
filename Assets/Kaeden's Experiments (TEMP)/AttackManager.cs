using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//THIS SCRIPT IS A UNIVERSAL SCRIPT THAT GENERATES HITBOXES ACCORDING TO DATA GIVEN IT
//THIS SCRIPT WILL NOT DETERMINE WHICH ATTACK CAN BE DONE. THAT HAS TO BE GIVEN TO IT VIA StartPlay(int moveIndex) AND moveContainer
//THIS SCRIPT SIMPLY PLAYS HITBOX ANIMATIONS. IT IS NOT A DECIDER
public class AttackManager : MonoBehaviour, IScriptable
{
    //Weapon interfacing (not used in this file but offered for others to interface with)
    public TextAsset weaponList;
    [System.Serializable]
    public class WeaponList
    {
        public Weapon[] weaponlist;
        public int index;
    }
    public WeaponList wpnList = new WeaponList();

    //Components
    private Animator animator;
    [HideInInspector]
    public GameObject hitboxParent;
    

    //Component Lists/Arrays
    [HideInInspector]
    public List<Hitbox> HBList = new List<Hitbox>();
    private IDictionary<int, List<Attack>> frames = new Dictionary<int, List<Attack>>();
    private IDictionary<Collider2D, Hitbox> hasHit = new Dictionary<Collider2D, Hitbox>();
    private HashSet<Collider2D> alreadyDamaged = new HashSet<Collider2D>();

    //technical information
    public bool active;
    private IUnique uniqueScript;

    //framedata
    public TextAsset[] moveContainer;
    public enum ScriptTypes{
        Movement,
        Attacking
    }

    [System.Serializable]
    public class FrameData
    {
        //numeral data
        public Attack[] framedata;
        public int damage;
        public int knockback;
        public int hitstop;
        public float hitstun;
        public float x_knockback;
        public float y_knockback;

        //move info
        public string hitsTag;      //can make this into an array
        public string[] cancelBy;

        //on hit, deactivate move or hitbox?
        public bool deactivateMove;
        public bool relativeKnockback;

        public string functCall;
        
    }
    public FrameData framedata = new FrameData();

    // Start is called before the first frame update
    void Start()
    {
        active = false;
        HBList.Clear();
        uniqueScript = GetComponent<IUnique>();
        animator = GetComponent<Animator>();
        //variable for others to grab
        if(weaponList != null)
            wpnList = JsonUtility.FromJson<WeaponList>(weaponList.text);
        //atk = AttackID.Sword1;
    }
//-----------------------------HITBOX FUNCTIONS----------------------------------------------------------
    private void CreateHitbox(Attack a){
        GameObject HitboxPrefab = Resources.Load("Prefabs/Hitboxes") as GameObject;
        GameObject parent = hitboxParent;
        if(hitboxParent == null)
            parent = gameObject;
        GameObject createdHitbox = Instantiate(HitboxPrefab);
        createdHitbox.name = "Hitbox";
        createdHitbox.transform.SetParent(parent.transform);
        
        Hitbox HBObj = createdHitbox.GetComponent<Hitbox>();
        HBObj.Atk = a;
        HBList.Add(HBObj);
    }

    private void DestroyAllHitboxes(){
        int HBListLength = HBList != null ? HBList.Count : 0;
        for(int i = 0; i < HBListLength; i++){
            Hitbox hb = HBList[i];
            Destroy(hb.gameObject);
        }
        HBList.Clear();
    }

    private void UpdateHitboxInfo(Hitbox hb, Attack atk){
        hb.SetAuxillaryValues(framedata.hitstop, framedata.hitsTag, framedata.cancelBy, framedata.relativeKnockback, framedata.functCall);
        //set default value (when frame's damage/knockback is 0)
        atk.damage = atk.damage == 0 ? framedata.damage : atk.damage;
        atk.knockback = atk.knockback == 0 ? framedata.knockback : atk.knockback;
        atk.hitstun = atk.hitstun == 0 ? framedata.hitstun : atk.hitstun;
        atk.x_knockback = atk.x_knockback == 0 ? framedata.x_knockback : atk.x_knockback;
        atk.y_knockback = atk.y_knockback == 0 ? framedata.y_knockback : atk.y_knockback;
        hb.Atk = atk;
        //set position and scale, then fix rotation
        hb.gameObject.transform.localPosition = new Vector3(hb.Atk.x_pos, hb.Atk.y_pos, 0);
        hb.gameObject.transform.localScale = new Vector3(hb.Atk.x_scale, hb.Atk.y_scale, 0);
        hb.gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
        hb.Activate();
    }

    private void ProvisionHitboxes(int frameCount){
        if (!frames.ContainsKey(frameCount))
            return;
        int counter = 0;
        int HBListLength = HBList != null ? HBList.Count : 0;
        foreach(Attack a in frames[frameCount])
        {
            //create hitboxes if need more
            if (counter >= HBListLength)
                CreateHitbox(a);
            UpdateHitboxInfo(HBList[counter], a);
            counter += 1;
        }

        //update length of HBList
        HBListLength = HBList != null ? HBList.Count : 0;
        //destroy extra hitboxes
        for(int i = counter; i < HBListLength; i++){
            Hitbox hb = HBList[i];
            HBList.RemoveAt(i);
            Destroy(hb.gameObject);
        }
        //Debug.Log("CURRENT: " + currentFrame);
        CallHitboxes();
    }

    private void CallHitboxes()
    {
        //update each hitbox
        if (active)
        {
            //object that hit something
            //Hitbox current = null;
            hasHit.Clear();
            //have you hit something?
            foreach (Hitbox box in HBList)
            {
                box.updateHitboxes();
                if (box._state == Hitbox.ColliderState.Colliding)
                {
                    foreach(Collider2D entity in box.collidersList)
                    {
                        //if hitbox can't hit tag ignore
                        if (!entity.gameObject.tag.Equals(box.hitsTag))
                            continue;
                        //if already hit within swing ignore
                        if (alreadyDamaged.Contains(entity))
                            continue;

                        //add to hasHit dictionary for quick entity access
                        if(!hasHit.ContainsKey(entity))
                            hasHit.Add(entity, box);
                        //replace hitbox hitting entity if ID overrides exising box ID
                        if (hasHit[entity].Atk.ID > box.Atk.ID)
                            hasHit[entity] = box;
                    }
                }
            }
            //apply hit to all entities in hasHit
            foreach(var entity in hasHit.Keys)
            {
                GameObject effect = HurtBoxSearch(entity.gameObject);
                if (hasHit[entity].hitEntity(effect))
                {
                    Debug.Log(hasHit[entity].Atk.ID + " has hit " + effect.name);
                    alreadyDamaged.Add(entity);
                }
                    
            }
        }
    }
    public GameObject HurtBoxSearch(GameObject part){
        Hurtbox check = null;
        while(check == null){
            check = part?.GetComponent<Hurtbox>();
            if(part.transform?.parent == null)
                return part;
            if(check == null)
                part = part.transform.parent.gameObject;
        } 
        return part;
    } 
    //-----------------------------PLAY ATTACK FUNCTIONS----------------------------------------------------------
    public void StartPlay(int moveIndex){
        //get current animation to keep track of current animation frame (attach hitboxes to animation)

        //get framedata
        framedata = JsonUtility.FromJson<FrameData>(moveContainer[moveIndex & moveContainer.Length].text);

        //quickly load framedata into frames (hashmap that loads into hitbox)
        foreach(Attack a in framedata.framedata)
        {
            if (!frames.ContainsKey(a.frame))
                frames[a.frame] = new List<Attack>();
            frames[a.frame].Add(a);
        }
        //tell hitboxes to update
        active = true;
        ProvisionHitboxes(0);
        //apply function effect (on first frame of attack)
        if(framedata.functCall != null || framedata.functCall == "")
            uniqueScript.EffectManager(framedata.functCall);
    }

    public void StopPlay(){
        if(framedata.deactivateMove)
            active = false;
        //temporary, turn off hitboxes or delete hitboxes here
        foreach (Hitbox box in HBList)
            box.Deactivate();
    }

    public void DestroyPlay(){
        active = false;
        ScriptToggle(1);
        frames.Clear();
        DestroyAllHitboxes();
        
        //clear history of what you've hit
        alreadyDamaged.Clear();
    }
    //-----------------------------ISCRIPTABLE FUNCTIONS----------------------------------------------------------
    public void ScriptHandler(bool flag)
    {
        //DANGEROUS, could recurse and crash game without && active
        if (!flag && active)
        {
            DestroyPlay();
        }
    }
    public void EnableByID(int ID)
    {
        if(ID == 2)
        {

        }
    }
    public void DisableByID(int ID)
    {
        if(ID == 2)
        {
            DestroyPlay();
        }
    }
    //-----------------------------SCRIPT INTERFACE FUNCTIONS----------------------------------------------------------
    public void ScriptToggle(int flag){
        bool ret = flag > 0 ? true : false;
        IScriptable[] scripts = gameObject?.GetComponent<Hurtbox>().scriptableScripts;
        if (scripts == null) scripts = gameObject.GetComponentsInChildren<IScriptable>();

        foreach (IScriptable s in scripts){
            s.ScriptHandler(ret);
        }
    }

    public void ScriptActivate(ScriptTypes ID){
        IScriptable[] scripts = gameObject?.GetComponent<Hurtbox>().scriptableScripts;
        if (scripts == null) scripts = gameObject.GetComponentsInChildren<IScriptable>();

        foreach (IScriptable s in scripts){
            s.EnableByID((int)ID);
        }
    }

    public void ScriptDeactivate(ScriptTypes ID)
    {
        IScriptable[] scripts = gameObject?.GetComponent<Hurtbox>().scriptableScripts;
        if (scripts == null) scripts = gameObject.GetComponentsInChildren<IScriptable>();

        foreach (IScriptable s in scripts)
        {
            s.DisableByID((int)ID);
        }
    }
    //-----------------------------HELPER FUNCTIONS----------------------------------------------------------
    private int GetAnimationFrame(){
        AnimatorClipInfo[] m_CurrentClipInfo = animator.GetCurrentAnimatorClipInfo(0);
        //(m_CurrentClipInfo[0].clip.name)
        AnimatorStateInfo animationInfo = animator.GetCurrentAnimatorStateInfo(0);
        float timeOfAnimation = GetComponent<Animator>().runtimeAnimatorController.animationClips[0].length;
        int ret = ((int)(animationInfo.normalizedTime % 1 * (m_CurrentClipInfo[0].clip.length * m_CurrentClipInfo[0].clip.frameRate)));
        return ret;
    }
    
    // Update is called once per frame
    void Update()
    {
        //Debug.Log(Sword1.var);
        CallHitboxes();
    }
}
