using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{
    public Rigidbody Rb;

    private float lifetime = 5f;

    private Material material;
    private Color originalColour;
    private float fadePercent;
    private float deathTime;

    private bool fading;

    void Start()
    {
        material = GetComponent<Renderer>().material;
        originalColour = material.color;
        deathTime = Time.time + lifetime;

        StartCoroutine ("Fade");
    }

    IEnumerator Fade()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);

            if(fading)
            {
                fadePercent += Time.deltaTime;
                material.color = Color.Lerp(originalColour, Color.clear, fadePercent);
                if(fadePercent >= 1)
                {
                    Destroy(gameObject);
                }
            }

            else if(Time.time > deathTime)
            {
                fading = true;
            }
        }
    }

    void OnTriggerEnter(Collider c)
    {
        if(c.tag == "Ground")
        {
            Rb.isKinematic = false;
        }
    }
}
