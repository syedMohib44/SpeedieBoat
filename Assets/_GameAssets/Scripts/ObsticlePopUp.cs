using SpeedyBoat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAnalyticsSDK;

public class ObsticlePopUp : MonoBehaviour
{
    private float popUpTime = 5;
    private bool isReacged;
    private GameObject popUps;
    void Start()
    {
        GameAnalytics.Initialize();
    }
    // Update is called once per frame
    void Update()
    {
        popUpTime -= Time.deltaTime;
        if (isReacged == false)
        {
            this.transform.localPosition += new Vector3(0, 2 * Time.deltaTime, 0);
        }

        if (isReacged)
        {
            //popUpTime += Time.deltaTime;
            this.transform.position -= new Vector3(0, 2 * Time.deltaTime, 0);
        }

        if (transform.localPosition.y >= 1f && isReacged == false)
        {
            isReacged = true;
        }

        if (transform.localPosition.y <= -1f)
        {
            if (popUpTime <= 0)
            {
                isReacged = false;
                popUpTime = 5;
            }
        }
    }
}
