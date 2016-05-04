// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Collections.Generic;
using System.Linq;
using FsCheck;
using Helios.Buffers;

namespace Helios.FsCheck.Tests.Buffers
{
    public class BufferGenerators
    {
        public static Gen<BufferOperations.IWrite> WriteInt()
        {
            return Arb.Default.Int32().Generator.Select(i => (BufferOperations.IWrite) new BufferOperations.WriteInt(i));
        }

        public static Gen<BufferOperations.IWrite> WriteUInt()
        {
            return
                Arb.Default.UInt32().Generator.Select(i => (BufferOperations.IWrite) new BufferOperations.WriteUInt(i));
        }

        public static Gen<BufferOperations.IWrite> WriteShort()
        {
            return
                Arb.Default.Int16().Generator.Select(s => (BufferOperations.IWrite) new BufferOperations.WriteShort(s));
        }

        public static Gen<BufferOperations.IWrite> WriteUShort()
        {
            return
                Arb.Default.UInt16()
                    .Generator.Select(s => (BufferOperations.IWrite) new BufferOperations.WriteUShort(s));
        }

        public static Gen<BufferOperations.IWrite> WriteLong()
        {
            return Arb.Default.Int64()
                .Generator.Select(l => (BufferOperations.IWrite) new BufferOperations.WriteLong(l));
        }

        public static Gen<BufferOperations.IWrite> WriteByte()
        {
            return Arb.Default.Byte().Generator.Select(b => (BufferOperations.IWrite) new BufferOperations.WriteByte(b));
        }

        public static Gen<BufferOperations.IWrite> WriteBytes()
        {
            return
                Gen.ListOf(Arb.Default.Byte().Generator)
                    .Select(bytes => (BufferOperations.IWrite) new BufferOperations.WriteBytes(bytes.ToArray()));
        }

        public static Gen<BufferOperations.IWrite> WriteBool()
        {
            return
                Arb.Default.Bool().Generator.Select(b => (BufferOperations.IWrite) new BufferOperations.WriteBoolean(b));
        }

        public static Gen<BufferOperations.IWrite> WriteChar()
        {
            return Arb.Default.Char().Generator.Select(c => (BufferOperations.IWrite) new BufferOperations.WriteChar(c));
        }

        public static Gen<BufferOperations.IWrite> DiscardReadBytes()
        {
            return Gen.Constant((BufferOperations.IWrite) new BufferOperations.DiscardReadBytes());
        }

        public static Gen<BufferOperations.IWrite> DiscardSomeReadBytes()
        {
            return Gen.Constant((BufferOperations.IWrite) new BufferOperations.DiscardSomeReadBytes());
        }

        public static Arbitrary<BufferOperations.IWrite[]> Writes()
        {
            var seq = Gen.Sequence(WriteChar(), WriteInt(), WriteBool(), WriteByte(), WriteBytes(), WriteLong(),
                WriteShort(), WriteUInt(), WriteUShort(), DiscardReadBytes(), DiscardSomeReadBytes());
            return Arb.From(seq);
        }

        public static Arbitrary<BufferSize> BufferSize()
        {
            return Arb.From(Gen.Choose(10, 1024).Select(i => new BufferSize(i, int.MaxValue)));
        }

        public static Arbitrary<IByteBuf> ByteBuf()
        {
            return Arb.From(Writes().Generator.Select(writes =>
            {
                const int initialCapacity = 1024;
                var buf = UnpooledByteBufAllocator.Default.Buffer(initialCapacity, initialCapacity*4);
                foreach (var write in writes)
                {
                    write.Execute(buf);
                }
                return buf;
            }));
        }
    }

    public class BufferSize
    {
        public BufferSize(int initialSize, int maxSize)
        {
            InitialSize = initialSize;
            MaxSize = maxSize;
        }

        public int InitialSize { get; }

        public int MaxSize { get; }

        public override string ToString()
        {
            return $"BufferSize(InitialSize = {InitialSize}, MaxSize = {MaxSize})";
        }
    }

