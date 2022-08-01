using System.Security.AccessControl;
using System.Threading.Tasks.Sources;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

using static Unity.Mathematics.math;
using float4x4 = Unity.Mathematics.float4x4;
using quaternion = Unity.Mathematics.quaternion;

namespace Basics06
{
    public class Fractal : MonoBehaviour
    {
        [SerializeField, Range(1, 8)] int depth = 4;
        [SerializeField] Mesh mesh;
        [SerializeField] Material material;
        static float3[] directions = { up(), right(), left(), forward(), back() };
        static quaternion[] rotations = {
            quaternion.identity,
            quaternion.RotateZ(-0.5f * PI),quaternion.RotateZ(0.5f*PI),
            quaternion.RotateX(0.5f*PI),quaternion.RotateX(-0.5f*PI)};
        //FractalPart[][] parts;
        //Matrix4x4[][] matrices;
        ComputeBuffer[] matricesBuffers;
        static readonly int matricesId = Shader.PropertyToID("_Matrices");
        static MaterialPropertyBlock propertyBlock;

        NativeArray<FractalPart>[] parts;
        NativeArray<Matrix4x4>[] matrices;

        struct FractalPart
        {
            public Vector3 direction, worldPosition;
            public Quaternion rotation, worldRotation;
            public float spinAngle;
        }

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        struct UpdateFractalLevelJob : IJobFor
        {
            [ReadOnly] public float spinAngleDelta;
            public float scale;

            public NativeArray<FractalPart> parents;
            public NativeArray<FractalPart> parts;
            [WriteOnly] public NativeArray<Matrix4x4> matrices;
            public void Execute(int index)
            {
                FractalPart parent = parents[index / 5];
                FractalPart part = parts[index];
                //part.rotation *= deltaRotation;
                part.spinAngle += spinAngleDelta;
                part.worldRotation = mul(parent.worldRotation, mul(part.rotation, quaternion.RotateY(part.spinAngle)));
                part.worldPosition = parent.worldPosition + mul(parent.worldRotation, 1.5f * scale * part.direction);
                parts[index] = part;
                matrices[index] = Matrix4x4.TRS(part.worldPosition, part.worldRotation, float3(scale));
            }
        }

        private void OnEnable()
        {
            //parts = new FractalPart[depth][];
            //matrices = new Matrix4x4[depth][];
            parts = new NativeArray<FractalPart>[depth];
            matrices = new NativeArray<Matrix4x4>[depth];


            matricesBuffers = new ComputeBuffer[depth];
            int stride = 16 * 4;
            for (int i = 0, length = 1; i < parts.Length; i++, length *= 5)
            {
                //parts[i] = new FractalPart[length];
                //matrices[i] = new Matrix4x4[length];
                parts[i] = new NativeArray<FractalPart>(length, Allocator.Persistent);
                matrices[i] = new NativeArray<Matrix4x4>(length, Allocator.Persistent);
                matricesBuffers[i] = new ComputeBuffer(length, stride);
            }

            //float scale = 1f;
            parts[0][0] = CreatePart(0);

            for (int li = 1; li < parts.Length; li++)
            {
                //scale *= 0.5f;
                NativeArray<FractalPart> levelParts = parts[li];
                for (int fpi = 0; fpi < levelParts.Length; fpi += 5)
                {
                    for (int ci = 0; ci < 5; ci++)
                        levelParts[fpi + ci] = CreatePart(ci);
                }
            }

            propertyBlock ??= new MaterialPropertyBlock();
        }
        private void OnDisable()
        {
            for (int i = 0; i < matricesBuffers.Length; i++)
            {
                matricesBuffers[i].Release();
                parts[i].Dispose();
                matrices[i].Dispose();
            }
            parts = null;
            matrices = null;
            matricesBuffers = null;
        }

        private void OnValidate()
        {
            if (parts != null && enabled)
            {
                OnDisable();
                OnEnable();
            }
        }

        FractalPart CreatePart(int childIndex) => new FractalPart
        {
            direction = directions[childIndex],
            rotation = rotations[childIndex]
            //transform = go.transform
        };


