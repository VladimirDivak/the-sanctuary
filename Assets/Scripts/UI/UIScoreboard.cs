using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class UIScoreboard : MonoBehaviour
{
    [SerializeField] Text scores;
    [SerializeField] Text multiplier;

    private void OnEnable()
    {
        multiplier.gameObject.SetActive(false);
    }

    public void UpdateScores(int scoresValue, int multiplierValue)
    {
        scores.text = scoresValue.ToString();

        if(multiplierValue != 1)
        {
            multiplier.text = $"X{multiplierValue}";
            if(!multiplier.gameObject.activeSelf)
                multiplier.gameObject.SetActive(true);
        }
        else
        {
            if(multiplier.gameObject.activeSelf)
            multiplier.gameObject.SetActive(false);
        }
    }

    public void Reset()
    {
        scores.text = "0";
        multiplier.gameObject.SetActive(false);
    }
}
