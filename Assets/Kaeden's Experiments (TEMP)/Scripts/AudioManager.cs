using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    //Audio
    public AudioClip[] AttackAudio;  
    public AudioSource myaudiosrc;
    public void PlayAudio(int index)
    {
        if (index >= AttackAudio.Length) return;
        if (myaudiosrc != null) myaudiosrc.PlayOneShot(AttackAudio[index], 1);
    }
    public void StopAudio()
    {
        myaudiosrc.Stop();
    }
    // Start is called before the first frame update
    void Start()
    {
        myaudiosrc = gameObject.GetComponent<AudioSource>();
    }
}
