using System.Diagnostics;
using System.Runtime.CompilerServices;
using Anvil.CSharp.Logging;
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
            EmitErrorIfNonUniformScale(worldToLocalMtx.GetScale());
            return quaternion.LookRotationSafe(
                math.rotate(worldToLocalMtx, math.mul(rotation, math.forward())),
                math.rotate(worldToLocalMtx, math.mul(rotation, math.up()))
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion ConvertLocalToWorldRotation(LocalToWorld localToWorld, quaternion rotation)
        {
            return ConvertLocalToWorldRotation(localToWorld.Value, rotation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion ConvertLocalToWorldRotation(float4x4 localToWorldMtx, quaternion rotation)
        {
            EmitErrorIfNonUniformScale(localToWorldMtx.GetScale());
            return math.mul(localToWorldMtx.GetRotation(),rotation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 ConvertWorldToLocalScale(LocalToWorld localToWorld, float3 scale)
        {
            return ConvertLocalToWorldScale(math.inverse(localToWorld.Value), scale);
        }

        /// <summary>
        /// Converts a world scale value to the local space expressed by a matrix.
        ///
        /// NOTE: Transform matrices with negative scale values may produce output inconsistent with the existing
        /// component values. The results are still valid but should be applied in tandem with
        /// <see cref="ConvertWorldToLocalRotation"/>.
        /// (transforms with negative scale may be represented by multiple combinations of rotation and scale)
        /// </summary>
        /// <param name="worldToLocalMtx">The world to local transformation matrix.</param>
        /// <param name="scale">The world scale value transform.</param>
        /// <remarks>NOTE: This </remarks>
        /// <returns>The local scale value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 ConvertWorldToLocalScale(float4x4 worldToLocalMtx, float3 scale)
        {
            float3 worldToLocalScale = worldToLocalMtx.GetScale();
            EmitErrorIfNonUniformScale(worldToLocalScale);

            return worldToLocalScale * scale;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 ConvertLocalToWorldScale(LocalToWorld localToWorld, float3 scale)
        {
            return ConvertLocalToWorldScale(localToWorld.Value, scale);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 ConvertLocalToWorldScale(float4x4 localToWorldMtx, float3 scale)
        {
            float3 localToWorldScale = localToWorldMtx.GetScale();
            EmitErrorIfNonUniformScale(localToWorldScale);

            return localToWorldScale * scale;
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

        //TODO: #116 - Transforms with non-uniform scale operations are not currently supported.
        [Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EmitErrorIfNonUniformScale(
            float3 scale,
            [CallerMemberName] string callerMethodName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            scale = math.abs(scale);
            bool isUniform = scale.x.IsApproximately(scale.y) && scale.y.IsApproximately(scale.z);
            if (!isUniform)
            {
                Log.GetStaticLogger(typeof(TransformUtil)).Error(
                    "This conversion does not support transforms with non-uniform scaling.",
                    callerMethodName,
                    callerFilePath,
                    callerLineNumber
                    );
            }
        }
    }
}