using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Basics01
{
    public class Clock : MonoBehaviour
    {
        [SerializeField] Transform hoursPivot, minutesPivot, secondsPivot;
        const float hoursToDegrees = -30f, minutesToDegrees = -6f, secondsToDegrees = -6f;
        private void Awake()
        {
            Debug.Log(DateTime.Now);
            DateTime time = DateTime.Now;
            hoursPivot.localRotation = Quaternion.Euler(0f, 0f, hoursToDegrees * time.Hour);
            minutesPivot.localRotation = Quaternion.Euler(0f, 0f, minutesToDegrees * time.Minute);
            secondsPivot.localRotation = Quaternion.Euler(0f, 0f, secondsToDegrees * time.Second);
        }

        private void Update()
        {
            var time = DateTime.Now.TimeOfDay;
            hoursPivot.localRotation = Quaternion.Euler(0f, 0f, hoursToDegrees * (float)time.TotalHours);
            minutesPivot.localRotation = Quaternion.Euler(0f, 0f, minutesToDegrees * (float)time.TotalMinutes);
            secondsPivot.localRotation = Quaternion.Euler(0f, 0f, secondsToDegrees * (float)time.TotalSeconds);
        }
    }

}

