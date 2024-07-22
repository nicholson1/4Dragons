using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarknessController : MonoBehaviour
{
    [SerializeField] private ParticleSystem LargeParticleSystem;
    [SerializeField] private GameObject DarknessObj;
    private Renderer[] renderers;
    private List<Material> materialToSwap = new List<Material>();
    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].materials.Length >= 2)
            {
                materialToSwap.Add(renderers[i].materials[1]);
                Material temp = renderers[i].material;
                renderers[i].material = materialToSwap[i];
                materialToSwap[i] = temp;
            }
            else
            {
                materialToSwap.Add(null);
            }
        }

    }

    public void Cleanse()
    {
        //Debug.Log("start cleanse");
        StartCoroutine(DisableDarkness(3.5f));
    }

    IEnumerator DisableDarkness(float wait)
    {
        yield return new WaitForSeconds(wait);
        LargeParticleSystem.Stop();
        DarknessObj.gameObject.SetActive(false);
        yield return new WaitForSeconds(2);
        for (int i = 0; i < materialToSwap.Count; i++)
        {
            if (renderers[i].materials.Length >= 2)
            {
                Material temp = renderers[i].material;
                renderers[i].material = materialToSwap[i];
                materialToSwap[i] = temp;
            }
        }
        GetComponent<Character>()._am.SetTrigger(TheSpellBook.AnimationTriggerNames.Cleanse.ToString());

        //todo activate another particle system blue burst or something
        //Debug.Log("finish cleanse");
    }
    
    

    
}
