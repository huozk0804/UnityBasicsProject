using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basics04;

namespace Basics05
{
    public class ComputeShadersGraph : MonoBehaviour
    {
        [SerializeField]
        Transform pointPrefab;
        [SerializeField, Range(10, 400)]
        int resolution = 10;
        [SerializeField]
        MeasuringPerformanceFunctionLibray.FunctionName function = default;
        Transform[] points;

        [SerializeField, Min(0f)]
        float functionDuration = 1f, transitionDuration = 1f;
        float duration;
        bool transitioning;

        MeasuringPerformanceFunctionLibray.FunctionName transitionFunction;

        public enum TransitionMode { Cycle, Random }
        [SerializeField]
        TransitionMode transitionMode;

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
            duration += Time.deltaTime;
            if (transitioning)
            {
                if (duration >= transitionDuration)
                {
                    duration -= transitionDuration;
                    transitioning = false;
                }
            }
            else if (duration >= functionDuration)
            {
                duration -= functionDuration;
                //funcition = MeasuringPerformanceFunctionLibray.GetNextFunctionName(funcition);
                transitioning = true;
                transitionFunction = function;
                PickNextFunction();
            }

            if (transitioning)
            {
                UpdateFunctionTransition();
            }
            else
            {
                UpdateFunction();
            }
        }

        void PickNextFunction()
        {
            function = transitionMode == TransitionMode.Cycle ?
            MeasuringPerformanceFunctionLibray.GetNextFunctionName(function) :
            MeasuringPerformanceFunctionLibray.GetRandomFunctionNameOtherThan(function);
        }

        void UpdateFunction()
        {
            MeasuringPerformanceFunctionLibray.Function f = MeasuringPerformanceFunctionLibray.GetFunction(function);

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

        void UpdateFunctionTransition()
        {
            MeasuringPerformanceFunctionLibray.Function
                from = MeasuringPerformanceFunctionLibray.GetFunction(transitionFunction),
                to = MeasuringPerformanceFunctionLibray.GetFunction(function);
            float progress = duration / transitionDuration;
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
                points[i].localPosition = MeasuringPerformanceFunctionLibray.Morph(
                    u, v, time, from, to, progress
                );
            }
        }
    }
}
