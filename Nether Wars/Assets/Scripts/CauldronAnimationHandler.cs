using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CauldronAnimationHandler : MonoBehaviour
{
    public Color bloodColor;
    public Color boneColor;
    public Color cinderColor;
    public Color slimeColor;
    public Color shadowColor;

    public MeshRenderer liquidRenderer;
    public ParticleSystemRenderer particleRenderer;
    public Animator animator;

    static CauldronAnimationHandler local;

    void Start()
    {
        local = this;
    }

    IEnumerator ChangeLiquidColor(Color newColor, float duration)
    {
        float timer = 0;
        Color oldColor = liquidRenderer.material.color;
        float deltaR = newColor.r - oldColor.r; float speedR = deltaR / duration;
        float deltaG = newColor.g - oldColor.g; float speedG = deltaG / duration;
        float deltaB = newColor.b - oldColor.b; float speedB = deltaB / duration;

        Color tempColor = oldColor;
        while (timer < duration)
        {
            tempColor.r += speedR * Time.deltaTime;
            tempColor.g += speedG * Time.deltaTime;
            tempColor.b += speedB * Time.deltaTime;

            liquidRenderer.material.color = tempColor;
            particleRenderer.material.color = tempColor;
            timer += Time.deltaTime;
            yield return null;
        }
        liquidRenderer.material.color = newColor;
        particleRenderer.material.color = newColor;
    }

    public static void Animate()
    {
        local.animator.SetTrigger("stir");
    }
}