    public class BufferContentsComparer : IEqualityComparer<object>
    {
        public new bool Equals(object x, object y)
        {
            var bytes = x as byte[];
            if (bytes != null && y is byte[])
            {
                var xBytes = bytes;
                var yBytes = (byte[]) y;
                return xBytes.SequenceEqual(yBytes);
            }

            return x.Equals(y);
        }

        public int GetHashCode(object obj)
        {
            return obj.GetHashCode();
        }
    }

    public class BufferOperations
    {
        public static readonly BufferContentsComparer Comparer = new BufferContentsComparer();

        public interface IWrite
        {
            object UntypedData { get; }

            /// <summary>
            ///     Returns the number of bytes written.
            /// </summary>
            int Execute(IByteBuf buf);

            IRead ToRead();
        }

        /// <summary>
        ///     A write operation we'll execute against a <see cref="IByteBuf" />
        /// </summary>
        /// <typeparam name="T">Primitive data type</typeparam>
        public abstract class Write<T> : IWrite
        {
            protected Write(T data)
            {
                Data = data;
            }

            public T Data { get; }

            public object UntypedData => Data;

            /// <summary>
            ///     Applies the write to the underlying <see cref="IByteBuf" />
            /// </summary>
            /// <param name="buf">The underlying buffer we're going to write to</param>
            /// <returns>The number of bytes written</returns>
            public int Execute(IByteBuf buf)
            {
                var currentLength = buf.WriterIndex;
                ExecuteInternal(buf);
                return buf.WriterIndex - currentLength;
            }

            public abstract IRead ToRead();

            protected abstract void ExecuteInternal(IByteBuf buf);

            public override string ToString()
            {
                return $"Write<{typeof (T)}>({Data})";
            }
        }

        public class WriteBoolean : Write<bool>
        {
            public WriteBoolean(bool data) : base(data)
            {
            }

            protected override void ExecuteInternal(IByteBuf buf)
            {
                buf = buf.WriteBoolean(Data);
            }

            public override IRead ToRead()
            {
                return new ReadBoolean();
            }
        }

        public class WriteInt : Write<int>
        {
            public WriteInt(int data) : base(data)
            {
            }

            protected override void ExecuteInternal(IByteBuf buf)
            {
                buf = buf.WriteInt(Data);
            }

            public override IRead ToRead()
            {
                return new ReadInt();
            }
        }

        public class WriteUInt : Write<uint>
        {
            public WriteUInt(uint data) : base(data)
            {
            }

            protected override void ExecuteInternal(IByteBuf buf)
            {
                buf = buf.WriteUnsignedInt(Data);
            }

            public override IRead ToRead()
            {
                return new ReadUInt();
            }
        }

        public class WriteShort : Write<short>
        {
            public WriteShort(short data) : base(data)
            {
            }

            protected override void ExecuteInternal(IByteBuf buf)
            {
                buf = buf.WriteShort(Data);
            }

            public override IRead ToRead()
            {
                return new ReadShort();
            }
        }

        public class WriteUShort : Write<ushort>
        {
            public WriteUShort(ushort data) : base(data)
            {
            }

            protected override void ExecuteInternal(IByteBuf buf)
            {
                buf = buf.WriteUnsignedShort(Data);
            }

            public override IRead ToRead()
            {
                return new ReadUShort();
            }
        }

        public class WriteLong : Write<long>
        {
            public WriteLong(long data) : base(data)
            {
            }

            protected override void ExecuteInternal(IByteBuf buf)
            {
                buf = buf.WriteLong(Data);
            }

            public override IRead ToRead()
            {
                return new ReadLong();
            }
        }

        public class WriteDouble : Write<double>
        {
            public WriteDouble(double data) : base(data)
            {
            }

            protected override void ExecuteInternal(IByteBuf buf)
            {
                buf = buf.WriteDouble(Data);
            }

            public override IRead ToRead()
            {
                return new ReadDouble();
            }
        }

        public class WriteByte : Write<byte>
        {
            public WriteByte(byte data) : base(data)
            {
            }

            protected override void ExecuteInternal(IByteBuf buf)
            {
                buf = buf.WriteByte(Data);
            }

            public override IRead ToRead()
            {
                return new ReadByte();
            }
        }

