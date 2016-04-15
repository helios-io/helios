using System.Linq;
using FsCheck;
using Helios.Buffers;

namespace Helios.FsCheck.Tests.Buffers
{
    public class BufferGenerators
    {
        public static Gen<BufferOperations.IWrite> WriteInt()
        {
            return Arb.Default.Int32().Generator.Select(i => (BufferOperations.IWrite)new BufferOperations.WriteInt(i));
        }

        public static Gen<BufferOperations.IWrite> WriteUInt()
        {
            return Arb.Default.UInt32().Generator.Select(i => (BufferOperations.IWrite)new BufferOperations.WriteUInt(i));
        }

        public static Gen<BufferOperations.IWrite> WriteShort()
        {
            return Arb.Default.Int16().Generator.Select(s => (BufferOperations.IWrite)new BufferOperations.WriteShort(s));
        }

        public static Gen<BufferOperations.IWrite> WriteUShort()
        {
            return Arb.Default.UInt16().Generator.Select(s => (BufferOperations.IWrite)new BufferOperations.WriteUShort(s));
        }

        public static Gen<BufferOperations.IWrite> WriteLong()
        {
            return Arb.Default.Int64().Generator.Select(l => (BufferOperations.IWrite)new BufferOperations.WriteLong(l));
        }

        public static Gen<BufferOperations.IWrite> WriteByte()
        {
            return Arb.Default.Byte().Generator.Select(b => (BufferOperations.IWrite)(new BufferOperations.WriteByte(b)));
        }

        public static Gen<BufferOperations.IWrite> WriteBytes()
        {
            return Gen.ListOf(Arb.Default.Byte().Generator).Select(bytes => (BufferOperations.IWrite)(new BufferOperations.WriteBytes(bytes.ToArray())));
        }

        public static Gen<BufferOperations.IWrite> WriteBool()
        {
            return Arb.Default.Bool().Generator.Select(b => (BufferOperations.IWrite)(new BufferOperations.WriteBoolean(b)));
        }

        public static Gen<BufferOperations.IWrite> WriteChar()
        {
            return Arb.Default.Char().Generator.Select(c => (BufferOperations.IWrite)(new BufferOperations.WriteChar(c)));
        }

        public static Arbitrary<BufferOperations.IWrite[]> Writes()
        {
            var seq = (Gen.Sequence<BufferOperations.IWrite>(WriteChar(), WriteInt(), WriteBool(), WriteByte(), WriteBytes(), WriteLong(),
                    WriteShort(), WriteUInt(), WriteUShort()));
            return Arb.From(seq);
        }

    }

    public class BufferOperations
    {
        public interface IWrite
        {
            /// <summary>
            /// Returns the number of bytes written.
            /// </summary>
            int Execute(IByteBuf buf);
        }

        /// <summary>
        /// A write operation we'll execute against a <see cref="IByteBuf"/>
        /// </summary>
        /// <typeparam name="T">Primitive data type</typeparam>
        public abstract class Write<T> : IWrite
        {
            protected Write(T data)
            {
                Data = data;
            }

            public T Data { get; private set; }

            /// <summary>
            /// Applies the write to the underlying <see cref="IByteBuf"/>
            /// </summary>
            /// <param name="buf">The underlying buffer we're going to write to</param>
            /// <returns>The number of bytes written</returns>
            public int Execute(IByteBuf buf)
            {
                var currentLength = buf.WriterIndex;
                ExecuteInternal(buf);
                return buf.WriterIndex - currentLength;
            }

            protected abstract void ExecuteInternal(IByteBuf buf);

            public abstract Read<T> ToRead();
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

            public override Read<bool> ToRead()
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

            public override Read<int> ToRead()
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

            public override Read<uint> ToRead()
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

            public override Read<short> ToRead()
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

            public override Read<ushort> ToRead()
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

            public override Read<long> ToRead()
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

            public override Read<double> ToRead()
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

            public override Read<byte> ToRead()
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

            public override Read<byte[]> ToRead()
            {
                return new ReadBytes(Data.Length);
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

            public override Read<char> ToRead()
            {
                return new ReadChar();
            }
        }

        public interface IRead
        {
            object Execute(IByteBuf buf);
        }

        public abstract class Read<T> : IRead
        {
            public abstract T Execute(IByteBuf buf);

            object IRead.Execute(IByteBuf buf)
            {
                return Execute(buf);
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
                return buf.ReadBytes(ByteLength).ToArray();
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
