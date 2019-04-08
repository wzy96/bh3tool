
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProtoBuf
{
    /// <summary>
    /// A stateful reader, used to read a protobuf stream. Typical usage would be (sequentially) to call
    /// ReadFieldHeader and (after matching the field) an appropriate Read* method.
    /// </summary>
    public sealed class ProtoReader : IDisposable
    {
        Stream source;
        byte[] ioBuffer = new byte[4096];
        int depth, ioIndex, available;
        long blockEnd64, dataRemaining64;
        bool isFixedLength;

        // this is how many outstanding objects do not currently have
        // values for the purposes of reference tracking; we'll default
        // to just trapping the root object
        // note: objects are trapped (the ref and key mapped) via NoteObject
        uint trapCount; // uint is so we can use beq/bne more efficiently than bgt

        /// <summary>
        /// Gets the number of the field being processed.
        /// </summary>
        public int FieldNumber { get; private set; }

        /// <summary>
        /// Indicates the underlying proto serialization format on the wire.
        /// </summary>
        public WireType WireType { get; private set; }


        internal const long TO_EOF = -1;



        private static void Init(ProtoReader reader, Stream source, long length)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (!source.CanRead) throw new ArgumentException("Cannot read from stream", nameof(source));
            reader.source = source;
            reader.ioBuffer = new byte[source.Length];
            bool isFixedLength = length >= 0;
            reader.isFixedLength = isFixedLength;
            reader.dataRemaining64 = isFixedLength ? length : 0;

            reader.LongPosition = 0;
            reader.available = reader.depth = reader.FieldNumber = reader.ioIndex = 0;
            reader.blockEnd64 = long.MaxValue;
            reader.WireType = WireType.None;
            reader.trapCount = 1;
        }

        /// <summary>
        /// Releases resources used by the reader, but importantly <b>does not</b> Dispose the 
        /// underlying stream; in many typical use-cases the stream is used for different
        /// processes, so it is assumed that the consumer will Dispose their stream separately.
        /// </summary>
        public void Dispose()
        {
            // importantly, this does **not** own the stream, and does not dispose it
            source = null;
        }

        internal int TryReadUInt32VariantWithoutMoving(bool trimNegative, out uint value)
        {
            if (available < 10) Ensure(10, false);
            if (available == 0)
            {
                value = 0;
                return 0;
            }
            int readPos = ioIndex;
            value = ioBuffer[readPos++];
            if ((value & 0x80) == 0) return 1;
            value &= 0x7F;
            if (available == 1) throw EoF(this);

            uint chunk = ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 7;
            if ((chunk & 0x80) == 0) return 2;
            if (available == 2) throw EoF(this);

            chunk = ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 14;
            if ((chunk & 0x80) == 0) return 3;
            if (available == 3) throw EoF(this);

            chunk = ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 21;
            if ((chunk & 0x80) == 0) return 4;
            if (available == 4) throw EoF(this);

            chunk = ioBuffer[readPos];
            value |= chunk << 28; // can only use 4 bits from this chunk
            if ((chunk & 0xF0) == 0) return 5;

            if (trimNegative // allow for -ve values
                && (chunk & 0xF0) == 0xF0
                && available >= 10
                    && ioBuffer[++readPos] == 0xFF
                    && ioBuffer[++readPos] == 0xFF
                    && ioBuffer[++readPos] == 0xFF
                    && ioBuffer[++readPos] == 0xFF
                    && ioBuffer[++readPos] == 0x01)
            {
                return 10;
            }
            throw AddErrorData(new OverflowException(), this);
        }

        private uint ReadUInt32Variant(bool trimNegative)
        {
            int read = TryReadUInt32VariantWithoutMoving(trimNegative, out uint value);
            if (read > 0)
            {
                ioIndex += read;
                available -= read;
                LongPosition += read;
                return value;
            }
            throw EoF(this);
        }

        private bool TryReadUInt32Variant(out uint value)
        {
            int read = TryReadUInt32VariantWithoutMoving(false, out value);
            if (read > 0)
            {
                ioIndex += read;
                available -= read;
                LongPosition += read;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Reads an unsigned 32-bit integer from the stream; supported wire-types: Variant, Fixed32, Fixed64
        /// </summary>
        public uint ReadUInt32()
        {
            switch (WireType)
            {
                case WireType.Variant:
                    return ReadUInt32Variant(false);
                case WireType.Fixed32:
                    if (available < 4) Ensure(4, true);
                    LongPosition += 4;
                    available -= 4;
                    return ((uint)ioBuffer[ioIndex++])
                        | (((uint)ioBuffer[ioIndex++]) << 8)
                        | (((uint)ioBuffer[ioIndex++]) << 16)
                        | (((uint)ioBuffer[ioIndex++]) << 24);
                case WireType.Fixed64:
                    ulong val = ReadUInt64();
                    checked { return (uint)val; }
                default:
                    throw CreateWireTypeException();
            }
        }

        /// <summary>
        /// Returns the position of the current reader (note that this is not necessarily the same as the position
        /// in the underlying stream, if multiple readers are used on the same stream)
        /// </summary>
        public int Position { get { return checked((int)LongPosition); } }

        /// <summary>
        /// Returns the position of the current reader (note that this is not necessarily the same as the position
        /// in the underlying stream, if multiple readers are used on the same stream)
        /// </summary>
        public long LongPosition { get; private set; }
        internal void Ensure(int count, bool strict)
        {
            if (ioIndex + count >= ioBuffer.Length)
            {
                // need to shift the buffer data to the left to make space
                Buffer.BlockCopy(ioBuffer, ioIndex, ioBuffer, 0, available);
                ioIndex = 0;
            }
            count -= available;
            int writePos = ioIndex + available, bytesRead;
            int canRead = ioBuffer.Length - writePos;
            if (isFixedLength)
            {   // throttle it if needed
                if (dataRemaining64 < canRead) canRead = (int)dataRemaining64;
            }
            while (count > 0 && canRead > 0 && (bytesRead = source.Read(ioBuffer, writePos, canRead)) > 0)
            {
                available += bytesRead;
                count -= bytesRead;
                canRead -= bytesRead;
                writePos += bytesRead;
                if (isFixedLength) { dataRemaining64 -= bytesRead; }
            }
            if (strict && count > 0)
            {
                throw EoF(this);
            }

        }
        /// <summary>
        /// Reads a signed 16-bit integer from the stream: Variant, Fixed32, Fixed64, SignedVariant
        /// </summary>
        public short ReadInt16()
        {
            checked { return (short)ReadInt32(); }
        }
        /// <summary>
        /// Reads an unsigned 16-bit integer from the stream; supported wire-types: Variant, Fixed32, Fixed64
        /// </summary>
        public ushort ReadUInt16()
        {
            checked { return (ushort)ReadUInt32(); }
        }

        /// <summary>
        /// Reads an unsigned 8-bit integer from the stream; supported wire-types: Variant, Fixed32, Fixed64
        /// </summary>
        public byte ReadByte()
        {
            checked { return (byte)ReadUInt32(); }
        }

        /// <summary>
        /// Reads a signed 8-bit integer from the stream; supported wire-types: Variant, Fixed32, Fixed64, SignedVariant
        /// </summary>
        public sbyte ReadSByte()
        {
            checked { return (sbyte)ReadInt32(); }
        }

        /// <summary>
        /// Reads a signed 32-bit integer from the stream; supported wire-types: Variant, Fixed32, Fixed64, SignedVariant
        /// </summary>
        public int ReadInt32()
        {
            switch (WireType)
            {
                case WireType.Variant:
                    return (int)ReadUInt32Variant(true);
                case WireType.Fixed32:
                    if (available < 4) Ensure(4, true);
                    LongPosition += 4;
                    available -= 4;
                    return ((int)ioBuffer[ioIndex++])
                        | (((int)ioBuffer[ioIndex++]) << 8)
                        | (((int)ioBuffer[ioIndex++]) << 16)
                        | (((int)ioBuffer[ioIndex++]) << 24);
                case WireType.Fixed64:
                    long l = ReadInt64();
                    checked { return (int)l; }
                case WireType.SignedVariant:
                    return Zag(ReadUInt32Variant(true));
                default:
                    throw CreateWireTypeException();
            }
        }
        private const long Int64Msb = ((long)1) << 63;
        private const int Int32Msb = ((int)1) << 31;
        private static int Zag(uint ziggedValue)
        {
            int value = (int)ziggedValue;
            return (-(value & 0x01)) ^ ((value >> 1) & ~ProtoReader.Int32Msb);
        }

        private static long Zag(ulong ziggedValue)
        {
            long value = (long)ziggedValue;
            return (-(value & 0x01L)) ^ ((value >> 1) & ~ProtoReader.Int64Msb);
        }
        /// <summary>
        /// Reads a signed 64-bit integer from the stream; supported wire-types: Variant, Fixed32, Fixed64, SignedVariant
        /// </summary>
        public long ReadInt64()
        {
            switch (WireType)
            {
                case WireType.Variant:
                    return (long)ReadUInt64Variant();
                case WireType.Fixed32:
                    return ReadInt32();
                case WireType.Fixed64:
                    if (available < 8) Ensure(8, true);
                    LongPosition += 8;
                    available -= 8;

#if NETCOREAPP2_1
                    var result = System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(ioBuffer.AsSpan(ioIndex, 8));

                    ioIndex+= 8;

                    return result;
#else
                    return ((long)ioBuffer[ioIndex++])
                        | (((long)ioBuffer[ioIndex++]) << 8)
                        | (((long)ioBuffer[ioIndex++]) << 16)
                        | (((long)ioBuffer[ioIndex++]) << 24)
                        | (((long)ioBuffer[ioIndex++]) << 32)
                        | (((long)ioBuffer[ioIndex++]) << 40)
                        | (((long)ioBuffer[ioIndex++]) << 48)
                        | (((long)ioBuffer[ioIndex++]) << 56);
#endif
                case WireType.SignedVariant:
                    return Zag(ReadUInt64Variant());
                default:
                    throw CreateWireTypeException();
            }
        }

        private int TryReadUInt64VariantWithoutMoving(out ulong value)
        {
            if (available < 10) Ensure(10, false);
            if (available == 0)
            {
                value = 0;
                return 0;
            }
            int readPos = ioIndex;
            value = ioBuffer[readPos++];
            if ((value & 0x80) == 0) return 1;
            value &= 0x7F;
            if (available == 1) throw EoF(this);

            ulong chunk = ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 7;
            if ((chunk & 0x80) == 0) return 2;
            if (available == 2) throw EoF(this);

            chunk = ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 14;
            if ((chunk & 0x80) == 0) return 3;
            if (available == 3) throw EoF(this);

            chunk = ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 21;
            if ((chunk & 0x80) == 0) return 4;
            if (available == 4) throw EoF(this);

            chunk = ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 28;
            if ((chunk & 0x80) == 0) return 5;
            if (available == 5) throw EoF(this);

            chunk = ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 35;
            if ((chunk & 0x80) == 0) return 6;
            if (available == 6) throw EoF(this);

            chunk = ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 42;
            if ((chunk & 0x80) == 0) return 7;
            if (available == 7) throw EoF(this);


            chunk = ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 49;
            if ((chunk & 0x80) == 0) return 8;
            if (available == 8) throw EoF(this);

            chunk = ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 56;
            if ((chunk & 0x80) == 0) return 9;
            if (available == 9) throw EoF(this);

            chunk = ioBuffer[readPos];
            value |= chunk << 63; // can only use 1 bit from this chunk

            if ((chunk & ~(ulong)0x01) != 0) throw AddErrorData(new OverflowException(), this);
            return 10;
        }

        private ulong ReadUInt64Variant()
        {
            int read = TryReadUInt64VariantWithoutMoving(out ulong value);
            if (read > 0)
            {
                ioIndex += read;
                available -= read;
                LongPosition += read;
                return value;
            }
            throw EoF(this);
        }


#if COREFX
        static readonly Encoding encoding = Encoding.UTF8;
#else
        static readonly UTF8Encoding encoding = new UTF8Encoding();
#endif
        /// <summary>
        /// Reads a string from the stream (using UTF8); supported wire-types: String
        /// </summary>
        public string ReadString()
        {
            if (WireType == WireType.String)
            {
                int bytes = (int)ReadUInt32Variant(false);
                if (bytes == 0) return "";
                if (available < bytes) Ensure(bytes, true);

                string s = encoding.GetString(ioBuffer, ioIndex, bytes);
                available -= bytes;
                LongPosition += bytes;
                ioIndex += bytes;
                
                return s;
            }
            throw CreateWireTypeException();
        }
        /// <summary>
        ///  Reads bytes from the stream (using UTF8); supported wire-types: String
        /// </summary>
        public byte[] ReadStringBytes()
        {
            if(WireType==WireType.String)
            {
                int bytes = (int)ReadUInt32Variant(false);
                if (bytes == 0) return null;
                if (available < bytes) Ensure(bytes, true);

                byte[] b = new byte[bytes];
                Array.Copy(ioBuffer, ioIndex, b, 0, bytes);
                available -= bytes;
                LongPosition += bytes;
                ioIndex += bytes;

                return b;
            }
            throw CreateWireTypeException();
        }
        /// <summary>
        /// Throws an exception indication that the given value cannot be mapped to an enum.
        /// </summary>

        private Exception CreateWireTypeException()
        {
            return CreateException("Invalid wire-type; this usually means you have over-written a file without truncating or setting the length; see https://stackoverflow.com/q/2152978/23354");
        }

        private Exception CreateException(string message)
        {
            return new Exception();
        }
        /// <summary>
        /// Reads a double-precision number from the stream; supported wire-types: Fixed32, Fixed64
        /// </summary>
        public
#if !FEAT_SAFE
 unsafe
#endif
 double ReadDouble()
        {
            switch (WireType)
            {
                case WireType.Fixed32:
                    return ReadSingle();
                case WireType.Fixed64:
                    long value = ReadInt64();
#if FEAT_SAFE
                    return BitConverter.ToDouble(BitConverter.GetBytes(value), 0);
#else
                    return *(double*)&value;
#endif
                default:
                    throw CreateWireTypeException();
            }
        }
        /// <summary>
        /// Reads a field header from the stream, setting the wire-type and retuning the field number. If no
        /// more fields are available, then 0 is returned. This methods respects sub-messages.
        /// </summary>
        public int ReadFieldHeader()
        {
            // at the end of a group the caller must call EndSubItem to release the
            // reader (which moves the status to Error, since ReadFieldHeader must
            // then be called)
            if (blockEnd64 <= LongPosition || WireType == WireType.EndGroup) { return 0; }

            if (TryReadUInt32Variant(out uint tag) && tag != 0)
            {
                WireType = (WireType)(tag & 7);
                FieldNumber = (int)(tag >> 3);
                if (FieldNumber < 1)
                    throw new Exception();
            }
            else
            {
                WireType = WireType.None;
                FieldNumber = 0;
            }
            if (WireType == ProtoBuf.WireType.EndGroup)
            {
                if (depth > 0) return 0; // spoof an end, but note we still set the field-number
                throw new Exception("Unexpected end-group in source data; this usually means the source data is corrupt");
            }
            return FieldNumber;
        }
        /// <summary>
        /// Looks ahead to see whether the next field in the stream is what we expect
        /// (typically; what we've just finished reading - for example ot read successive list items)
        /// </summary>
        public bool TryReadFieldHeader(int field)
        {
            // check for virtual end of stream
            if (blockEnd64 <= LongPosition || WireType == WireType.EndGroup) { return false; }

            int read = TryReadUInt32VariantWithoutMoving(false, out uint tag);
            WireType tmpWireType; // need to catch this to exclude (early) any "end group" tokens
            if (read > 0 && ((int)tag >> 3) == field
                && (tmpWireType = (WireType)(tag & 7)) != WireType.EndGroup)
            {
                WireType = tmpWireType;
                FieldNumber = field;
                LongPosition += read;
                ioIndex += read;
                available -= read;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Compares the streams current wire-type to the hinted wire-type, updating the reader if necessary; for example,
        /// a Variant may be updated to SignedVariant. If the hinted wire-type is unrelated then no change is made.
        /// </summary>
        public void Hint(WireType wireType)
        {
            if (this.WireType == wireType) { }  // fine; everything as we expect
            else if (((int)wireType & 7) == (int)this.WireType)
            {   // the underling type is a match; we're customising it with an extension
                this.WireType = wireType;
            }
            // note no error here; we're OK about using alternative data
        }

        /// <summary>
        /// Verifies that the stream's current wire-type is as expected, or a specialized sub-type (for example,
        /// SignedVariant) - in which case the current wire-type is updated. Otherwise an exception is thrown.
        /// </summary>
        public void Assert(WireType wireType)
        {
            if (this.WireType == wireType) { }  // fine; everything as we expect
            else if (((int)wireType & 7) == (int)this.WireType)
            {   // the underling type is a match; we're customising it with an extension
                this.WireType = wireType;
            }
            else
            {   // nope; that is *not* what we were expecting!
                throw CreateWireTypeException();
            }
        }

        /// <summary>
        /// Discards the data for the current field.
        /// </summary>
        public void SkipField()
        {
            switch (WireType)
            {
                case WireType.Fixed32:
                    if (available < 4) Ensure(4, true);
                    available -= 4;
                    ioIndex += 4;
                    LongPosition += 4;
                    return;
                case WireType.Fixed64:
                    if (available < 8) Ensure(8, true);
                    available -= 8;
                    ioIndex += 8;
                    LongPosition += 8;
                    return;
                case WireType.String:
                    long len = (long)ReadUInt64Variant();
                    if (len <= available)
                    { // just jump it!
                        available -= (int)len;
                        ioIndex += (int)len;
                        LongPosition += len;
                        return;
                    }
                    // everything remaining in the buffer is garbage
                    LongPosition += len; // assumes success, but if it fails we're screwed anyway
                    len -= available; // discount anything we've got to-hand
                    ioIndex = available = 0; // note that we have no data in the buffer
                    if (isFixedLength)
                    {
                        if (len > dataRemaining64) throw EoF(this);
                        // else assume we're going to be OK
                        dataRemaining64 -= len;
                    }
                    ProtoReader.Seek(source, len, ioBuffer);
                    return;
                case WireType.Variant:
                case WireType.SignedVariant:
                    ReadUInt64Variant(); // and drop it
                    return;
                case WireType.StartGroup:
                    int originalFieldNumber = this.FieldNumber;
                    depth++; // need to satisfy the sanity-checks in ReadFieldHeader
                    while (ReadFieldHeader() > 0) { SkipField(); }
                    depth--;
                    if (WireType == WireType.EndGroup && FieldNumber == originalFieldNumber)
                    { // we expect to exit in a similar state to how we entered
                        WireType = ProtoBuf.WireType.None;
                        return;
                    }
                    throw CreateWireTypeException();
                case WireType.None: // treat as explicit errorr
                case WireType.EndGroup: // treat as explicit error
                default: // treat as implicit error
                    throw CreateWireTypeException();
            }
        }

        /// <summary>
        /// Reads an unsigned 64-bit integer from the stream; supported wire-types: Variant, Fixed32, Fixed64
        /// </summary>
        public ulong ReadUInt64()
        {
            switch (WireType)
            {
                case WireType.Variant:
                    return ReadUInt64Variant();
                case WireType.Fixed32:
                    return ReadUInt32();
                case WireType.Fixed64:
                    if (available < 8) Ensure(8, true);
                    LongPosition += 8;
                    available -= 8;

                    return ((ulong)ioBuffer[ioIndex++])
                        | (((ulong)ioBuffer[ioIndex++]) << 8)
                        | (((ulong)ioBuffer[ioIndex++]) << 16)
                        | (((ulong)ioBuffer[ioIndex++]) << 24)
                        | (((ulong)ioBuffer[ioIndex++]) << 32)
                        | (((ulong)ioBuffer[ioIndex++]) << 40)
                        | (((ulong)ioBuffer[ioIndex++]) << 48)
                        | (((ulong)ioBuffer[ioIndex++]) << 56);
                default:
                    throw CreateWireTypeException();
            }
        }
        /// <summary>
        /// Reads a single-precision number from the stream; supported wire-types: Fixed32, Fixed64
        /// </summary>
        public
#if !FEAT_SAFE
 unsafe
#endif
 float ReadSingle()
        {
            switch (WireType)
            {
                case WireType.Fixed32:
                    {
                        int value = ReadInt32();
#if FEAT_SAFE
                        return BitConverter.ToSingle(BitConverter.GetBytes(value), 0);
#else
                        return *(float*)&value;
#endif
                    }
                case WireType.Fixed64:
                    {
                        double value = ReadDouble();
                        float f = (float)value;
                        if (float.IsInfinity(f) && !double.IsInfinity(value))
                        {
                            throw AddErrorData(new OverflowException(), this);
                        }
                        return f;
                    }
                default:
                    throw CreateWireTypeException();
            }
        }

        /// <summary>
        /// Reads a boolean value from the stream; supported wire-types: Variant, Fixed32, Fixed64
        /// </summary>
        /// <returns></returns>
        public bool ReadBoolean()
        {
            return ReadUInt32() != 0;
        }

        private static readonly byte[] EmptyBlob = new byte[0];
        /// <summary>
        /// Reads a byte-sequence from the stream, appending them to an existing byte-sequence (which can be null); supported wire-types: String
        /// </summary>
        public static byte[] AppendBytes(byte[] value, ProtoReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            switch (reader.WireType)
            {
                case WireType.String:
                    int len = (int)reader.ReadUInt32Variant(false);
                    reader.WireType = WireType.None;
                    if (len == 0) return value ?? EmptyBlob;
                    int offset;
                    if (value == null || value.Length == 0)
                    {
                        offset = 0;
                        value = new byte[len];
                    }
                    else
                    {
                        offset = value.Length;
                        byte[] tmp = new byte[value.Length + len];
                        Buffer.BlockCopy(value, 0, tmp, 0, value.Length);
                        value = tmp;
                    }
                    // value is now sized with the final length, and (if necessary)
                    // contains the old data up to "offset"
                    reader.LongPosition += len; // assume success
                    while (len > reader.available)
                    {
                        if (reader.available > 0)
                        {
                            // copy what we *do* have
                            Buffer.BlockCopy(reader.ioBuffer, reader.ioIndex, value, offset, reader.available);
                            len -= reader.available;
                            offset += reader.available;
                            reader.ioIndex = reader.available = 0; // we've drained the buffer
                        }
                        //  now refill the buffer (without overflowing it)
                        int count = len > reader.ioBuffer.Length ? reader.ioBuffer.Length : len;
                        if (count > 0) reader.Ensure(count, true);
                    }
                    // at this point, we know that len <= available
                    if (len > 0)
                    {   // still need data, but we have enough buffered
                        Buffer.BlockCopy(reader.ioBuffer, reader.ioIndex, value, offset, len);
                        reader.ioIndex += len;
                        reader.available -= len;
                    }
                    return value;
                case WireType.Variant:
                    return new byte[0];
                default:
                    throw reader.CreateWireTypeException();
            }
        }

        //static byte[] ReadBytes(Stream stream, int length)
        //{
        //    if (stream == null) throw new ArgumentNullException("stream");
        //    if (length < 0) throw new ArgumentOutOfRangeException("length");
        //    byte[] buffer = new byte[length];
        //    int offset = 0, read;
        //    while (length > 0 && (read = stream.Read(buffer, offset, length)) > 0)
        //    {
        //        length -= read;
        //    }
        //    if (length > 0) throw EoF(null);
        //    return buffer;
        //}
        private static int ReadByteOrThrow(Stream source)
        {
            int val = source.ReadByte();
            if (val < 0) throw EoF(null);
            return val;
        }

        /// <summary>
        /// Reads a little-endian encoded integer. An exception is thrown if the data is not all available.
        /// </summary>
        public static int DirectReadLittleEndianInt32(Stream source)
        {
            return ReadByteOrThrow(source)
                | (ReadByteOrThrow(source) << 8)
                | (ReadByteOrThrow(source) << 16)
                | (ReadByteOrThrow(source) << 24);
        }

        /// <summary>
        /// Reads a big-endian encoded integer. An exception is thrown if the data is not all available.
        /// </summary>
        public static int DirectReadBigEndianInt32(Stream source)
        {
            return (ReadByteOrThrow(source) << 24)
                 | (ReadByteOrThrow(source) << 16)
                 | (ReadByteOrThrow(source) << 8)
                 | ReadByteOrThrow(source);
        }

        /// <summary>
        /// Reads a varint encoded integer. An exception is thrown if the data is not all available.
        /// </summary>
        public static int DirectReadVarintInt32(Stream source)
        {
            int bytes = TryReadUInt64Variant(source, out ulong val);
            if (bytes <= 0) throw EoF(null);
            return checked((int)val);
        }

        /// <summary>
        /// Reads a string (of a given lenth, in bytes) directly from the source into a pre-existing buffer. An exception is thrown if the data is not all available.
        /// </summary>
        public static void DirectReadBytes(Stream source, byte[] buffer, int offset, int count)
        {
            int read;
            if (source == null) throw new ArgumentNullException("source");
            while (count > 0 && (read = source.Read(buffer, offset, count)) > 0)
            {
                count -= read;
                offset += read;
            }
            if (count > 0) throw EoF(null);
        }

        /// <summary>
        /// Reads a given number of bytes directly from the source. An exception is thrown if the data is not all available.
        /// </summary>
        public static byte[] DirectReadBytes(Stream source, int count)
        {
            byte[] buffer = new byte[count];
            DirectReadBytes(source, buffer, 0, count);
            return buffer;
        }

        /// <summary>
        /// Reads a string (of a given lenth, in bytes) directly from the source. An exception is thrown if the data is not all available.
        /// </summary>
        public static string DirectReadString(Stream source, int length)
        {
            byte[] buffer = new byte[length];
            DirectReadBytes(source, buffer, 0, length);
            return Encoding.UTF8.GetString(buffer, 0, length);
        }


        /// <returns>The number of bytes consumed; 0 if no data available</returns>
        private static int TryReadUInt64Variant(Stream source, out ulong value)
        {
            value = 0;
            int b = source.ReadByte();
            if (b < 0) { return 0; }
            value = (uint)b;
            if ((value & 0x80) == 0) { return 1; }
            value &= 0x7F;
            int bytesRead = 1, shift = 7;
            while (bytesRead < 9)
            {
                b = source.ReadByte();
                if (b < 0) throw EoF(null);
                value |= ((ulong)b & 0x7F) << shift;
                shift += 7;
                bytesRead++;

                if ((b & 0x80) == 0) return bytesRead;
            }
            b = source.ReadByte();
            if (b < 0) throw EoF(null);
            if ((b & 1) == 0) // only use 1 bit from the last byte
            {
                value |= ((ulong)b & 0x7F) << shift;
                return ++bytesRead;
            }
            throw new OverflowException();
        }

        internal static void Seek(Stream source, long count, byte[] buffer)
        {
            if (source.CanSeek)
            {
                source.Seek(count, SeekOrigin.Current);
                count = 0;
            }
            else if (buffer != null)
            {
                int bytesRead;
                while (count > buffer.Length && (bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    count -= bytesRead;
                }
                while (count > 0 && (bytesRead = source.Read(buffer, 0, (int)count)) > 0)
                {
                    count -= bytesRead;
                }
            }
            else // borrow a buffer
            {
                throw new Exception();
            }
            if (count > 0) throw EoF(null);
        }
        internal static Exception AddErrorData(Exception exception, ProtoReader source)
        {
#if !CF && !PORTABLE
            if (exception != null && source != null && !exception.Data.Contains("protoSource"))
            {
                exception.Data.Add("protoSource", string.Format("tag={0}; wire-type={1}; offset={2}; depth={3}",
                    source.FieldNumber, source.WireType, source.LongPosition, source.depth));
            }
#endif
            return exception;
        }

        private static Exception EoF(ProtoReader source)
        {
            return AddErrorData(new EndOfStreamException(), source);
        }





        /// <summary>
        /// Indicates whether the reader still has data remaining in the current sub-item,
        /// additionally setting the wire-type for the next field if there is more data.
        /// This is used when decoding packed data.
        /// </summary>
        public static bool HasSubValue(WireType wireType, ProtoReader source)
        {
            if (source == null) throw new ArgumentNullException("source");
            // check for virtual end of stream
            if (source.blockEnd64 <= source.LongPosition || wireType == WireType.EndGroup) { return false; }
            source.WireType = wireType;
            return true;
        }

        public ProtoReader(Stream source, long length)
        {
            Init(this, source, length);
        }

        #region RECYCLER

        internal static ProtoReader Create(Stream source, int len)
            => Create(source, (long)len);
        /// <summary>
        /// Creates a new reader against a stream
        /// </summary>
        /// <param name="source">The source stream</param>
        /// <param name="model">The model to use for serialization; this can be null, but this will impair the ability to deserialize sub-objects</param>
        /// <param name="context">Additional context about this serialization operation</param>
        /// <param name="length">The number of bytes to read, or -1 to read until the end of the stream</param>
        public static ProtoReader Create(Stream source, long length = TO_EOF)
        {
            ProtoReader reader = GetRecycled();
            if (reader == null)
            {
                return new ProtoReader(source, length);
            }
            Init(reader, source, length);
            return reader;
        }

        [ThreadStatic]
        private static ProtoReader lastReader;

        private static ProtoReader GetRecycled()
        {
            ProtoReader tmp = lastReader;
            lastReader = null;
            return tmp;
        }


        #endregion
    }
}
