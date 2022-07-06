using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basics02
{
    public class Graph : MonoBehaviour
    {
        [SerializeField]
        Transform pointPrefab;
        [SerializeField, Range(10, 100)]
        int resolution = 10;
        Transform[] points;

        private void Awake()
        {
            // int i = 0;
            // while (++i < 10)
            // {
            //     Transform point = Instantiate(pointPrefab);
            //     point.localPosition = Vector3.right * i;
            // }

            // float step = 2f / resolution;
            // Vector3 position = Vector3.zero;
            // var scale = Vector3.one * step;
            // for (int i = 0; i < resolution; i++)
            // {
            //     Transform point = Instantiate(pointPrefab);
            //     position.x = (i + 0.5f) * step - 1f;
            //     position.y = position.x * position.x * position.x;
            //     point.localPosition = position;
            //     point.localScale = scale;
            //     point.SetParent(transform, false);
            // }

            points = new Transform[resolution];
            float step = 2f / resolution;
            Vector3 position = Vector3.zero;
            var scale = Vector3.one * step;
            for (int i = 0; i < points.Length; i++)
            {
                var point = points[i] = Instantiate(pointPrefab);
                position.x = (i + 0.5f) * step - 1f;
                position.y = position.x * position.x * position.x;
                point.localPosition = position;
                point.localScale = scale;
                point.SetParent(transform, false);
            }
        }

        private void Update()
        {
            float time = Time.time;
            for (int i = 0; i < points.Length; i++)
            {
                Transform point = points[i];
                Vector3 position = point.localPosition;
                position.y = Mathf.Sin(Mathf.PI * (position.x + time));
                point.localPosition = position;
            }
        }
    }
}
