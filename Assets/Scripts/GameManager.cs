using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public string levelName = "Level";

    [SerializeField] Animator animator;

    public void Reset()
    {
        StartCoroutine(ResetCoroutine());
    }

    public void OnReset(InputAction.CallbackContext context) => Reset();

    IEnumerator ResetCoroutine()
    {
        animator.SetTrigger("Fade");
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(levelName);
    }
}
