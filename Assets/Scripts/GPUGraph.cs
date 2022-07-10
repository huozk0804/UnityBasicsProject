using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basics05
{
    public class GPUGraph : MonoBehaviour
    {
        const int maxResolution = 1000;
        [SerializeField, Range(10, maxResolution)] int resolution = 10;
        [SerializeField] ComputeShadersFunctionLibray.FunctionName function = default;
        [SerializeField, Min(0f)] float functionDuration = 1f, transitionDuration = 1f;
        float duration;
        bool transitioning;
        ComputeShadersFunctionLibray.FunctionName transitionFunction;
        public enum TransitionMode { Cycle, Random }
        [SerializeField] TransitionMode transitionMode;

        ComputeBuffer positionsBuffer;
        [SerializeField] ComputeShader computeShader;
        static readonly int positionsID = Shader.PropertyToID("_Positions"),
        resolutionID = Shader.PropertyToID("_Resolution"),
        stepID = Shader.PropertyToID("_Step"),
        timeID = Shader.PropertyToID("_Time"),
        transitionProgressId = Shader.PropertyToID("_TransitionProgress");
        [SerializeField] Material material;
        [SerializeField] Mesh mesh;

        private void OnEnable()
        {
            positionsBuffer = new ComputeBuffer(maxResolution * maxResolution, 3 * 4);
        }

        private void OnDisable()
        {
            positionsBuffer.Release();
            positionsBuffer = null;
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

            UpdateFunctionOnGPU();
        }

        void PickNextFunction()
        {
            function = transitionMode == TransitionMode.Cycle ?
            ComputeShadersFunctionLibray.GetNextFunctionName(function) :
            ComputeShadersFunctionLibray.GetRandomFunctionNameOtherThan(function);
        }

        void UpdateFunctionOnGPU()
        {
            float step = 2f / resolution;
            computeShader.SetInt(resolutionID, resolution);
            computeShader.SetFloat(stepID, step);
            computeShader.SetFloat(timeID, Time.time);
            if (transitioning)
            {
                computeShader.SetFloat(
                    transitionProgressId,
                    Mathf.SmoothStep(0f, 1f, duration / transitionDuration)
                );
            }

            var kernelIndex = (int)function + (int)(transitioning ? transitionFunction : function) * ComputeShadersFunctionLibray.GetFunctionCount;
            computeShader.SetBuffer(kernelIndex, positionsID, positionsBuffer);

            int groups = Mathf.CeilToInt(resolution / 8f);
            computeShader.Dispatch(kernelIndex, groups, groups, 1);

            material.SetBuffer(positionsID, positionsBuffer);
            material.SetFloat(stepID, step);
            var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
            Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, resolution * resolution);
        }
    }
}
