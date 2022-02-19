using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

public class BallsRack : MonoBehaviour
{
    [SerializeField] PlayerBall playerBall;
    [Space]
    public bool useLastBall;
    [SerializeField] Texture2D lastBallTexture;
    [Space]
    public int activeBalls;
    [SerializeField] GameObject[] balls;

    [SerializeField] public UnityEvent OnRackIsEmpty;

    public int ballsCounter => balls.Count(x => x.activeSelf);

    void Start()
    {
        balls = new GameObject[transform.childCount];
        for(int i = 0; i < transform.childCount; i++)
        {
            GameObject currentBall = transform.GetChild(i).gameObject;
            balls[i] = currentBall;

            if(i == activeBalls - 1 && useLastBall)
            {
                balls[i].GetComponent<MeshRenderer>()
                    .material
                    .SetInt(playerBall.propIsLastBall, 1);
            }
            
            if(i > activeBalls - 1) balls[i].SetActive(false);
        }
    }

    public async void GetBall()
    {
        if(ballsCounter == 0 ||
        PlayerController.Instance.currentBall != null ||
        Vector3.Distance(PlayerController.Instance.position, transform.position) > 2f)
        {
            return;
        }

        var newBall = Instantiate(playerBall, transform.position + Vector3.up * 1.5f, Quaternion.identity);
        balls.First(x => x.activeSelf).SetActive(false);

        await Task.Delay(10);

        if(ballsCounter == 0 && useLastBall) newBall.SetLastBallTexture(1);
        PlayerController.Instance.currentBall = newBall;
        newBall.PhysicDisable();
    }
}
