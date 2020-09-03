using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] GameManager game = default;
    [SerializeField] TextMeshProUGUI text = default;

    void Update()
    {
        text.text = $"{game.score}";
    }
}