        public class WriteBytes : Write<byte[]>
        {
            public WriteBytes(byte[] data) : base(data)
            {
            }

            protected override void ExecuteInternal(IByteBuf buf)
            {
                buf = buf.WriteBytes(Data);
            }

            public override IRead ToRead()
            {
                return new ReadBytes(Data.Length);
            }

            public override string ToString()
            {
                return $"Write<byte[]>(Length = {Data.Length}, Data=[{string.Join("|", Data)}])";
            }
        }

        public class WriteChar : Write<char>
        {
            public WriteChar(char data) : base(data)
            {
            }

            protected override void ExecuteInternal(IByteBuf buf)
            {
                buf = buf.WriteChar(Data);
            }

            public override IRead ToRead()
            {
                return new ReadChar();
            }
        }

        /// <summary>
        ///     Not really a write operation, but we interleave it so as to verify the behavior following a few reads
        /// </summary>
        public class DiscardReadBytes : Write<byte>
        {
            public DiscardReadBytes() : base(0)
            {
            }

            protected override void ExecuteInternal(IByteBuf buf)
            {
                buf.DiscardReadBytes();
            }

            public override IRead ToRead()
            {
                return new ReadNoOp();
            }
        }

        public class DiscardSomeReadBytes : Write<byte>
        {
            public DiscardSomeReadBytes() : base(0)
            {
            }

            protected override void ExecuteInternal(IByteBuf buf)
            {
                buf.DiscardSomeReadBytes();
            }

            public override IRead ToRead()
            {
                return new ReadNoOp();
            }
        }

        public interface IRead
        {
            object Execute(IByteBuf buf);
        }

        public abstract class Read<T> : IRead
        {
            object IRead.Execute(IByteBuf buf)
            {
                return Execute(buf);
            }

            public abstract T Execute(IByteBuf buf);

            public override string ToString()
            {
                return $"Read<{typeof (T)}>()";
            }
        }

        /// <summary>
        ///     Used for <see cref="IWrite" /> operations that don't actually modify the buffer
        /// </summary>
        public class ReadNoOp : Read<byte>
        {
            public override byte Execute(IByteBuf buf)
            {
                return 0;
            }
        }

        public class ReadInt : Read<int>
        {
            public override int Execute(IByteBuf buf)
            {
                return buf.ReadInt();
            }
        }

        public class ReadUInt : Read<uint>
        {
            public override uint Execute(IByteBuf buf)
            {
                return buf.ReadUnsignedInt();
            }
        }

        public class ReadByte : Read<byte>
        {
            public override byte Execute(IByteBuf buf)
            {
                return buf.ReadByte();
            }
        }

        public class ReadBytes : Read<byte[]>
        {
            public ReadBytes(int byteLength)
            {
                ByteLength = byteLength;
            }

            public int ByteLength { get; }

            public override byte[] Execute(IByteBuf buf)
            {
                var readBuf = buf.ReadBytes(ByteLength);
                if (readBuf.IsReadable())
                    return readBuf.ToArray();
                return new byte[0];
            }

            public override string ToString()
            {
                return $"Read<byte[]>(Length = {ByteLength})";
            }
        }

        public class ReadBoolean : Read<bool>
        {
            public override bool Execute(IByteBuf buf)
            {
                return buf.ReadBoolean();
            }
        }

        public class ReadLong : Read<long>
        {
            public override long Execute(IByteBuf buf)
            {
                return buf.ReadLong();
            }
        }

        public class ReadDouble : Read<double>
        {
            public override double Execute(IByteBuf buf)
            {
                return buf.ReadDouble();
            }
        }

        public class ReadShort : Read<short>
        {
            public override short Execute(IByteBuf buf)
            {
                return buf.ReadShort();
            }
        }

        public class ReadUShort : Read<ushort>
        {
            public override ushort Execute(IByteBuf buf)
            {
                return buf.ReadUnsignedShort();
            }
        }

        public class ReadChar : Read<char>
        {
            public override char Execute(IByteBuf buf)
            {
                return buf.ReadChar();
            }
        }
    }
}

