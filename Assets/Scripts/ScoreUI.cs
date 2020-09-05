using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text = default;

    void Update()
    {
        var score = GameManager.instance.score;
        text.text = $"{score}";
    }
}
