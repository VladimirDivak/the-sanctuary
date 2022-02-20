using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class UIBestIdicators : MonoBehaviour
{
    [SerializeField] Text bestTime;
    [SerializeField] Text bestScore;

    public void SetData(string time, string score)
    {
        bestTime.text = time;
        bestScore.text = score;
    }
}