        private void Update()
        {
            //Quaternion deltaRotation = Quaternion.Euler(0f, 22.5f * Time.deltaTime, 0f);
            float spinAngleDelta = 0.125f * PI * Time.deltaTime;

            FractalPart rootPart = parts[0][0];
            //rootPart.rotation *= deltaRotation;
            rootPart.spinAngle += spinAngleDelta;

            rootPart.worldRotation = mul(transform.rotation, mul(rootPart.rotation, quaternion.RotateY(rootPart.spinAngle)));
            rootPart.worldPosition = transform.position;
            parts[0][0] = rootPart;
            float objectScale = transform.lossyScale.x;
            matrices[0][0] = float4x4.TRS(
                rootPart.worldPosition, rootPart.worldRotation, float3(objectScale)
            );

            float scale = objectScale;
            JobHandle jobHandle = default;
            for (int li = 1; li < parts.Length; li++)
            {
                scale *= 0.5f;
                // NativeArray<FractalPart> parentParts = parts[li - 1];
                // NativeArray<FractalPart> levelParts = parts[li];
                // NativeArray<Matrix4x4> levelMatrices = matrices[li];
                jobHandle = new UpdateFractalLevelJob
                {
                    spinAngleDelta = spinAngleDelta,
                    scale = scale,
                    parents = parts[li - 1],
                    parts = parts[li],
                    matrices = matrices[li]
                }.Schedule(parts[li].Length, jobHandle);
                // for (int fpi = 0; fpi < parts[li].Length; fpi++)
                // {
                //     job.Execute(fpi);
                //     //Transform partentTransform = parentParts[fpi / 5].transform;
                //     FractalPart parent = parentParts[fpi / 5];
                //     FractalPart part = levelParts[fpi];
                //     //part.rotation *= deltaRotation;
                //     part.spinAngle += spinAngleDelta;
                //     part.worldRotation = parent.worldRotation * (part.rotation * Quaternion.Euler(0f, part.spinAngle, 0f));
                //     part.worldPosition = parent.worldPosition +
                //         parent.worldRotation * (1.5f * scale * part.direction);
                //     levelParts[fpi] = part;

                //     levelMatrices[fpi] = Matrix4x4.TRS(part.worldPosition, part.worldRotation, scale * Vector3.one);
                // }
            }
            jobHandle.Complete();

            var bounds = new Bounds(rootPart.worldPosition, 3f * objectScale * Vector3.one);
            for (int i = 0; i < matricesBuffers.Length; i++)
            {
                ComputeBuffer buffer = matricesBuffers[i];
                buffer.SetData(matrices[i]);
                propertyBlock.SetBuffer(matricesId, buffer);
                Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, buffer.count, propertyBlock);
            }
        }

        // // Start is called before the first frame update
        // void Start()
        // {
        //     name = "Fractal" + depth;

        //     if (depth <= 1)
        //     { return; }
        //     Fractal childA = CreateChild(Vector3.up, Quaternion.identity);
        //     Fractal childB = CreateChild(Vector3.right, Quaternion.Euler(0f, 0f, -90f));
        //     Fractal childC = CreateChild(Vector3.left, Quaternion.Euler(0f, 0f, 90f));
        //     Fractal childD = CreateChild(Vector3.forward, Quaternion.Euler(90f, 0f, 0f));
        //     Fractal childE = CreateChild(Vector3.back, Quaternion.Euler(-90f, 0f, 0f));

        //     childA.transform.SetParent(transform, false);
        //     childB.transform.SetParent(transform, false);
        //     childC.transform.SetParent(transform, false);
        //     childD.transform.SetParent(transform, false);
        //     childE.transform.SetParent(transform, false);


        //     // child.transform.SetParent(transform, false);
        //     // child.transform.localPosition = 0.75f * Vector3.right;
        //     // child.transform.localScale = 0.5f * Vector3.one;
        //     // child.depth -= 1;

        //     // child = Instantiate(this);
        //     // child.depth -= 1;
        //     // child.transform.SetParent(transform, false);
        //     // child.transform.localPosition = 0.75f * Vector3.up;
        //     // child.transform.localScale = 0.5f * Vector3.one;
        // }

        // // Update is called once per frame
        // void Update()
        // {
        //     transform.Rotate(0f, 22.5f * Time.deltaTime, 0f);
        // }

        // Fractal CreateChild(Vector3 direction, Quaternion rotatiion)
        // {
        //     Fractal child = Instantiate(this);
        //     child.depth -= 1;
        //     child.transform.SetParent(transform, false);
        //     child.transform.localPosition = 0.75f * direction;
        //     child.transform.localRotation = rotatiion;
        //     child.transform.localScale = 0.5f * Vector3.one;
        //     return child;
        // }
    }
}
