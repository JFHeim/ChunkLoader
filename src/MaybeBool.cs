// namespace ChunkLoader;
//
// using System;
//
// /// <summary>
// /// Represents a three-valued boolean logic system (Kleene logic).
// /// </summary>
// public readonly struct MaybeBool : IEquatable<MaybeBool>, IConvertible
// {
//     private readonly sbyte _value;
//
//     private MaybeBool(sbyte value) => _value = value;
//
//     public static readonly MaybeBool False = new MaybeBool(0);
//     public static readonly MaybeBool True = new MaybeBool(1);
//     public static readonly MaybeBool Maybe = new MaybeBool(-1);
//
//     #region Conversion
//
//     public static implicit operator MaybeBool(bool b) => b ? True : False;
//
//     /// <summary>
//     /// Explicitly converts a <see cref="MaybeBool"/> to a <see cref="bool"/>.
//     /// </summary>
//     /// <param name="mb">The <see cref="MaybeBool"/> value to convert.</param>
//     /// <returns>True if the state is True; False if the state is False.</returns>
//     /// <exception cref="InvalidCastException">Thrown when <paramref name="mb"/> is Maybe.</exception>
//     public static explicit operator bool(MaybeBool mb) => mb._value switch
//     {
//         1 => true,
//         0 => false,
//         _ => throw new InvalidCastException("Cannot cast 'Maybe' to bool.")
//     };
//
//     /// <summary>
//     /// Implicitly converts a <see cref="MaybeBool"/> to an <see cref="int"/>.
//     /// </summary>
//     /// <param name="mb">The <see cref="MaybeBool"/> value to convert.</param>
//     /// <returns>An <see cref="int"/>: 1 for True, 0 for False, -1 for Maybe.</returns>
//     public static implicit operator int(MaybeBool mb) => mb._value;
//
//     /// <summary>
//     /// Implicitly converts a <see cref="MaybeBool"/> to an <see cref="sbyte"/>.
//     /// </summary>
//     /// <param name="mb">The <see cref="MaybeBool"/> value to convert.</param>
//     /// <returns>An <see cref="sbyte"/>: 1 for True, 0 for False, -1 for Maybe.</returns>
//     public static implicit operator sbyte(MaybeBool mb) => mb._value;
//
//     /// <summary>
//     /// Explicitly converts an <see cref="int"/> to a <see cref="MaybeBool"/>.
//     /// </summary>
//     /// <param name="b">The int value (-1, 0, or 1).</param>
//     /// <returns>A <see cref="MaybeBool"/> representing the corresponding state.</returns>
//     /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="b"/> is not a valid state.</exception>
//     public static explicit operator MaybeBool(int b) => b switch
//     {
//         0 => False,
//         1 => True,
//         -1 => Maybe,
//         _ => throw new ArgumentOutOfRangeException(nameof(b))
//     };
//
//     /// <summary>
//     /// Explicitly converts an <see cref="sbyte"/> to a <see cref="MaybeBool"/>.
//     /// </summary>
//     /// <param name="b">The sbyte value (-1, 0, or 1).</param>
//     /// <returns>A <see cref="MaybeBool"/> representing the corresponding state.</returns>
//     /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="b"/> is not a valid state.</exception>
//     public static explicit operator MaybeBool(sbyte b) => b switch
//     {
//         0 => False,
//         1 => True,
//         -1 => Maybe,
//         _ => throw new ArgumentOutOfRangeException(nameof(b))
//     };
//
//     #endregion
//
//     #region Logic Operators
//
//     public static bool operator true(MaybeBool x) => x._value == 1;
//     public static bool operator false(MaybeBool x) => x._value == 0;
//
//     public static MaybeBool operator !(MaybeBool x) => x._value switch
//     {
//         1 => False,
//         0 => True,
//         _ => Maybe
//     };
//
//     public static MaybeBool operator &(MaybeBool x, MaybeBool y)
//     {
//         if (x._value == 0 || y._value == 0) return False;
//         if (x._value == 1 && y._value == 1) return True;
//         return Maybe;
//     }
//
//     public static MaybeBool operator |(MaybeBool x, MaybeBool y)
//     {
//         if (x._value == 1 || y._value == 1) return True;
//         if (x._value == 0 && y._value == 0) return False;
//         return Maybe;
//     }
//
//     /// <summary>
//     /// Performs exclusive OR. If either is Maybe, result is Maybe.
//     /// </summary>
//     public static MaybeBool operator ^(MaybeBool x, MaybeBool y)
//     {
//         if (x._value == -1 || y._value == -1) return Maybe;
//         return x._value != y._value; // Returns True if different, False if same
//     }
//
//     public static bool operator ==(MaybeBool x, MaybeBool y) => x._value == y._value;
//     public static bool operator !=(MaybeBool x, MaybeBool y) => x._value != y._value;
//
//     #endregion
//
//     #region Standard Overrides
//
//     public override bool Equals(object? obj) => obj is MaybeBool mb && Equals(mb);
//     public bool Equals(MaybeBool other) => _value == other._value;
//     public override int GetHashCode() => _value.GetHashCode();
//     public override string ToString() => _value switch
//     {
//         1 => nameof(True),
//         0 => nameof(False),
//         _ => nameof(Maybe)
//     };
//
//     #endregion
//
//     #region IConvertible (Reflection & Serialization support)
//
//     TypeCode IConvertible.GetTypeCode() => TypeCode.SByte;
//     bool IConvertible.ToBoolean(IFormatProvider? provider) => ((IConvertible)_value).ToBoolean(provider);
//     sbyte IConvertible.ToSByte(IFormatProvider? provider) => _value;
//
//     // Остальные методы IConvertible реализуются через (IConvertible)_value (явное приведение) или кидают NotSupported
//     byte IConvertible.ToByte(IFormatProvider? provider) => (byte)_value;
//     char IConvertible.ToChar(IFormatProvider? provider) => throw new NotSupportedException();
//     DateTime IConvertible.ToDateTime(IFormatProvider? provider) => throw new NotSupportedException();
//     decimal IConvertible.ToDecimal(IFormatProvider? provider) => _value;
//     double IConvertible.ToDouble(IFormatProvider? provider) => _value;
//     short IConvertible.ToInt16(IFormatProvider? provider) => _value;
//     int IConvertible.ToInt32(IFormatProvider? provider) => _value;
//     long IConvertible.ToInt64(IFormatProvider? provider) => _value;
//     float IConvertible.ToSingle(IFormatProvider? provider) => _value;
//     string IConvertible.ToString(IFormatProvider? provider) => ToString();
//     object IConvertible.ToType(Type conversionType, IFormatProvider? provider) => Convert.ChangeType(_value, conversionType, provider);
//     ushort IConvertible.ToUInt16(IFormatProvider? provider) => (ushort)_value;
//     uint IConvertible.ToUInt32(IFormatProvider? provider) => (uint)_value;
//     ulong IConvertible.ToUInt64(IFormatProvider? provider) => (ulong)_value;
//
//     #endregion
// }