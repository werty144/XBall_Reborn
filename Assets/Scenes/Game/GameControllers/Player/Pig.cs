using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pig : MonoBehaviour
{
    public GameObject PigModel;
    public GameObject Skin;
    public void Piggiwise()
    {
        gameObject.GetComponent<GrabManager>().SetCooldownMillis(PigRules.PigDurationMillis);
        PigModel.SetActive(true);
        Skin.SetActive(false);
        
        Invoke(nameof(UnPiggiwise), PigRules.PigDurationMillis / 1000f);
    }

    void UnPiggiwise()
    {
        PigModel.SetActive(false);
        Skin.SetActive(true);
    }
}
