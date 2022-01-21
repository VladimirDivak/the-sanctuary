using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

public class BallsRack : MonoBehaviour
{
    [SerializeField]
    PlayerBall playerBall;
    [SerializeField]
    int activeBalls;
    [SerializeField]
    GameObject[] balls;
    [SerializeField]
    bool ableToUse = true;

    [SerializeField]
    public UnityEvent OnRackIsEmpty;

    public int ballsCounter { get => balls.Count(x => x.activeSelf); }

    void Start()
    {
        balls = new GameObject[transform.childCount];
        for(int i = 0; i < transform.childCount; i++)
        {
            GameObject currentBall = transform.GetChild(i).gameObject;
            balls[i] = currentBall;

            if(i > activeBalls - 1) balls[i].SetActive(false);
        }
    }

    public async void GetBall()
    {
        if(ballsCounter == 0 || PlayerController.Instance.currentBall != null)
        {
            return;
        }

        var newBall = Instantiate(playerBall, Vector3.zero + Vector3.up * 2, Quaternion.identity);
        balls.First(x => x.activeSelf).SetActive(false);
        await Task.Delay(25);
        
        PlayerController.Instance.currentBall = newBall;
        newBall.PhysicDisable();
    }
}
