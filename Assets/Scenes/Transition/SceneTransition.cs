using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public Animator Animator;

    public void LoadScene(string SceneName)
    {
        StartCoroutine(Go(SceneName));
    }

    IEnumerator Go(string SceneName)
    {
        Animator.SetTrigger("Start");
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(SceneName);
    }
}
