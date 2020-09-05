using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Animator))]
public class GameManager : MonoBehaviour
{
    static public GameManager instance { get; private set; } = null;

    public string levelName = "Level";

    public int score { get; private set; } = 0;

    Animator animator;

    void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        animator = GetComponent<Animator>();
    }

    public void Reset()
    {
        StartCoroutine(ResetCoroutine());
    }

    public void IncreaseScore(int amount = 1)
    {
        score += amount;
    }

    IEnumerator ResetCoroutine()
    {
        animator.SetTrigger("Fade");
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(levelName);
    }
}
