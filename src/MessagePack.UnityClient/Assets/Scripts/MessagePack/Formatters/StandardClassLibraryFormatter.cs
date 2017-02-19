﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MessagePack.Formatters
{
    // byte[], string, decimal, TimeSpan, DateTimeOffset, Guid, Uri, Version, KeyValuePair
    // NET40 -> BigInteger, Complex, Tuple

    // byte[] is special. represents bin type.
    public class ByteArrayFormatter : IMessagePackFormatter<byte[]>
    {
        public static readonly ByteArrayFormatter Instance = new ByteArrayFormatter();

        ByteArrayFormatter()
        {

        }

        public int Serialize(ref byte[] bytes, int offset, byte[] value)
        {
            if (value == null)
            {
                return MessagePackBinary.WriteNil(ref bytes, offset);
            }
            else
            {
                return MessagePackBinary.WriteBytes(ref bytes, offset, value);
            }
        }

        public byte[] Deserialize(byte[] bytes, int offset, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }
            else
            {
                return MessagePackBinary.ReadBytes(bytes, offset, out readSize);
            }
        }

        public int Serialize(ref byte[] bytes, int offset, byte[] value, IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return MessagePackBinary.WriteNil(ref bytes, offset);
            }
            else
            {
                return MessagePackBinary.WriteBytes(ref bytes, offset, value);
            }
        }

        public byte[] Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }
            else
            {
                return MessagePackBinary.ReadBytes(bytes, offset, out readSize);
            }
        }
    }

    public class NullableStringFormatter : IMessagePackFormatter<String>
    {
        public static readonly NullableStringFormatter Instance = new NullableStringFormatter();

        NullableStringFormatter()
        {

        }

        public int Serialize(ref byte[] bytes, int offset, String value)
        {
            if (value == null)
            {
                return MessagePackBinary.WriteNil(ref bytes, offset);
            }
            else
            {
                return MessagePackBinary.WriteString(ref bytes, offset, value);
            }
        }

        public String Deserialize(byte[] bytes, int offset, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }
            else
            {
                return MessagePackBinary.ReadString(bytes, offset, out readSize);
            }
        }

        public int Serialize(ref byte[] bytes, int offset, String value, IFormatterResolver typeResolver)
        {
            if (value == null)
            {
                return MessagePackBinary.WriteNil(ref bytes, offset);
            }
            else
            {
                return MessagePackBinary.WriteString(ref bytes, offset, value);
            }
        }

        public String Deserialize(byte[] bytes, int offset, IFormatterResolver typeResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }
            else
            {
                return MessagePackBinary.ReadString(bytes, offset, out readSize);
            }
        }
    }

    public class DecimalFormatter : IMessagePackFormatter<Decimal>
    {
        public static readonly DecimalFormatter Instance = new DecimalFormatter();


        DecimalFormatter()
        {

        }

        public int Serialize(ref byte[] bytes, int offset, decimal value)
        {
            return MessagePackBinary.WriteString(ref bytes, offset, value.ToString(CultureInfo.InvariantCulture));
        }

        public decimal Deserialize(byte[] bytes, int offset, out int readSize)
        {
            return decimal.Parse(MessagePackBinary.ReadString(bytes, offset, out readSize));
        }

        public int Serialize(ref byte[] bytes, int offset, decimal value, IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteString(ref bytes, offset, value.ToString(CultureInfo.InvariantCulture));
        }

        public decimal Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            return decimal.Parse(MessagePackBinary.ReadString(bytes, offset, out readSize));
        }
    }

    public class TimeSpanFormatter : IMessagePackFormatter<TimeSpan>
    {
        public static readonly IMessagePackFormatter<TimeSpan> Instance = new TimeSpanFormatter();

        TimeSpanFormatter()
        {

        }

        public int Serialize(ref byte[] bytes, int offset, TimeSpan value, IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteInt64(ref bytes, offset, value.Ticks);
        }

        public TimeSpan Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            return new TimeSpan(MessagePackBinary.ReadInt64(bytes, offset, out readSize));
        }
    }

    public class DateTimeOffsetFormatter : IMessagePackFormatter<DateTimeOffset>
    {
        public static readonly IMessagePackFormatter<DateTimeOffset> Instance = new DateTimeOffsetFormatter();


        DateTimeOffsetFormatter()
        {

        }

        public int Serialize(ref byte[] bytes, int offset, DateTimeOffset value, IFormatterResolver formatterResolver)
        {
            var startOffset = offset;
            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, 2);
            offset += MessagePackBinary.WriteDateTime(ref bytes, offset, value.UtcDateTime);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.Offset.Ticks);
            return offset - startOffset;
        }

        public DateTimeOffset Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            var startOffset = offset;
            var count = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            if (count != 2) throw new InvalidOperationException("Invalid DateTimeOffset format.");

            var utc = MessagePackBinary.ReadDateTime(bytes, offset, out readSize);
            offset += readSize;

            var dtOffsetTicks = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
            offset += readSize;

            readSize = offset - startOffset;

            return new DateTimeOffset(utc, new TimeSpan(dtOffsetTicks));
        }
    }

    public class GuidFormatter : IMessagePackFormatter<Guid>
    {
        public static readonly IMessagePackFormatter<Guid> Instance = new GuidFormatter();


        GuidFormatter()
        {

        }

        public int Serialize(ref byte[] bytes, int offset, Guid value, IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteString(ref bytes, offset, value.ToString());
        }

        public Guid Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            throw new NotImplementedException("Unity does not support Guid.Parse.");
        }
    }

    public class UriFormatter : IMessagePackFormatter<Uri>
    {
        public static readonly IMessagePackFormatter<Uri> Instance = new UriFormatter();


        UriFormatter()
        {

        }

        public int Serialize(ref byte[] bytes, int offset, Uri value, IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return MessagePackBinary.WriteNil(ref bytes, offset);
            }
            else
            {
                return MessagePackBinary.WriteString(ref bytes, offset, value.ToString());
            }
        }

        public Uri Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }
            else
            {
                return new Uri(MessagePackBinary.ReadString(bytes, offset, out readSize));
            }
        }
    }

    public class VersionFormatter : IMessagePackFormatter<Version>
    {
        public static readonly IMessagePackFormatter<Version> Instance = new VersionFormatter();

        VersionFormatter()
        {

        }

        public int Serialize(ref byte[] bytes, int offset, Version value, IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return MessagePackBinary.WriteNil(ref bytes, offset);
            }
            else
            {
                return MessagePackBinary.WriteString(ref bytes, offset, value.ToString());
            }
        }

        public Version Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }
            else
            {
                return new Version(MessagePackBinary.ReadString(bytes, offset, out readSize));
            }
        }
    }

    public class KeyValuePairFormatter<TKey, TValue> : IMessagePackFormatter<KeyValuePair<TKey, TValue>>
    {
        public int Serialize(ref byte[] bytes, int offset, KeyValuePair<TKey, TValue> value, IFormatterResolver formatterResolver)
        {
            var startOffset = offset;
            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, 2);
            offset += formatterResolver.GetFormatterWithVerify<TKey>().Serialize(ref bytes, offset, value.Key, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<TValue>().Serialize(ref bytes, offset, value.Value, formatterResolver);
            return offset - startOffset;
        }

        public KeyValuePair<TKey, TValue> Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            var startOffset = offset;
            var count = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            if (count != 2) throw new InvalidOperationException("Invalid KeyValuePair format.");

            var key = formatterResolver.GetFormatterWithVerify<TKey>().Deserialize(bytes, offset, formatterResolver, out readSize);
            offset += readSize;

            var value = formatterResolver.GetFormatterWithVerify<TValue>().Deserialize(bytes, offset, formatterResolver, out readSize);
            offset += readSize;

            readSize = offset - startOffset;
            return new KeyValuePair<TKey, TValue>(key, value);
        }
    }

}