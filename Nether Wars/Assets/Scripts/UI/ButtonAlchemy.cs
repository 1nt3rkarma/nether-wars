using System.Collections.Generic;
using UnityEngine;

public class ButtonAlchemy : MonoBehaviour
{
    public Animator animator;
    public AudioSource audioSource;
    public List<AudioClip> ingredientSounds;

    public void AddBlood()
    {
        Alchemy.AddElement(Elements.blood);
        animator.SetTrigger("animate");
        PlaySound();
    }
    public void RemoveBlood()
    {
        Alchemy.RemoveElement(Elements.blood);
        animator.SetTrigger("animate");
    }

    public void AddBone()
    {
        Alchemy.AddElement(Elements.bone);
        animator.SetTrigger("animate");
        PlaySound();
    }
    public void RemoveBone()
    {
        Alchemy.RemoveElement(Elements.bone);
        animator.SetTrigger("animate");
    }

    public void AddCinder()
    {
        Alchemy.AddElement(Elements.cinder);
        animator.SetTrigger("animate");
        PlaySound();
    }
    public void RemoveCinder()
    {
        Alchemy.RemoveElement(Elements.cinder);
        animator.SetTrigger("animate");
    }

    public void AddSlime()
    {
        Alchemy.AddElement(Elements.slime);
        animator.SetTrigger("animate");
        PlaySound();
    }
    public void RemoveSlime()
    {
        Alchemy.RemoveElement(Elements.slime);
        animator.SetTrigger("animate");
    }

    public void AddShadow()
    {
        Alchemy.AddElement(Elements.shadow);
        animator.SetTrigger("animate");
        PlaySound();
    }
    public void RemoveShadow()
    {
        Alchemy.RemoveElement(Elements.shadow);
        animator.SetTrigger("animate");
    }

    void PlaySound()
    {
        int c = ingredientSounds.Count;
        if (c > 0)
        {
            int rand = Random.Range(0, ingredientSounds.Count);
            audioSource.PlayOneShot(ingredientSounds[rand]);
        }
    }
}
