using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basics03
{
    public class MathematicalGraph : MonoBehaviour
    {
        [SerializeField]
        Transform pointPrefab;
        [SerializeField, Range(10, 100)]
        int resolution = 10;
        [SerializeField]
        FunctionLibray.FuncitionName funcition;
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

            float step = 2f / resolution;
            var scale = Vector3.one * step;
            //var position = Vector3.zero;
            points = new Transform[resolution * resolution];
            for (int i = 0; i < points.Length; i++)
            {
                //if (x == resolution) {
                //	x = 0;
                //	z += 1;
                //}
                Transform point = points[i] = Instantiate(pointPrefab);
                //position.x = (x + 0.5f) * step - 1f;
                //position.z = (z + 0.5f) * step - 1f;
                //point.localPosition = position;
                point.localScale = scale;
                point.SetParent(transform, false);
            }
        }

        private void Update()
        {
            FunctionLibray.Funcition f = FunctionLibray.GetFuncition(funcition);

            float time = Time.time;
            float step = 2f / resolution;
            float v = 0.5f * step - 1f;
            for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++)
            {
                if (x == resolution)
                {
                    x = 0;
                    z += 1;
                    v = (z + 0.5f) * step - 1f;
                }
                float u = (x + 0.5f) * step - 1f;
                //float v = (z + 0.5f) * step - 1f;
                points[i].localPosition = f(u, v, time);
            }
        }
    }
}
