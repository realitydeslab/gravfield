using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;


public struct EffectParameter_Float : INetworkSerializable, System.IEquatable<EffectParameter_Float>
{
    public FixedString64Bytes address;
    public float value;


    public EffectParameter_Float(string _address, int _value) : this()
    {
        this.address = _address;
        this.value = _value;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out address);
            reader.ReadValueSafe(out value);
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(address);
            writer.WriteValueSafe(value);
        }
    }

    public bool Equals(EffectParameter_Float other)
    {
        return address == other.address && value == other.value;
    }
}
