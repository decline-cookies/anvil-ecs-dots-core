using System;
using System.Runtime.CompilerServices;
using Anvil.Unity.Core;
using Anvil.Unity.DOTS.Mathematics;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Anvil.Unity.DOTS.Entities.Transform
{
    public static class TransformUtil
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 ConvertWorldToLocalPoint(LocalToWorld localToWorld, float3 point)
        {
            return ConvertWorldToLocalPoint(math.inverse(localToWorld.Value), point);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 ConvertWorldToLocalPoint(float4x4 worldToLocalMtx, float3 point)
        {
            return math.transform(worldToLocalMtx, point);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 ConvertLocalToWorldPoint(LocalToWorld localToWorld, float3 point)
        {
            return ConvertLocalToWorldPoint(localToWorld.Value, point);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 ConvertLocalToWorldPoint(float4x4 localToWorldMtx, float3 point)
        {
            return math.transform(localToWorldMtx, point);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion ConvertWorldToLocalRotation(LocalToWorld localToWorld, quaternion rotation)
        {
            return ConvertWorldToLocalRotation(math.inverse(localToWorld.Value), rotation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion ConvertWorldToLocalRotation(float4x4 worldToLocalMtx, quaternion rotation)
        {
            return math.mul(worldToLocalMtx.GetRotation(), rotation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion ConvertLocalToWorldRotation(LocalToWorld localToWorld, quaternion rotation)
        {
            return ConvertLocalToWorldRotation(localToWorld.Value, rotation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion ConvertLocalToWorldRotation(float4x4 localToWorldMtx, quaternion rotation)
        {
            return math.mul(localToWorldMtx.GetRotation(),rotation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 ConvertLocalToWorldScale(LocalToWorld localToWorld, float3 scale)
        {
            return ConvertLocalToWorldScale(localToWorld.Value, scale);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 ConvertLocalToWorldScale(float4x4 localToWorldMtx, float3 scale)
        {
            return localToWorldMtx.GetScale() * scale;
        }

        public static Rect ConvertWorldToLocalRect(LocalToWorld localToWorld, Rect worldRect)
        {
            //TODO: #321 - Optimize...
            float4x4 worldToLocalMtx = math.inverse(localToWorld.Value);

            float3 point1 = (Vector3)worldRect.min;
            float3 point2 = (Vector3)worldRect.max;
            float3 point3 = new float3(point1.x, point2.y, 0);
            float3 point4 = new float3(point2.x, point1.y, 0);

            return RectUtil.CreateFromPoints(
                ConvertWorldToLocalPoint(worldToLocalMtx, point1).xy,
                ConvertWorldToLocalPoint(worldToLocalMtx, point2).xy,
                ConvertWorldToLocalPoint(worldToLocalMtx, point3).xy,
                ConvertWorldToLocalPoint(worldToLocalMtx, point4).xy
            );
        }

        public static Rect ConvertLocalToWorldRect(LocalToWorld localToWorld, Rect localRect)
        {
            //TODO: #321 - Optimize...
            float4x4 worldToLocalMtx = math.inverse(localToWorld.Value);

            float3 point1 = (Vector3)localRect.min;
            float3 point2 = (Vector3)localRect.max;
            float3 point3 = new float3(point1.x, point2.y, 0);
            float3 point4 = new float3(point2.x, point1.y, 0);

            return RectUtil.CreateFromPoints(
                ConvertLocalToWorldPoint(worldToLocalMtx, point1).xy,
                ConvertLocalToWorldPoint(worldToLocalMtx, point2).xy,
                ConvertLocalToWorldPoint(worldToLocalMtx, point3).xy,
                ConvertLocalToWorldPoint(worldToLocalMtx, point4).xy
            );
        }
    }
}