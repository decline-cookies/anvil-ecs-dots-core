using Anvil.CSharp.Mathematics;
using Anvil.Unity.DOTS.Entities.Transform;
using Anvil.Unity.DOTS.Mathematics;
using NUnit.Framework;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Anvil.Unity.DOTS.Tests.Entities.Transform
{
    public static class TransformUtilTests
    {
        //TODO: Update to use [DefaultFloatingPointTolerance] when Unity uses NUnit >=3.7 and remove all .Using<float3>(EqualityWithTolerance) uses
        // https://github.com/nunit/nunit/blob/master/src/NUnitFramework/framework/Attributes/DefaultFloatingPointToleranceAttribute.cs
        private const float FLOATING_POINT_TOLERANCE = 0.00001f;

        private static int EqualityWithTolerance(float3 a, float3 b)
        {
            return math.all(math.abs(a - b) < MathUtil.FLOATING_POINT_EQUALITY_TOLERANCE) ? 0 : 1;
        }
        private static int EqualityWithTolerance(float4 a, float4 b)
        {
            return math.all(math.abs(a - b) < MathUtil.FLOATING_POINT_EQUALITY_TOLERANCE) ? 0 : 1;
        }
        private static int EqualityWithTolerance(quaternion a, quaternion b)
        {
            return EqualityWithTolerance(a.value, b.value);
        }

        // ----- ConvertWorldToLocalPointTest ----- //
        [Test]
        public static void ConvertWorldToLocalPointTest_Identity()
        {
            float3 point_one = new float3(1, 1, 1);
            float3 point_seven = point_one * 7f;
            float3 point_sevenXY = new float3(point_seven.xy, 0);

            LocalToWorld localToWorld_Identity = new LocalToWorld() { Value = float4x4.identity };
            float4x4 worldToLocal_TranslatedOne = math.inverse(localToWorld_Identity.Value);
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_Identity, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_Identity, point_one), Is.EqualTo(point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_Identity, point_seven), Is.EqualTo(point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_Identity, point_sevenXY), Is.EqualTo(point_sevenXY).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_Identity, -point_one), Is.EqualTo(-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_Identity, -point_seven), Is.EqualTo(-point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_Identity, -point_sevenXY), Is.EqualTo(-point_sevenXY).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedOne, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedOne, point_one), Is.EqualTo(point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedOne, point_seven), Is.EqualTo(point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedOne, point_sevenXY), Is.EqualTo(point_sevenXY).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedOne, -point_one), Is.EqualTo(-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedOne, -point_seven), Is.EqualTo(-point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedOne, -point_sevenXY), Is.EqualTo(-point_sevenXY).Using<float3>(EqualityWithTolerance));
        }

        [Test]
        public static void ConvertWorldToLocalPointTest_Translate()
        {
            float3 point_one = new float3(1, 1, 1);
            float3 point_seven = point_one * 7f;
            float3 point_oneXY = new float3(1, 1, 0);
            float3 point_sevenXY = point_oneXY * 7f;

            LocalToWorld localToWorld_TranslatedOne = new LocalToWorld()
            {
                Value = float4x4.TRS(point_one, quaternion.identity, point_one)
            };
            float4x4 worldToLocal_TranslatedOne = math.inverse(localToWorld_TranslatedOne.Value);
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedOne, float3.zero), Is.EqualTo(-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedOne, point_one), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedOne, point_seven), Is.EqualTo(point_seven-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedOne, point_sevenXY), Is.EqualTo(point_sevenXY-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedOne, -point_one), Is.EqualTo(-point_one-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedOne, -point_seven), Is.EqualTo(-point_seven-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedOne, -point_sevenXY), Is.EqualTo(-point_sevenXY-point_one).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedOne, float3.zero), Is.EqualTo(-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedOne, point_one), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedOne, point_seven), Is.EqualTo(point_seven-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedOne, point_sevenXY), Is.EqualTo(point_sevenXY-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedOne, -point_one), Is.EqualTo(-point_one-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedOne, -point_seven), Is.EqualTo(-point_seven-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedOne, -point_sevenXY), Is.EqualTo(-point_sevenXY-point_one).Using<float3>(EqualityWithTolerance));


            LocalToWorld localToWorld_TranslatedNegativeOne = new LocalToWorld()
            {
                Value = float4x4.TRS(-point_one, quaternion.identity, point_one)
            };
            float4x4 worldToLocal_TranslatedNegativeOne = math.inverse(localToWorld_TranslatedNegativeOne.Value);
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedNegativeOne, float3.zero), Is.EqualTo(point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedNegativeOne, point_one), Is.EqualTo(point_one+point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedNegativeOne, point_seven), Is.EqualTo(point_seven+point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedNegativeOne, point_sevenXY), Is.EqualTo(point_sevenXY+point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedNegativeOne, -point_one), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedNegativeOne, -point_seven), Is.EqualTo(-point_seven+point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedNegativeOne, -point_sevenXY), Is.EqualTo(-point_sevenXY+point_one).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedNegativeOne, float3.zero), Is.EqualTo(point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedNegativeOne, point_one), Is.EqualTo(point_one+point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedNegativeOne, point_seven), Is.EqualTo(point_seven+point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedNegativeOne, point_sevenXY), Is.EqualTo(point_sevenXY+point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedNegativeOne, -point_one), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedNegativeOne, -point_seven), Is.EqualTo(-point_seven+point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedNegativeOne, -point_sevenXY), Is.EqualTo(-point_sevenXY+point_one).Using<float3>(EqualityWithTolerance));


            LocalToWorld localToWorld_TranslatedSeven = new LocalToWorld()
            {
                Value = float4x4.TRS(point_seven, quaternion.identity, point_one)
            };
            float4x4 worldToLocal_TranslatedSeven = math.inverse(localToWorld_TranslatedSeven.Value);
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedSeven, float3.zero), Is.EqualTo(-point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedSeven, point_one), Is.EqualTo(-point_seven+point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedSeven, point_seven), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedSeven, point_sevenXY), Is.EqualTo(point_sevenXY - point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedSeven, -point_one), Is.EqualTo(-point_seven-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedSeven, -point_seven), Is.EqualTo(-point_seven-point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedSeven, -point_sevenXY), Is.EqualTo(-point_sevenXY-point_seven).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedSeven, float3.zero), Is.EqualTo(-point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedSeven, point_one), Is.EqualTo(-point_seven+point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedSeven, point_seven), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedSeven, point_sevenXY), Is.EqualTo(point_sevenXY - point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedSeven, -point_one), Is.EqualTo(-point_seven-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedSeven, -point_seven), Is.EqualTo(-point_seven-point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedSeven, -point_sevenXY), Is.EqualTo(-point_sevenXY-point_seven).Using<float3>(EqualityWithTolerance));


            LocalToWorld localToWorld_TranslatedNegativeSeven = new LocalToWorld()
            {
                Value = float4x4.TRS(-point_seven, quaternion.identity, point_one)
            };
            float4x4 worldToLocal_TranslatedNegativeSeven = math.inverse(localToWorld_TranslatedNegativeSeven.Value);
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedNegativeSeven, float3.zero), Is.EqualTo(point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedNegativeSeven, point_one), Is.EqualTo(point_seven+point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedNegativeSeven, point_seven), Is.EqualTo(point_seven+point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedNegativeSeven, point_sevenXY), Is.EqualTo(point_sevenXY+point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedNegativeSeven, -point_one), Is.EqualTo(point_seven-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedNegativeSeven, -point_seven), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedNegativeSeven, -point_sevenXY), Is.EqualTo(point_seven-point_sevenXY).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedNegativeSeven, float3.zero), Is.EqualTo(point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedNegativeSeven, point_one), Is.EqualTo(point_seven+point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedNegativeSeven, point_seven), Is.EqualTo(point_seven+point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedNegativeSeven, point_sevenXY), Is.EqualTo(point_sevenXY+point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedNegativeSeven, -point_one), Is.EqualTo(point_seven-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedNegativeSeven, -point_seven), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedNegativeSeven, -point_sevenXY), Is.EqualTo(point_seven-point_sevenXY).Using<float3>(EqualityWithTolerance));


            LocalToWorld localToWorld_TranslatedSevenXY = new LocalToWorld()
            {
                Value = float4x4.TRS(point_sevenXY, quaternion.identity, point_one)
            };
            float4x4 worldToLocal_TranslatedSevenXY = math.inverse(localToWorld_TranslatedSevenXY.Value);
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedSevenXY, float3.zero), Is.EqualTo(-point_sevenXY).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedSevenXY, point_one), Is.EqualTo(-point_sevenXY+point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedSevenXY, point_seven), Is.EqualTo(point_seven-point_sevenXY).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedSevenXY, point_sevenXY), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedSevenXY, -point_one), Is.EqualTo(-point_sevenXY-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedSevenXY, -point_seven), Is.EqualTo(-point_seven-point_sevenXY).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedSevenXY, -point_sevenXY), Is.EqualTo(-point_sevenXY-point_sevenXY).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedSevenXY, float3.zero), Is.EqualTo(-point_sevenXY).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedSevenXY, point_one), Is.EqualTo(-point_sevenXY+point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedSevenXY, point_seven), Is.EqualTo(point_seven-point_sevenXY).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedSevenXY, point_sevenXY), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedSevenXY, -point_one), Is.EqualTo(-point_sevenXY-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedSevenXY, -point_seven), Is.EqualTo(-point_seven-point_sevenXY).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_TranslatedSevenXY, -point_sevenXY), Is.EqualTo(-point_sevenXY-point_sevenXY).Using<float3>(EqualityWithTolerance));
        }

        [Test]
        public static void ConvertWorldToLocalPointTest_Rotate()
        {
            float3 point_one = new float3(1, 1, 1);
            float3 point_seven = point_one * 7f;

            LocalToWorld localToWorld_RotatedZ90 = new LocalToWorld()
            {
                Value = float4x4.TRS(float3.zero, quaternion.Euler(0, 0, math.radians(90)), point_one)
            };
            float4x4 worldToLocal_RotatedZ90 = math.inverse(localToWorld_RotatedZ90.Value);
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_RotatedZ90, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_RotatedZ90, point_one), Is.EqualTo(new float3(1f, -1f, 1f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_RotatedZ90, point_seven), Is.EqualTo(new float3(7f, -7f, 7f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_RotatedZ90, -point_one), Is.EqualTo(new float3(-1f, 1f, -1f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_RotatedZ90, -point_seven), Is.EqualTo(new float3(-7f, 7f, -7f)).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_RotatedZ90, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_RotatedZ90, point_one), Is.EqualTo(new float3(1f, -1f, 1f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_RotatedZ90, point_seven), Is.EqualTo(new float3(7f, -7f, 7f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_RotatedZ90, -point_one), Is.EqualTo(new float3(-1f, 1f, -1f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_RotatedZ90, -point_seven), Is.EqualTo(new float3(-7f, 7f, -7f)).Using<float3>(EqualityWithTolerance));


            LocalToWorld localToWorld_RotatedZNegative90 = new LocalToWorld()
            {
                Value = float4x4.TRS(float3.zero, quaternion.Euler(0, 0, math.radians(-90)), point_one)
            };
            float4x4 worldToLocal_RotatedNegative90 = math.inverse(localToWorld_RotatedZNegative90.Value);
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_RotatedZNegative90, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_RotatedZNegative90, point_one), Is.EqualTo(new float3(-1f, 1f, 1f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_RotatedZNegative90, point_seven), Is.EqualTo(new float3(-7f, 7f, 7f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_RotatedZNegative90, -point_one), Is.EqualTo(new float3(1f, -1f, -1f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_RotatedZNegative90, -point_seven), Is.EqualTo(new float3(7f, -7f, -7f)).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_RotatedNegative90, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_RotatedNegative90, point_one), Is.EqualTo(new float3(-1f, 1f, 1f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_RotatedNegative90, point_seven), Is.EqualTo(new float3(-7f, 7f, 7f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_RotatedNegative90, -point_one), Is.EqualTo(new float3(1f, -1f, -1f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_RotatedNegative90, -point_seven), Is.EqualTo(new float3(7f, -7f, -7f)).Using<float3>(EqualityWithTolerance));


            LocalToWorld localToWorld_RotatedZX90 = new LocalToWorld()
            {
                Value = float4x4.TRS(float3.zero, quaternion.Euler(math.radians(90), 0, math.radians(90)), point_one)
            };
            float4x4 worldToLocal_RotatedZX90 = math.inverse(localToWorld_RotatedZX90.Value);
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_RotatedZX90, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_RotatedZX90, point_one), Is.EqualTo(new float3(1f, -1f, -1f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_RotatedZX90, point_seven), Is.EqualTo(new float3(7f, -7f, -7f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_RotatedZX90, -point_one), Is.EqualTo(new float3(-1f, 1f, 1f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_RotatedZX90, -point_seven), Is.EqualTo(new float3(-7f, 7f, 7f)).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_RotatedZX90, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_RotatedZX90, point_one), Is.EqualTo(new float3(1f, -1f, -1f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_RotatedZX90, point_seven), Is.EqualTo(new float3(7f, -7f, -7f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_RotatedZX90, -point_one), Is.EqualTo(new float3(-1f, 1f, 1f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_RotatedZX90, -point_seven), Is.EqualTo(new float3(-7f, 7f, 7f)).Using<float3>(EqualityWithTolerance));


            LocalToWorld localToWorld_RotatedZXNegative90 = new LocalToWorld()
            {
                Value = float4x4.TRS(float3.zero, quaternion.Euler(math.radians(-90), 0, math.radians(-90)), point_one)
            };
            float4x4 worldToLocal_RotatedZXNegative90 = math.inverse(localToWorld_RotatedZXNegative90.Value);
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_RotatedZXNegative90, float3.zero), Is.EqualTo(float3.zero));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_RotatedZXNegative90, point_one), Is.EqualTo(point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_RotatedZXNegative90, point_seven), Is.EqualTo(point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_RotatedZXNegative90, -point_one), Is.EqualTo(-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_RotatedZXNegative90, -point_seven), Is.EqualTo(-point_seven).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_RotatedZXNegative90, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_RotatedZXNegative90, point_one), Is.EqualTo(point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_RotatedZXNegative90, point_seven), Is.EqualTo(point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_RotatedZXNegative90, -point_one), Is.EqualTo(-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_RotatedZXNegative90, -point_seven), Is.EqualTo(-point_seven).Using<float3>(EqualityWithTolerance));


            LocalToWorld localToWorld_RotatedZX45 = new LocalToWorld()
            {
                Value = float4x4.TRS(float3.zero, quaternion.Euler(math.radians(45), 0, math.radians(45)), point_one)
            };
            float4x4 worldToLocal_RotatedZX45 = math.inverse(localToWorld_RotatedZX45.Value);
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_RotatedZX45, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_RotatedZX45, point_one), Is.EqualTo(new float3(1.70710683f,0.292893201f,0f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_RotatedZX45, point_seven), Is.EqualTo(new float3(11.9497471f,2.0502522f,0f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_RotatedZX45, -point_one), Is.EqualTo(new float3(-1.70710683f,-0.292893201f,0f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_RotatedZX45, -point_seven), Is.EqualTo(new float3(-11.9497471f,-2.0502522f,0f)).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_RotatedZX45, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_RotatedZX45, point_one), Is.EqualTo(new float3(1.70710683f,0.292893201f,0f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_RotatedZX45, point_seven), Is.EqualTo(new float3(11.9497471f,2.0502522f,0f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_RotatedZX45, -point_one), Is.EqualTo(new float3(-1.70710683f,-0.292893201f,0f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_RotatedZX45, -point_seven), Is.EqualTo(new float3(-11.9497471f,-2.0502522f,0f)).Using<float3>(EqualityWithTolerance));
        }

        [Test]
        public static void ConvertWorldToLocalPointTest_Scale()
        {
            float3 point_one = new float3(1, 1, 1);
            float3 point_seven = point_one * 7f;

            LocalToWorld localToWorld_Scaled2 = new LocalToWorld()
            {
                Value = float4x4.TRS(float3.zero, quaternion.identity, point_one*2f)
            };
            float4x4 worldToLocal_Scaled2 = math.inverse(localToWorld_Scaled2.Value);
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_Scaled2, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_Scaled2, point_one), Is.EqualTo(point_one/2f).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_Scaled2, point_seven), Is.EqualTo(point_seven/2f).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_Scaled2, -point_one), Is.EqualTo(-point_one/2f).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_Scaled2, -point_seven), Is.EqualTo(-point_seven/2f).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_Scaled2, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_Scaled2, point_one), Is.EqualTo(point_one/2f).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_Scaled2, point_seven), Is.EqualTo(point_seven/2f).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_Scaled2, -point_one), Is.EqualTo(-point_one/2f).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_Scaled2, -point_seven), Is.EqualTo(-point_seven/2f).Using<float3>(EqualityWithTolerance));


            LocalToWorld localToWorld_ScaledNegative2 = new LocalToWorld()
            {
                Value = float4x4.TRS(float3.zero, quaternion.identity, point_one*-2f)
            };
            float4x4 worldToLocal_ScaledNegative2 = math.inverse(localToWorld_ScaledNegative2.Value);
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_ScaledNegative2, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_ScaledNegative2, point_one), Is.EqualTo(-point_one/2f).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_ScaledNegative2, point_seven), Is.EqualTo(-point_seven/2f).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_ScaledNegative2, -point_one), Is.EqualTo(point_one/2f).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_ScaledNegative2, -point_seven), Is.EqualTo(point_seven/2f).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_ScaledNegative2, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_ScaledNegative2, point_one), Is.EqualTo(-point_one/2f).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_ScaledNegative2, point_seven), Is.EqualTo(-point_seven/2f).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_ScaledNegative2, -point_one), Is.EqualTo(point_one/2f).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_ScaledNegative2, -point_seven), Is.EqualTo(point_seven/2f).Using<float3>(EqualityWithTolerance));


            float3 scaleZ2 = new float3(point_one.xy, 2f);
            LocalToWorld localToWorld_ScaledZ2 = new LocalToWorld()
            {
                Value = float4x4.TRS(float3.zero, quaternion.identity, scaleZ2)
            };
            float4x4 worldToLocal_ScaledZ2 = math.inverse(localToWorld_ScaledZ2.Value);
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_ScaledZ2, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_ScaledZ2, point_one), Is.EqualTo(point_one/scaleZ2).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_ScaledZ2, point_seven), Is.EqualTo(point_seven/scaleZ2).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_ScaledZ2, -point_one), Is.EqualTo(-point_one/scaleZ2).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_ScaledZ2, -point_seven), Is.EqualTo(-point_seven/scaleZ2).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_ScaledZ2, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_ScaledZ2, point_one), Is.EqualTo(point_one/scaleZ2).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_ScaledZ2, point_seven), Is.EqualTo(point_seven/scaleZ2).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_ScaledZ2, -point_one), Is.EqualTo(-point_one/scaleZ2).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_ScaledZ2, -point_seven), Is.EqualTo(-point_seven/scaleZ2).Using<float3>(EqualityWithTolerance));


            float3 scaleZNegative2 = new float3(point_one.xy, -2f);
            LocalToWorld localToWorld_ScaledZNegative2 = new LocalToWorld()
            {
                Value = float4x4.TRS(float3.zero, quaternion.identity, scaleZNegative2)
            };
            float4x4 worldToLocal_ScaledZNegative2 = math.inverse(localToWorld_ScaledZNegative2.Value);
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_ScaledZNegative2, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_ScaledZNegative2, point_one), Is.EqualTo(point_one/scaleZNegative2).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_ScaledZNegative2, point_seven), Is.EqualTo(point_seven/scaleZNegative2).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_ScaledZNegative2, -point_one), Is.EqualTo(-point_one/scaleZNegative2).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_ScaledZNegative2, -point_seven), Is.EqualTo(-point_seven/scaleZNegative2).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_ScaledZNegative2, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_ScaledZNegative2, point_one), Is.EqualTo(point_one/scaleZNegative2).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_ScaledZNegative2, point_seven), Is.EqualTo(point_seven/scaleZNegative2).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_ScaledZNegative2, -point_one), Is.EqualTo(-point_one/scaleZNegative2).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_ScaledZNegative2, -point_seven), Is.EqualTo(-point_seven/scaleZNegative2).Using<float3>(EqualityWithTolerance));


            float3 scaleZXOnePointFive = new float3(1.5f, 1f, 1.5f);
            LocalToWorld localToWorld_ScaledZXOnePointFive = new LocalToWorld()
            {
                Value = float4x4.TRS(float3.zero, quaternion.identity, scaleZXOnePointFive)
            };
            float4x4 worldToLocal_ScaledZXOnePointFive = math.inverse(localToWorld_ScaledZXOnePointFive.Value);
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_ScaledZXOnePointFive, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_ScaledZXOnePointFive, point_one), Is.EqualTo(point_one/scaleZXOnePointFive).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_ScaledZXOnePointFive, point_seven), Is.EqualTo(point_seven/scaleZXOnePointFive).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_ScaledZXOnePointFive, -point_one), Is.EqualTo(-point_one/scaleZXOnePointFive).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_ScaledZXOnePointFive, -point_seven), Is.EqualTo(-point_seven/scaleZXOnePointFive).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_ScaledZXOnePointFive, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_ScaledZXOnePointFive, point_one), Is.EqualTo(point_one/scaleZXOnePointFive).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_ScaledZXOnePointFive, point_seven), Is.EqualTo(point_seven/scaleZXOnePointFive).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_ScaledZXOnePointFive, -point_one), Is.EqualTo(-point_one/scaleZXOnePointFive).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_ScaledZXOnePointFive, -point_seven), Is.EqualTo(-point_seven/scaleZXOnePointFive).Using<float3>(EqualityWithTolerance));
        }

        [Test]
        public static void ConvertWorldToLocalPointTest_Compound()
        {
            float3 point_one = new float3(1f, 1f, 1f);
            float3 point_seven = point_one * 7f;
            float3 point_sevenXY = new float3(point_seven.xy, 0f);

            LocalToWorld localToWorld_Compound = new LocalToWorld()
            {
                Value = float4x4.TRS(point_sevenXY, quaternion.Euler(math.radians(45), 0, math.radians(45)), point_one*2f)
            };
            float4x4 worldToLocal_Compound = math.inverse(localToWorld_Compound.Value);
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_Compound, float3.zero), Is.EqualTo(new float3(-4.22487354f, 0.7248739f, 2.47487402f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_Compound, point_one), Is.EqualTo(new float3(-3.37132025f, 0.871320367f, 2.47487402f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_Compound, point_seven), Is.EqualTo(new float3(1.75f, 1.75f, 2.47487378f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_Compound, -point_one), Is.EqualTo(new float3(-5.07842731f, 0.578427196f, 2.47487402f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_Compound, -point_seven), Is.EqualTo(new float3(-10.1997471f, -0.300252199f, 2.4748745f)).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_Compound, float3.zero), Is.EqualTo(new float3(-4.22487354f, 0.7248739f, 2.47487402f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_Compound, point_one), Is.EqualTo(new float3(-3.37132025f, 0.871320367f, 2.47487402f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_Compound, point_seven), Is.EqualTo(new float3(1.75f, 1.75f, 2.47487378f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_Compound, -point_one), Is.EqualTo(new float3(-5.07842731f, 0.578427196f, 2.47487402f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_Compound, -point_seven), Is.EqualTo(new float3(-10.1997471f, -0.300252199f, 2.4748745f)).Using<float3>(EqualityWithTolerance));


            LocalToWorld localToWorld_CompoundNegative = new LocalToWorld()
            {
                Value = float4x4.TRS(-point_sevenXY, quaternion.Euler(math.radians(-45), 0, math.radians(-45)), point_one*-2f)
            };
            float4x4 worldToLocal_CompoundNegative = math.inverse(localToWorld_CompoundNegative.Value);
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_CompoundNegative, float3.zero), Is.EqualTo(new float3(-0.724873781f, -4.22487402f, -2.47487402f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_CompoundNegative, point_one), Is.EqualTo(new float3(-1.07842731f, -4.57842731f, -3.18198061f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_CompoundNegative, point_seven), Is.EqualTo(new float3(-3.19974756f,-6.69974804f,-7.42462158f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_CompoundNegative, -point_one), Is.EqualTo(new float3(-0.371320248f, -3.87132072f, -1.76776719f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_CompoundNegative, -point_seven), Is.EqualTo(new float3(1.75f, -1.75f, 2.47487378f)).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_CompoundNegative, float3.zero), Is.EqualTo(new float3(-0.724873781f, -4.22487402f, -2.47487402f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_CompoundNegative, point_one), Is.EqualTo(new float3(-1.07842731f, -4.57842731f, -3.18198061f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_CompoundNegative, point_seven), Is.EqualTo(new float3(-3.19974756f,-6.69974804f,-7.42462158f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_CompoundNegative, -point_one), Is.EqualTo(new float3(-0.371320248f, -3.87132072f, -1.76776719f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(worldToLocal_CompoundNegative, -point_seven), Is.EqualTo(new float3(1.75f, -1.75f, 2.47487378f)).Using<float3>(EqualityWithTolerance));
        }

        // ----- ConvertLocalToWorldPointTest ----- //
        [Test]
        public static void ConvertLocalToWorldPointTest_Identity()
        {
            float3 point_one = new float3(1, 1, 1);
            float3 point_seven = point_one * 7f;
            float3 point_oneXY = new float3(1, 1, 0);
            float3 point_sevenXY = point_oneXY * 7f;

            LocalToWorld localToWorld_Identity = new LocalToWorld() { Value = float4x4.identity };
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_Identity, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_Identity, point_one), Is.EqualTo(point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_Identity, point_seven), Is.EqualTo(point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_Identity, point_sevenXY), Is.EqualTo(point_sevenXY).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_Identity, -point_one), Is.EqualTo(-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_Identity, -point_seven), Is.EqualTo(-point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_Identity, -point_sevenXY), Is.EqualTo(-point_sevenXY).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_Identity.Value, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_Identity.Value, point_one), Is.EqualTo(point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_Identity.Value, point_seven), Is.EqualTo(point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_Identity.Value, point_sevenXY), Is.EqualTo(point_sevenXY).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_Identity.Value, -point_one), Is.EqualTo(-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_Identity.Value, -point_seven), Is.EqualTo(-point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_Identity.Value, -point_sevenXY), Is.EqualTo(-point_sevenXY).Using<float3>(EqualityWithTolerance));
        }

        [Test]
        public static void ConvertLocalToWorldPointTest_Translate()
        {
            float3 point_one = new float3(1, 1, 1);
            float3 point_seven = point_one * 7f;
            float3 point_oneXY = new float3(1, 1, 0);
            float3 point_sevenXY = point_oneXY * 7f;


            LocalToWorld localToWorld_TranslatedOne = new LocalToWorld()
            {
                Value = float4x4.TRS(point_one, quaternion.identity, point_one)
            };
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedOne, float3.zero), Is.EqualTo(point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedOne, point_one), Is.EqualTo(point_one+point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedOne, point_seven), Is.EqualTo(point_seven+point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedOne, point_sevenXY), Is.EqualTo(point_sevenXY+point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedOne, -point_one), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedOne, -point_seven), Is.EqualTo(-point_seven+point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedOne, -point_sevenXY), Is.EqualTo(-point_sevenXY+point_one).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedOne.Value, float3.zero), Is.EqualTo(point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedOne.Value, point_one), Is.EqualTo(point_one+point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedOne.Value, point_seven), Is.EqualTo(point_seven+point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedOne.Value, point_sevenXY), Is.EqualTo(point_sevenXY+point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedOne.Value, -point_one), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedOne.Value, -point_seven), Is.EqualTo(-point_seven+point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedOne.Value, -point_sevenXY), Is.EqualTo(-point_sevenXY+point_one).Using<float3>(EqualityWithTolerance));


            LocalToWorld localToWorld_TranslatedNegativeOne = new LocalToWorld()
            {
                Value = float4x4.TRS(-point_one, quaternion.identity, point_one)
            };
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedNegativeOne, float3.zero), Is.EqualTo(-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedNegativeOne, point_one), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedNegativeOne, point_seven), Is.EqualTo(point_seven-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedNegativeOne, point_sevenXY), Is.EqualTo(point_sevenXY-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedNegativeOne, -point_one), Is.EqualTo(-point_one-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedNegativeOne, -point_seven), Is.EqualTo(-point_seven-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedNegativeOne, -point_sevenXY), Is.EqualTo(-point_sevenXY-point_one).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedNegativeOne.Value, float3.zero), Is.EqualTo(-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedNegativeOne.Value, point_one), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedNegativeOne.Value, point_seven), Is.EqualTo(point_seven-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedNegativeOne.Value, point_sevenXY), Is.EqualTo(point_sevenXY-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedNegativeOne.Value, -point_one), Is.EqualTo(-point_one-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedNegativeOne.Value, -point_seven), Is.EqualTo(-point_seven-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedNegativeOne.Value, -point_sevenXY), Is.EqualTo(-point_sevenXY-point_one).Using<float3>(EqualityWithTolerance));


            LocalToWorld localToWorld_TranslatedSeven = new LocalToWorld()
            {
                Value = float4x4.TRS(point_seven, quaternion.identity, point_one)
            };
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedSeven, float3.zero), Is.EqualTo(point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedSeven, point_one), Is.EqualTo(point_seven+point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedSeven, point_seven), Is.EqualTo(point_seven+point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedSeven, point_sevenXY), Is.EqualTo(point_sevenXY+point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedSeven, -point_one), Is.EqualTo(point_seven-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedSeven, -point_seven), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedSeven, -point_sevenXY), Is.EqualTo(-point_sevenXY+point_seven).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedSeven.Value, float3.zero), Is.EqualTo(point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedSeven.Value, point_one), Is.EqualTo(point_seven+point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedSeven.Value, point_seven), Is.EqualTo(point_seven+point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedSeven.Value, point_sevenXY), Is.EqualTo(point_sevenXY+point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedSeven.Value, -point_one), Is.EqualTo(point_seven-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedSeven.Value, -point_seven), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalPoint(localToWorld_TranslatedSeven.Value, -point_sevenXY), Is.EqualTo(-point_sevenXY+point_seven).Using<float3>(EqualityWithTolerance));


            LocalToWorld localToWorld_TranslatedNegativeSeven = new LocalToWorld()
            {
                Value = float4x4.TRS(-point_seven, quaternion.identity, point_one)
            };
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedNegativeSeven, float3.zero), Is.EqualTo(-point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedNegativeSeven, point_one), Is.EqualTo(-point_seven+point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedNegativeSeven, point_seven), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedNegativeSeven, point_sevenXY), Is.EqualTo(point_sevenXY-point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedNegativeSeven, -point_one), Is.EqualTo(-point_seven-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedNegativeSeven, -point_seven), Is.EqualTo(-point_seven-point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedNegativeSeven, -point_sevenXY), Is.EqualTo(-point_sevenXY-point_seven).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedNegativeSeven.Value, float3.zero), Is.EqualTo(-point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedNegativeSeven.Value, point_one), Is.EqualTo(-point_seven+point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedNegativeSeven.Value, point_seven), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedNegativeSeven.Value, point_sevenXY), Is.EqualTo(point_sevenXY-point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedNegativeSeven.Value, -point_one), Is.EqualTo(-point_seven-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedNegativeSeven.Value, -point_seven), Is.EqualTo(-point_seven-point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedNegativeSeven.Value, -point_sevenXY), Is.EqualTo(-point_sevenXY-point_seven).Using<float3>(EqualityWithTolerance));


            LocalToWorld localToWorld_TranslatedSevenXY = new LocalToWorld()
            {
                Value = float4x4.TRS(point_sevenXY, quaternion.identity, point_one)
            };
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedSevenXY, float3.zero), Is.EqualTo(point_sevenXY).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedSevenXY, point_one), Is.EqualTo(point_one+point_sevenXY).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedSevenXY, point_seven), Is.EqualTo(point_seven+point_sevenXY).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedSevenXY, point_sevenXY), Is.EqualTo(point_sevenXY+point_sevenXY).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedSevenXY, -point_one), Is.EqualTo(point_sevenXY-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedSevenXY, -point_seven), Is.EqualTo(point_sevenXY-point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedSevenXY, -point_sevenXY), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedSevenXY.Value, float3.zero), Is.EqualTo(point_sevenXY).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedSevenXY.Value, point_one), Is.EqualTo(point_one+point_sevenXY).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedSevenXY.Value, point_seven), Is.EqualTo(point_seven+point_sevenXY).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedSevenXY.Value, point_sevenXY), Is.EqualTo(point_sevenXY+point_sevenXY).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedSevenXY.Value, -point_one), Is.EqualTo(point_sevenXY-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedSevenXY.Value, -point_seven), Is.EqualTo(point_sevenXY-point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_TranslatedSevenXY.Value, -point_sevenXY), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
        }

        [Test]
        public static void ConvertLocalToWorldPointTest_Rotate()
        {
            float3 point_one = new float3(1, 1, 1);
            float3 point_seven = point_one * 7f;

            LocalToWorld localToWorld_RotatedZ90 = new LocalToWorld()
            {
                Value = float4x4.TRS(float3.zero, quaternion.Euler(0, 0, math.radians(90)), point_one)
            };
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZ90, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZ90, point_one), Is.EqualTo(new float3(-1f, 1f, 1f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZ90, point_seven), Is.EqualTo(new float3(-7f, 7f, 7f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZ90, -point_one), Is.EqualTo(new float3(1f, -1f, -1f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZ90, -point_seven), Is.EqualTo(new float3(7f, -7f, -7f)).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZ90.Value, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZ90.Value, point_one), Is.EqualTo(new float3(-1f, 1f, 1f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZ90.Value, point_seven), Is.EqualTo(new float3(-7f, 7f, 7f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZ90.Value, -point_one), Is.EqualTo(new float3(1f, -1f, -1f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZ90.Value, -point_seven), Is.EqualTo(new float3(7f, -7f, -7f)).Using<float3>(EqualityWithTolerance));


            LocalToWorld localToWorld_RotatedZNegative90 = new LocalToWorld()
            {
                Value = float4x4.TRS(float3.zero, quaternion.Euler(0, 0, math.radians(-90)), point_one)
            };
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZNegative90, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZNegative90, point_one), Is.EqualTo(new float3(1f, -1f, 1f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZNegative90, point_seven), Is.EqualTo(new float3(7f, -7f, 7f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZNegative90, -point_one), Is.EqualTo(new float3(-1f, 1f, -1f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZNegative90, -point_seven), Is.EqualTo(new float3(-7f, 7f, -7f)).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZNegative90.Value, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZNegative90.Value, point_one), Is.EqualTo(new float3(1f, -1f, 1f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZNegative90.Value, point_seven), Is.EqualTo(new float3(7f, -7f, 7f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZNegative90.Value, -point_one), Is.EqualTo(new float3(-1f, 1f, -1f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZNegative90.Value, -point_seven), Is.EqualTo(new float3(-7f, 7f, -7f)).Using<float3>(EqualityWithTolerance));


            LocalToWorld localToWorld_RotatedZX90 = new LocalToWorld()
            {
                Value = float4x4.TRS(float3.zero, quaternion.Euler(math.radians(90), 0, math.radians(90)), point_one)
            };
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZX90, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZX90, point_one), Is.EqualTo(new float3(-1f, -1f, 1f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZX90, point_seven), Is.EqualTo(new float3(-7f, -7f, 7f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZX90, -point_one), Is.EqualTo(new float3(1f, 1f, -1f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZX90, -point_seven), Is.EqualTo(new float3(7f, 7f, -7f)).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZX90.Value, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZX90.Value, point_one), Is.EqualTo(new float3(-1f, -1f, 1f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZX90.Value, point_seven), Is.EqualTo(new float3(-7f, -7f, 7f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZX90.Value, -point_one), Is.EqualTo(new float3(1f, 1f, -1f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZX90.Value, -point_seven), Is.EqualTo(new float3(7f, 7f, -7f)).Using<float3>(EqualityWithTolerance));


            LocalToWorld localToWorld_RotatedZXNegative90 = new LocalToWorld()
            {
                Value = float4x4.TRS(float3.zero, quaternion.Euler(math.radians(-90), 0, math.radians(-90)), point_one)
            };
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZXNegative90, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZXNegative90, point_one), Is.EqualTo(point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZXNegative90, point_seven), Is.EqualTo(point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZXNegative90, -point_one), Is.EqualTo(-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZXNegative90, -point_seven), Is.EqualTo(-point_seven).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZXNegative90.Value, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZXNegative90.Value, point_one), Is.EqualTo(point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZXNegative90.Value, point_seven), Is.EqualTo(point_seven).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZXNegative90.Value, -point_one), Is.EqualTo(-point_one).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZXNegative90.Value, -point_seven), Is.EqualTo(-point_seven).Using<float3>(EqualityWithTolerance));


            LocalToWorld localToWorld_RotatedZX45 = new LocalToWorld()
            {
                Value = float4x4.TRS(float3.zero, quaternion.Euler(math.radians(45), 0, math.radians(45)), point_one)
            };
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZX45, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZX45, point_one), Is.EqualTo(new float3(0f,0.292893201f,1.70710683f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZX45, point_seven), Is.EqualTo(new float3(0f,2.0502522f,11.9497471f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZX45, -point_one), Is.EqualTo(new float3(0f,-0.292893201f,-1.70710683f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZX45, -point_seven), Is.EqualTo(new float3(0f,-2.0502522f,-11.9497471f)).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZX45.Value, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZX45.Value, point_one), Is.EqualTo(new float3(0f,0.292893201f,1.70710683f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZX45.Value, point_seven), Is.EqualTo(new float3(0f,2.0502522f,11.9497471f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZX45.Value, -point_one), Is.EqualTo(new float3(0f,-0.292893201f,-1.70710683f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_RotatedZX45.Value, -point_seven), Is.EqualTo(new float3(0f,-2.0502522f,-11.9497471f)).Using<float3>(EqualityWithTolerance));
        }

        [Test]
        public static void ConvertLocalToWorldPointTest_Scale()
        {
            float3 point_one = new float3(1, 1, 1);
            float3 point_seven = point_one * 7f;

            LocalToWorld localToWorld_Scaled2 = new LocalToWorld()
            {
                Value = float4x4.TRS(float3.zero, quaternion.identity, point_one*2f)
            };
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_Scaled2, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_Scaled2, point_one), Is.EqualTo(point_one*2f).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_Scaled2, point_seven), Is.EqualTo(point_seven*2f).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_Scaled2, -point_one), Is.EqualTo(-point_one*2f).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_Scaled2, -point_seven), Is.EqualTo(-point_seven*2f).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_Scaled2.Value, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_Scaled2.Value, point_one), Is.EqualTo(point_one*2f).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_Scaled2.Value, point_seven), Is.EqualTo(point_seven*2f).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_Scaled2.Value, -point_one), Is.EqualTo(-point_one*2f).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_Scaled2.Value, -point_seven), Is.EqualTo(-point_seven*2f).Using<float3>(EqualityWithTolerance));


            LocalToWorld localToWorld_ScaledNegative2 = new LocalToWorld()
            {
                Value = float4x4.TRS(float3.zero, quaternion.identity, point_one*-2f)
            };
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledNegative2, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledNegative2, point_one), Is.EqualTo(-point_one*2f).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledNegative2, point_seven), Is.EqualTo(-point_seven*2f).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledNegative2, -point_one), Is.EqualTo(point_one*2f).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledNegative2, -point_seven), Is.EqualTo(point_seven*2f).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledNegative2.Value, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledNegative2.Value, point_one), Is.EqualTo(-point_one*2f).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledNegative2.Value, point_seven), Is.EqualTo(-point_seven*2f).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledNegative2.Value, -point_one), Is.EqualTo(point_one*2f).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledNegative2.Value, -point_seven), Is.EqualTo(point_seven*2f).Using<float3>(EqualityWithTolerance));


            float3 scaleZ2 = new float3(point_one.xy, 2f);
            LocalToWorld localToWorld_ScaledZ2 = new LocalToWorld()
            {
                Value = float4x4.TRS(float3.zero, quaternion.identity, scaleZ2)
            };
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledZ2, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledZ2, point_one), Is.EqualTo(point_one*scaleZ2).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledZ2, point_seven), Is.EqualTo(point_seven*scaleZ2).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledZ2, -point_one), Is.EqualTo(-point_one*scaleZ2).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledZ2, -point_seven), Is.EqualTo(-point_seven*scaleZ2).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledZ2.Value, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledZ2.Value, point_one), Is.EqualTo(point_one*scaleZ2).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledZ2.Value, point_seven), Is.EqualTo(point_seven*scaleZ2).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledZ2.Value, -point_one), Is.EqualTo(-point_one*scaleZ2).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledZ2.Value, -point_seven), Is.EqualTo(-point_seven*scaleZ2).Using<float3>(EqualityWithTolerance));


            float3 scaleZNegative2 = new float3(point_one.xy, -2f);
            LocalToWorld localToWorld_ScaledZNegative2 = new LocalToWorld()
            {
                Value = float4x4.TRS(float3.zero, quaternion.identity, scaleZNegative2)
            };
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledZNegative2, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledZNegative2, point_one), Is.EqualTo(point_one*scaleZNegative2).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledZNegative2, point_seven), Is.EqualTo(point_seven*scaleZNegative2).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledZNegative2, -point_one), Is.EqualTo(-point_one*scaleZNegative2).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledZNegative2, -point_seven), Is.EqualTo(-point_seven*scaleZNegative2).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledZNegative2.Value, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledZNegative2.Value, point_one), Is.EqualTo(point_one*scaleZNegative2).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledZNegative2.Value, point_seven), Is.EqualTo(point_seven*scaleZNegative2).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledZNegative2.Value, -point_one), Is.EqualTo(-point_one*scaleZNegative2).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledZNegative2.Value, -point_seven), Is.EqualTo(-point_seven*scaleZNegative2).Using<float3>(EqualityWithTolerance));


            float3 scaleZXOnePointFive = new float3(1.5f, 1f, 1.5f);
            LocalToWorld localToWorld_ScaledZXOnePointFive = new LocalToWorld()
            {
                Value = float4x4.TRS(float3.zero, quaternion.identity, scaleZXOnePointFive)
            };
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledZXOnePointFive, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledZXOnePointFive, point_one), Is.EqualTo(point_one*scaleZXOnePointFive).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledZXOnePointFive, point_seven), Is.EqualTo(point_seven*scaleZXOnePointFive).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledZXOnePointFive, -point_one), Is.EqualTo(-point_one*scaleZXOnePointFive).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledZXOnePointFive, -point_seven), Is.EqualTo(-point_seven*scaleZXOnePointFive).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledZXOnePointFive.Value, float3.zero), Is.EqualTo(float3.zero).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledZXOnePointFive.Value, point_one), Is.EqualTo(point_one*scaleZXOnePointFive).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledZXOnePointFive.Value, point_seven), Is.EqualTo(point_seven*scaleZXOnePointFive).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledZXOnePointFive.Value, -point_one), Is.EqualTo(-point_one*scaleZXOnePointFive).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_ScaledZXOnePointFive.Value, -point_seven), Is.EqualTo(-point_seven*scaleZXOnePointFive).Using<float3>(EqualityWithTolerance));
        }

        [Test]
        public static void ConvertLocalToWorldPointTest_Compound()
        {
            float3 point_one = new float3(1f, 1f, 1f);
            float3 point_seven = point_one * 7f;
            float3 point_sevenXY = new float3(point_seven.xy, 0f);

            LocalToWorld localToWorld_Compound = new LocalToWorld()
            {
                Value = float4x4.TRS(point_sevenXY, quaternion.Euler(math.radians(45), 0, math.radians(45)), point_one*2f)
            };
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_Compound, float3.zero), Is.EqualTo(point_sevenXY).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_Compound, point_one), Is.EqualTo(new float3(7f, 7.58578634f, 3.41421366f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_Compound, point_seven), Is.EqualTo(new float3(7f, 11.1005039f, 23.8994942f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_Compound, -point_one), Is.EqualTo(new float3(7f,6.41421366f, -3.41421366f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_Compound, -point_seven), Is.EqualTo(new float3(7f, 2.89949608f, -23.8994942f)).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_Compound.Value, float3.zero), Is.EqualTo(point_sevenXY).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_Compound.Value, point_one), Is.EqualTo(new float3(7 ,7.58578634f, 3.41421366f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_Compound.Value, point_seven), Is.EqualTo(new float3(7 ,11.1005039f, 23.8994942f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_Compound.Value, -point_one), Is.EqualTo(new float3(7 ,6.41421366f, -3.41421366f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_Compound.Value, -point_seven), Is.EqualTo(new float3(7 ,2.89949608f, -23.8994942f)).Using<float3>(EqualityWithTolerance));


            LocalToWorld localToWorld_CompoundNegative = new LocalToWorld()
            {
                Value = float4x4.TRS(-point_sevenXY, quaternion.Euler(math.radians(-45), 0, math.radians(-45)), point_one*-2f)
            };
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_CompoundNegative, float3.zero), Is.EqualTo(-point_sevenXY).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_CompoundNegative, point_one), Is.EqualTo(new float3(-9.82842731f, -8.41421318f, -1.41421366f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_CompoundNegative, point_seven), Is.EqualTo(new float3(-26.7989922f, -16.8994961f, -9.89949512f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_CompoundNegative, -point_one), Is.EqualTo(new float3(-4.17157269f, -5.58578634f, 1.41421366f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_CompoundNegative, -point_seven), Is.EqualTo(new float3(12.7989922f, 2.89949608f, 9.89949512f)).Using<float3>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_CompoundNegative.Value, float3.zero), Is.EqualTo(-point_sevenXY).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_CompoundNegative.Value, point_one), Is.EqualTo(new float3(-9.82842731f, -8.41421318f, -1.41421366f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_CompoundNegative.Value, point_seven), Is.EqualTo(new float3(-26.7989922f, -16.8994961f, -9.89949512f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_CompoundNegative.Value, -point_one), Is.EqualTo(new float3(-4.17157269f, -5.58578634f, 1.41421366f)).Using<float3>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertLocalToWorldPoint(localToWorld_CompoundNegative.Value, -point_seven), Is.EqualTo(new float3(12.7989922f, 2.89949608f, 9.89949512f)).Using<float3>(EqualityWithTolerance));
        }

        // ----- ConvertWorldToLocalRotation ----- //
        [Test]
        public static void ConvertWorldToLocalRotation_Rotate()
        {
            float3 point_one = new float3(1f, 1f, 1f);
            quaternion rotation_fortyFive = quaternion.Euler(math.radians(45f), math.radians(45f), math.radians(45f));
            quaternion rotation_fortyFive_inverse = math.inverse(rotation_fortyFive);

            quaternion rotation_nintey = quaternion.Euler(math.radians(90f), math.radians(90f), math.radians(90f));
            quaternion rotation_nintey_inverse = math.inverse(rotation_nintey);

            quaternion rotation_ZfortyFive = quaternion.Euler(0f, 0f, math.radians(45f));
            quaternion rotation_ZfortyFive_inverse = math.inverse(rotation_ZfortyFive);

            quaternion rotation_Znintey = quaternion.Euler(0f, 0f, math.radians(90f));
            quaternion rotation_Znintey_inverse = math.inverse(rotation_Znintey);


            LocalToWorld localToWorld_nintey = new LocalToWorld()
            {
                Value = float4x4.TRS(float3.zero, rotation_nintey, point_one)
            };
            float4x4 worldToLocal_nintey = math.inverse(localToWorld_nintey.Value);
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(localToWorld_nintey, quaternion.identity), Is.EqualTo(math.mul(rotation_nintey_inverse, quaternion.identity)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(localToWorld_nintey, rotation_nintey), Is.EqualTo(math.mul(rotation_nintey_inverse, rotation_nintey)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(localToWorld_nintey, rotation_Znintey), Is.EqualTo(math.mul(rotation_nintey_inverse, rotation_Znintey)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(localToWorld_nintey, rotation_fortyFive), Is.EqualTo(math.mul(rotation_nintey_inverse, rotation_fortyFive)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(localToWorld_nintey, rotation_ZfortyFive), Is.EqualTo(math.mul(rotation_nintey_inverse, rotation_ZfortyFive)).Using<quaternion>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertWorldToLocalRotation(worldToLocal_nintey, quaternion.identity), Is.EqualTo(math.mul(rotation_nintey_inverse, quaternion.identity)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(worldToLocal_nintey, rotation_nintey), Is.EqualTo(math.mul(rotation_nintey_inverse, rotation_nintey)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(worldToLocal_nintey, rotation_Znintey), Is.EqualTo(math.mul(rotation_nintey_inverse, rotation_Znintey)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(worldToLocal_nintey, rotation_fortyFive), Is.EqualTo(math.mul(rotation_nintey_inverse, rotation_fortyFive)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(worldToLocal_nintey, rotation_ZfortyFive), Is.EqualTo(math.mul(rotation_nintey_inverse, rotation_ZfortyFive)).Using<quaternion>(EqualityWithTolerance));


            LocalToWorld localToWorld_fortyFive = new LocalToWorld()
            {
                Value = float4x4.TRS(float3.zero, rotation_fortyFive, point_one)
            };
            float4x4 worldToLocal_fortyFive = math.inverse(localToWorld_fortyFive.Value);
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(localToWorld_fortyFive, quaternion.identity), Is.EqualTo(math.mul(rotation_fortyFive_inverse, quaternion.identity)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(localToWorld_fortyFive, rotation_nintey), Is.EqualTo(math.mul(rotation_fortyFive_inverse, rotation_nintey)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(localToWorld_fortyFive, rotation_Znintey), Is.EqualTo(math.mul(rotation_fortyFive_inverse, rotation_Znintey)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(localToWorld_fortyFive, rotation_fortyFive), Is.EqualTo(math.mul(rotation_fortyFive_inverse, rotation_fortyFive)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(localToWorld_fortyFive, rotation_ZfortyFive), Is.EqualTo(math.mul(rotation_fortyFive_inverse, rotation_ZfortyFive)).Using<quaternion>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertWorldToLocalRotation(worldToLocal_fortyFive, quaternion.identity), Is.EqualTo(math.mul(rotation_fortyFive_inverse, quaternion.identity)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(worldToLocal_fortyFive, rotation_nintey), Is.EqualTo(math.mul(rotation_fortyFive_inverse, rotation_nintey)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(worldToLocal_fortyFive, rotation_Znintey), Is.EqualTo(math.mul(rotation_fortyFive_inverse, rotation_Znintey)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(worldToLocal_fortyFive, rotation_fortyFive), Is.EqualTo(math.mul(rotation_fortyFive_inverse, rotation_fortyFive)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(worldToLocal_fortyFive, rotation_ZfortyFive), Is.EqualTo(math.mul(rotation_fortyFive_inverse, rotation_ZfortyFive)).Using<quaternion>(EqualityWithTolerance));


            LocalToWorld localToWorld_Znintey = new LocalToWorld()
            {
                Value = float4x4.TRS(float3.zero, rotation_Znintey, point_one)
            };
            float4x4 worldToLocal_Znintey = math.inverse(localToWorld_Znintey.Value);
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(localToWorld_Znintey, quaternion.identity), Is.EqualTo(math.mul(rotation_Znintey_inverse, quaternion.identity)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(localToWorld_Znintey, rotation_nintey), Is.EqualTo(math.mul(rotation_Znintey_inverse, rotation_nintey)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(localToWorld_Znintey, rotation_Znintey), Is.EqualTo(math.mul(rotation_Znintey_inverse, rotation_Znintey)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(localToWorld_Znintey, rotation_fortyFive), Is.EqualTo(math.mul(rotation_Znintey_inverse, rotation_fortyFive)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(localToWorld_Znintey, rotation_ZfortyFive), Is.EqualTo(math.mul(rotation_Znintey_inverse, rotation_ZfortyFive)).Using<quaternion>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertWorldToLocalRotation(worldToLocal_Znintey, quaternion.identity), Is.EqualTo(math.mul(rotation_Znintey_inverse, quaternion.identity)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(worldToLocal_Znintey, rotation_nintey), Is.EqualTo(math.mul(rotation_Znintey_inverse, rotation_nintey)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(worldToLocal_Znintey, rotation_Znintey), Is.EqualTo(math.mul(rotation_Znintey_inverse, rotation_Znintey)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(worldToLocal_Znintey, rotation_fortyFive), Is.EqualTo(math.mul(rotation_Znintey_inverse, rotation_fortyFive)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(worldToLocal_Znintey, rotation_ZfortyFive), Is.EqualTo(math.mul(rotation_Znintey_inverse, rotation_ZfortyFive)).Using<quaternion>(EqualityWithTolerance));


            LocalToWorld localToWorld_ZfortyFive = new LocalToWorld()
            {
                Value = float4x4.TRS(float3.zero, rotation_ZfortyFive, point_one)
            };
            float4x4 worldToLocal_ZfortyFive = math.inverse(localToWorld_ZfortyFive.Value);
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(localToWorld_ZfortyFive, quaternion.identity), Is.EqualTo(math.mul(rotation_ZfortyFive_inverse, quaternion.identity)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(localToWorld_ZfortyFive, rotation_nintey), Is.EqualTo(math.mul(rotation_ZfortyFive_inverse, rotation_nintey)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(localToWorld_ZfortyFive, rotation_Znintey), Is.EqualTo(math.mul(rotation_ZfortyFive_inverse, rotation_Znintey)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(localToWorld_ZfortyFive, rotation_fortyFive), Is.EqualTo(math.mul(rotation_ZfortyFive_inverse, rotation_fortyFive)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(localToWorld_ZfortyFive, rotation_ZfortyFive), Is.EqualTo(math.mul(rotation_ZfortyFive_inverse, rotation_ZfortyFive)).Using<quaternion>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertWorldToLocalRotation(worldToLocal_ZfortyFive, quaternion.identity), Is.EqualTo(math.mul(rotation_ZfortyFive_inverse, quaternion.identity)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(worldToLocal_ZfortyFive, rotation_nintey), Is.EqualTo(math.mul(rotation_ZfortyFive_inverse, rotation_nintey)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(worldToLocal_ZfortyFive, rotation_Znintey), Is.EqualTo(math.mul(rotation_ZfortyFive_inverse, rotation_Znintey)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(worldToLocal_ZfortyFive, rotation_fortyFive), Is.EqualTo(math.mul(rotation_ZfortyFive_inverse, rotation_fortyFive)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(worldToLocal_ZfortyFive, rotation_ZfortyFive), Is.EqualTo(math.mul(rotation_ZfortyFive_inverse, rotation_ZfortyFive)).Using<quaternion>(EqualityWithTolerance));
        }

        [Test]
        public static void ConvertWorldToLocalRotation_Compound()
        {
            float3 point_one = new float3(1f, 1f, 1f);
            float3 point_sevenXY = new float3(7f, 7f, 0f);
            quaternion rotation_fortyFive = quaternion.Euler(math.radians(45f), math.radians(45f), math.radians(45f));
            quaternion rotation_nintey = quaternion.Euler(math.radians(90f), math.radians(90f), math.radians(90f));
            quaternion rotation_ZfortyFive = quaternion.Euler(0f, 0f, math.radians(45f));
            quaternion rotation_Znintey = quaternion.Euler(0f, 0f, math.radians(90f));

            quaternion rotation_XZ_fortyFive = quaternion.Euler(math.radians(45), 0, math.radians(45));
            quaternion rotation_XZ_fortyFive_inverse = math.inverse(rotation_XZ_fortyFive);
            quaternion rotation_XZ_negativeFortyFive = quaternion.Euler(math.radians(-45), 0, math.radians(-45));
            quaternion rotation_XZ_negativeFortyFive_inverse = math.inverse(rotation_XZ_negativeFortyFive);


            LocalToWorld localToWorld_compound = new LocalToWorld()
            {
                Value = float4x4.TRS(point_sevenXY, rotation_XZ_fortyFive, point_one*2f)
            };

            float4x4 worldToLocal_compound = math.inverse(localToWorld_compound.Value);
            // Assert.That(math.degrees(toEuler(localToWorld_compound.Value.GetRotation())), Is.EqualTo(math.degrees(toEuler(rotation_XZ_fortyFive))).Using<quaternion>(EqualityWithTolerance));

            // Assert.That(math.degrees(toEuler(TransformUtil.ConvertWorldToLocalRotation(localToWorld_compound, quaternion.identity))), Is.EqualTo(math.degrees(toEuler(math.mul(rotation_XZ_fortyFive_inverse, quaternion.identity)))).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(localToWorld_compound, quaternion.identity), Is.EqualTo(math.mul(rotation_XZ_fortyFive_inverse, quaternion.identity)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(localToWorld_compound, rotation_nintey), Is.EqualTo(math.mul(rotation_XZ_fortyFive_inverse, rotation_nintey)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(localToWorld_compound, rotation_Znintey), Is.EqualTo(math.mul(rotation_XZ_fortyFive_inverse, rotation_Znintey)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(localToWorld_compound, rotation_fortyFive), Is.EqualTo(math.mul(rotation_XZ_fortyFive_inverse, rotation_fortyFive)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(localToWorld_compound, rotation_ZfortyFive), Is.EqualTo(math.mul(rotation_XZ_fortyFive_inverse, rotation_ZfortyFive)).Using<quaternion>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertWorldToLocalRotation(worldToLocal_compound, quaternion.identity), Is.EqualTo(math.mul(rotation_XZ_fortyFive_inverse, quaternion.identity)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(worldToLocal_compound, rotation_nintey), Is.EqualTo(math.mul(rotation_XZ_fortyFive_inverse, rotation_nintey)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(worldToLocal_compound, rotation_Znintey), Is.EqualTo(math.mul(rotation_XZ_fortyFive_inverse, rotation_Znintey)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(worldToLocal_compound, rotation_fortyFive), Is.EqualTo(math.mul(rotation_XZ_fortyFive_inverse, rotation_fortyFive)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(worldToLocal_compound, rotation_ZfortyFive), Is.EqualTo(math.mul(rotation_XZ_fortyFive_inverse, rotation_ZfortyFive)).Using<quaternion>(EqualityWithTolerance));


            //TODO: HERE - ///Negative scale is the issue here. Issue decomposing negative scale since there are ambiguities.
            LocalToWorld localToWorld_compound_negative = new LocalToWorld()
            {
                // Value = float4x4.TRS(-point_sevenXY, rotation_XZ_negativeFortyFive, point_one*-2f)
                Value = float4x4.TRS(-point_sevenXY, rotation_XZ_negativeFortyFive, new float3(2f, 2f, -2f))
            };
            float4x4 worldToLocal_compound_negative = math.inverse(localToWorld_compound_negative.Value);

            Debug.Log("LTW Rot: " + math.degrees(localToWorld_compound_negative.Value.GetRotation().ToEuler()));
            Debug.Log("WTL Rot: " + math.degrees(worldToLocal_compound_negative.GetRotation().ToEuler()));

            Debug.Log("Component Direct: " + math.degrees(math.mul(rotation_XZ_negativeFortyFive_inverse, quaternion.identity).ToEuler()));
            Debug.Log("WTL Matrix: " + math.degrees(math.mul(worldToLocal_compound_negative.GetRotation(), quaternion.identity).ToEuler()));
            Debug.Log("WTL Matrix Scale: " + worldToLocal_compound_negative.GetScale());


            float4x4 trsComponentWise = float4x4.TRS(float3.zero, rotation_XZ_negativeFortyFive_inverse, 1f / (new float3(2f, 2f, -2f)));
            float4x4 trsUtil = float4x4.TRS(float3.zero, worldToLocal_compound_negative.GetRotation(), worldToLocal_compound_negative.GetScaleMagnitude());
            Debug.Log("-------------------");
            Debug.Log("TRS Component Wise: " + trsComponentWise);
            Debug.Log("TRS Util: " + trsUtil);
            Debug.Log("-");
            Debug.Log("TRS Component Wise Point: " + (math.transform(trsComponentWise, 5f)));
            Debug.Log("TRS Util Point: " + (math.transform(trsUtil, 5f)));
            Debug.Log("-------------------");

            Assert.That(math.degrees(TransformUtil.ConvertWorldToLocalRotation(localToWorld_compound_negative, quaternion.identity).ToEuler()),
                Is.EqualTo(math.degrees(math.mul(rotation_XZ_negativeFortyFive_inverse, quaternion.identity).ToEuler())).Using<quaternion>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertWorldToLocalRotation(localToWorld_compound_negative, quaternion.identity), Is.EqualTo(math.mul(rotation_XZ_negativeFortyFive_inverse, quaternion.identity)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(localToWorld_compound_negative, rotation_nintey), Is.EqualTo(math.mul(rotation_XZ_negativeFortyFive_inverse, rotation_nintey)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(localToWorld_compound_negative, rotation_Znintey), Is.EqualTo(math.mul(rotation_XZ_negativeFortyFive_inverse, rotation_Znintey)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(localToWorld_compound_negative, rotation_fortyFive), Is.EqualTo(math.mul(rotation_XZ_negativeFortyFive_inverse, rotation_fortyFive)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(localToWorld_compound_negative, rotation_ZfortyFive), Is.EqualTo(math.mul(rotation_XZ_negativeFortyFive_inverse, rotation_ZfortyFive)).Using<quaternion>(EqualityWithTolerance));

            Assert.That(TransformUtil.ConvertWorldToLocalRotation(worldToLocal_compound_negative, quaternion.identity), Is.EqualTo(math.mul(rotation_XZ_negativeFortyFive_inverse, quaternion.identity)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(worldToLocal_compound_negative, rotation_nintey), Is.EqualTo(math.mul(rotation_XZ_negativeFortyFive_inverse, rotation_nintey)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(worldToLocal_compound_negative, rotation_Znintey), Is.EqualTo(math.mul(rotation_XZ_negativeFortyFive_inverse, rotation_Znintey)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(worldToLocal_compound_negative, rotation_fortyFive), Is.EqualTo(math.mul(rotation_XZ_negativeFortyFive_inverse, rotation_fortyFive)).Using<quaternion>(EqualityWithTolerance));
            Assert.That(TransformUtil.ConvertWorldToLocalRotation(worldToLocal_compound_negative, rotation_ZfortyFive), Is.EqualTo(math.mul(rotation_XZ_negativeFortyFive_inverse, rotation_ZfortyFive)).Using<quaternion>(EqualityWithTolerance));
        }

        // ----- ConvertLocalToWorldRotation ----- //
        [Test]
        public static void ConvertLocalToWorldRotation()
        {

        }

        // [Test]
        // public static quaternion ConvertLocalToWorldRotationTest()
        // {
        //     return ConvertLocalToWorldRotation(localToWorld.Value, rotation);
        // }
        //
        // [Test]
        // public static float3 ConvertLocalToWorldScaleTest()
        // {
        //     return ConvertLocalToWorldScale(localToWorld.Value, scale);
        // }
        //
        // [Test]
        // public static Rect ConvertWorldToLocalRectTest()
        // {
        //     //TODO: #321 - Optimize...
        //     float4x4 worldToLocalMtx = math.inverse(localToWorld.Value);
        //
        //     float3 point1 = (Vector3)worldRect.min;
        //     float3 point2 = (Vector3)worldRect.max;
        //     float3 point3 = new float3(point1.x, point2.y, 0);
        //     float3 point4 = new float3(point2.x, point1.y, 0);
        //
        //     return RectUtil.CreateFromPoints(
        //         ConvertWorldToLocalPoint(worldToLocalMtx, point1).xy,
        //         ConvertWorldToLocalPoint(worldToLocalMtx, point2).xy,
        //         ConvertWorldToLocalPoint(worldToLocalMtx, point3).xy,
        //         ConvertWorldToLocalPoint(worldToLocalMtx, point4).xy
        //     );
        // }
        //
        // [Test]
        // public static Rect ConvertLocalToWorldRectTest()
        // {
        //     //TODO: #321 - Optimize...
        //     float4x4 worldToLocalMtx = math.inverse(localToWorld.Value);
        //
        //     float3 point1 = (Vector3)localRect.min;
        //     float3 point2 = (Vector3)localRect.max;
        //     float3 point3 = new float3(point1.x, point2.y, 0);
        //     float3 point4 = new float3(point2.x, point1.y, 0);
        //
        //     return RectUtil.CreateFromPoints(
        //         ConvertLocalToWorldPoint(worldToLocalMtx, point1).xy,
        //         ConvertLocalToWorldPoint(worldToLocalMtx, point2).xy,
        //         ConvertLocalToWorldPoint(worldToLocalMtx, point3).xy,
        //         ConvertLocalToWorldPoint(worldToLocalMtx, point4).xy
        //     );
        // }
    }
}