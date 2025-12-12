// Utils/BlockUtils.cs
// turns zoning area on/off by setting validArea.m_Area.w and block.m_Size.y to 0 or 6 depending on mode + side.
// Checks CellFlags.Occupied in the area and returns true if any cell is occupied. Protects occupied areas from zone changes.

namespace ZoningToolkit.Utils
{
    using Colossal.Mathematics;
    using Game.Common;
    using Game.Net;
    using Game.Zones;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;
    using ZoningToolkit.Components;

    internal static class BlockUtils
    {
        public static float blockCurveDotProduct(Block block, Curve curve)
        {
#if DEBUG
            Mod.s_Log.Debug($"Block direction {block.m_Direction}");
            Mod.s_Log.Debug($"Block position {block.m_Position}");
#endif

            MathUtils.Distance(curve.m_Bezier.xz, block.m_Position.xz, out float t);

            Vector2 tangent = GetTangent(curve.m_Bezier.xz, t);
            Vector2 perpendicular = new(tangent.y, -tangent.x);

            float dot = Vector2.Dot(perpendicular, block.m_Direction);

#if DEBUG
            Mod.s_Log.Debug($"Dot product: {dot}");
#endif

            return dot;
        }

        public static void deleteBlock(float dotProduct, ZoningInfo newZoningInfo, Entity blockEntity, EntityCommandBuffer ecb)
        {
            if (dotProduct > 0)
            {
                if (newZoningInfo.zoningMode == ZoningMode.Right || newZoningInfo.zoningMode == ZoningMode.None)
                {
                    ecb.RemoveComponent<Updated>(blockEntity);
                    ecb.RemoveComponent<Created>(blockEntity);
                    ecb.AddComponent(blockEntity, new Deleted());
                }
            }
            else
            {
                if (newZoningInfo.zoningMode == ZoningMode.Left || newZoningInfo.zoningMode == ZoningMode.None)
                {
                    ecb.RemoveComponent<Updated>(blockEntity);
                    ecb.RemoveComponent<Created>(blockEntity);
                    ecb.AddComponent(blockEntity, new Deleted());
                }
            }
        }

        public static void editBlockSizes(float dotProduct, ZoningInfo newZoningInfo, ValidArea validArea, Block block, Entity entity, EntityCommandBuffer ecb)
        {
            if (dotProduct > 0)
            {
                if (newZoningInfo.zoningMode == ZoningMode.Right || newZoningInfo.zoningMode == ZoningMode.None)
                {
                    validArea.m_Area.w = 0;
                    block.m_Size.y = 0;
                }
                else
                {
                    validArea.m_Area.w = 6;
                    block.m_Size.y = 6;
                }
            }
            else
            {
                if (newZoningInfo.zoningMode == ZoningMode.Left || newZoningInfo.zoningMode == ZoningMode.None)
                {
                    validArea.m_Area.w = 0;
                    block.m_Size.y = 0;
                }
                else
                {
                    validArea.m_Area.w = 6;
                    block.m_Size.y = 6;
                }
            }

            ecb.SetComponent(entity, validArea);
            ecb.SetComponent(entity, block);
        }

        public static bool isAnyCellOccupied(ref DynamicBuffer<Cell> cells, ref Block block, ref ValidArea validArea)
        {
#if DEBUG
            Mod.s_Log.Debug($"Block size x: {block.m_Size.x}, y: {block.m_Size.y}");
            Mod.s_Log.Debug($"Valid area x: {validArea.m_Area.x}, y: {validArea.m_Area.y}, z: {validArea.m_Area.z}, w: {validArea.m_Area.w}");
#endif

            if (validArea.m_Area.y * validArea.m_Area.w == 0)
                return false;

            for (int z = validArea.m_Area.z; z < validArea.m_Area.w; z++)
            {
                for (int x = validArea.m_Area.x; x < validArea.m_Area.y; x++)
                {
                    int index = z * block.m_Size.x + x;
                    if (index < 0 || index >= cells.Length)
                        continue;

                    Cell cell = cells[index];
                    if ((cell.m_State & CellFlags.Occupied) != 0)
                        return true;
                }
            }

            return false;
        }

        public static Vector2 GetTangent(Bezier4x2 curve, float t)
        {
            float2 derivative =
                3 * math.pow(1 - t, 2) * (curve.b - curve.a) +
                6 * (1 - t) * t * (curve.c - curve.b) +
                3 * math.pow(t, 2) * (curve.d - curve.c);

            return new Vector2(derivative.x, derivative.y);
        }
    }
}
