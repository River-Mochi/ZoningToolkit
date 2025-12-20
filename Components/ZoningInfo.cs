// File: Components/ZoningInfo.cs
// Purpose: Zone Tools per-road settings
// Notes to future self:
// - Do not reorder enum members.
// - Do not change existing numeric values once released.
// - New members can be appended with explicit numeric values.
// - Serialized as uint enum value.
// - Default(ZoningMode) == Left because Left == 0. Always set explicitly when creating. Vanilla default is both sides.

namespace ZoningToolkit.Components
{
    using System;
    using Colossal.Serialization.Entities;
    using Unity.Entities;

    public enum ZoningMode : uint
    {
        Left = 0,
        Right = 1,
        Default = 2,
        None = 3
    }

    public struct ZoningInfo : IComponentData, IQueryTypeParameter, IEquatable<ZoningInfo>, ISerializable
    {
        // Saved value used when applying block size edits.
        public ZoningMode zoningMode;

        public readonly bool Equals(ZoningInfo other) => zoningMode == other.zoningMode;

        public override readonly int GetHashCode() => zoningMode.GetHashCode();

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write((uint)zoningMode);
        }

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.Read(out uint value);
            zoningMode = (ZoningMode)value;
        }
    }

    // Marker component: triggers update pass for existing blocks.
    public struct ZoningInfoUpdated : IComponentData, IQueryTypeParameter
    {
    }
}
