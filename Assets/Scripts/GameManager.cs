using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public string levelName = "Level";

    [SerializeField] Animator animator;

    public void Reset()
    {
        StartCoroutine(ResetCoroutine());
    }

    IEnumerator ResetCoroutine()
    {
        animator.SetTrigger("fade");
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(levelName);
    }
}
