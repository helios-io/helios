// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Text;

namespace Helios.Buffers
{
    /// <summary>
    ///     Inspired by the Netty ByteBuffer implementation
    ///     (https://github.com/netty/netty/blob/master/buffer/src/main/java/io/netty/buffer/ByteBuf.java)
    ///     Provides circular-buffer-esque security around a byte array, allowing reads and writes to occur independently.
    ///     In general, the <see cref="IByteBuf" /> guarantees:
    ///     * <see cref="ReaderIndex" /> LESS THAN OR EQUAL TO <see cref="WriterIndex" /> LESS THAN OR EQUAL TO
    ///     <see cref="Capacity" />.
    /// </summary>
    public interface IByteBuf : IReferenceCounted
    {
        int Capacity { get; }

        /// <summary>
        ///     The byte order of the buffer. <see cref="ByteOrder.LittleEndian" /> by default.
        /// </summary>
        ByteOrder Order { get; }

        int MaxCapacity { get; }

        /// <summary>
        ///     The allocator who created this buffer
        /// </summary>
        IByteBufAllocator Allocator { get; }

        int ReaderIndex { get; }

        int WriterIndex { get; }

        int ReadableBytes { get; }

        int WritableBytes { get; }

        int MaxWritableBytes { get; }

        /// <summary>
        ///     Flag that indicates if this <see cref="IByteBuf" /> is backed by a byte array or not
        /// </summary>
        bool HasArray { get; }

        /// <summary>
        ///     Grabs the underlying byte array for this buffer
        /// </summary>
        /// <value></value>
        byte[] Array { get; }

        /// <summary>
        ///     Is this a direct buffer or not, e.g. is it backed by a simple byte array?
        /// </summary>
        bool IsDirect { get; }

        int ArrayOffset { get; }

        /// <summary>
        ///     Expands the capacity of this buffer so long as it is less than <see cref="MaxCapacity" />.
        /// </summary>
        IByteBuf AdjustCapacity(int newCapacity);

        /// <summary>
        ///     Returns a
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        IByteBuf WithOrder(ByteOrder order);

        /// <summary>
        ///     Sets the <see cref="WriterIndex" /> of this buffer
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">thrown if <see cref="WriterIndex" /> exceeds the length of the buffer</exception>
        IByteBuf SetWriterIndex(int writerIndex);

        /// <summary>
        ///     Sets the <see cref="ReaderIndex" /> of this buffer
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     thrown if <see cref="ReaderIndex" /> is greater than
        ///     <see cref="WriterIndex" /> or less than <c>0</c>.
        /// </exception>
        IByteBuf SetReaderIndex(int readerIndex);

        /// <summary>
        ///     Sets both indexes
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     thrown if <see cref="WriterIndex" /> or <see cref="ReaderIndex" /> exceeds
        ///     the length of the buffer
        /// </exception>
        IByteBuf SetIndex(int readerIndex, int writerIndex);

        /// <summary>
        ///     Returns true if <see cref="WriterIndex" /> - <see cref="ReaderIndex" /> is greater than <c>0</c>.
        /// </summary>
        bool IsReadable();

        /// <summary>
        ///     Is the buffer readable if and only if the buffer contains equal or more than the specified number of elements
        /// </summary>
        /// <param name="size">The number of elements we would like to read</param>
        bool IsReadable(int size);

        /// <summary>
        ///     Returns true if and only if <see cref="Capacity" /> - <see cref="WriterIndex" /> is greater than zero.
        /// </summary>
        bool IsWritable();

        /// <summary>
        ///     Returns true if and only if the buffer has enough <see cref="Capacity" /> to accommodate <see cref="size" />
        ///     additional bytes.
        /// </summary>
        /// <param name="size">The number of additional elements we would like to write.</param>
        bool IsWritable(int size);

        /// <summary>
        ///     Sets the <see cref="WriterIndex" /> and <see cref="ReaderIndex" /> to <c>0</c>. Does not erase any of the data
        ///     written into the buffer already,
        ///     but it will overwrite that data.
        /// </summary>
        IByteBuf Clear();

        /// <summary>
        ///     Marks the current <see cref="ReaderIndex" /> in this buffer. You can reposition the current
        ///     <see cref="ReaderIndex" />
        ///     to the marked <see cref="ReaderIndex" /> by calling <see cref="ResetReaderIndex" />.
        ///     The initial value of the marked <see cref="ReaderIndex" /> is <c>0</c>.
        /// </summary>
        IByteBuf MarkReaderIndex();

        /// <summary>
        ///     Repositions the current <see cref="ReaderIndex" /> to the marked <see cref="ReaderIndex" /> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     is thrown if the current <see cref="WriterIndex" /> is less than the
        ///     marked <see cref="ReaderIndex" />
        /// </exception>
        IByteBuf ResetReaderIndex();

        /// <summary>
        ///     Marks the current <see cref="WriterIndex" /> in this buffer. You can reposition the current
        ///     <see cref="WriterIndex" />
        ///     to the marked <see cref="WriterIndex" /> by calling <see cref="ResetWriterIndex" />.
        ///     The initial value of the marked <see cref="WriterIndex" /> is <c>0</c>.
        /// </summary>
        IByteBuf MarkWriterIndex();

        /// <summary>
        ///     Repositions the current <see cref="WriterIndex" /> to the marked <see cref="WriterIndex" /> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     is thrown if the current <see cref="ReaderIndex" /> is greater than the
        ///     marked <see cref="WriterIndex" />
        /// </exception>
        IByteBuf ResetWriterIndex();

        /// <summary>
        ///     Discards the bytes between the 0th index and <see cref="ReaderIndex" />.
        ///     It moves the bytes between <see cref="ReaderIndex" /> and <see cref="WriterIndex" /> to the 0th index,
        ///     and sets <see cref="ReaderIndex" /> and <see cref="WriterIndex" /> to <c>0</c> and
        ///     <c>oldWriterIndex - oldReaderIndex</c> respectively.
        /// </summary>
        /// <returns></returns>
        IByteBuf DiscardReadBytes();

        /// <summary>
        ///     Similar to <see cref="DiscardReadBytes" /> except that this method might discard some, all, or none of read bytes
        ///     depending on its internal implementation to reduce overall memory bandwidth consumption at the cost of potentially
        ///     additional total memory consumption.
        /// </summary>
        IByteBuf DiscardSomeReadBytes();

        /// <summary>
        ///     Makes sure the number of <see cref="WritableBytes" /> is equal to or greater than
        ///     the specified value (<see cref="minWritableBytes" />.) If there is enough writable bytes in this buffer,
        ///     the method returns with no side effect. Otherwise, it raises an <see cref="ArgumentOutOfRangeException" />.
        /// </summary>
        /// <param name="minWritableBytes">The expected number of minimum writable bytes</param>
        /// <exception cref="IndexOutOfRangeException">
        ///     if <see cref="WriterIndex" /> + <see cref="minWritableBytes" /> >
        ///     <see cref="MaxCapacity" />.
        /// </exception>
        IByteBuf EnsureWritable(int minWritableBytes);

        /// <summary>
        ///     Gets a boolean at the specified absolute <see cref="index" /> in this buffer.
        ///     This method does not modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" />
        ///     of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <see cref="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        bool GetBoolean(int index);

        /// <summary>
        ///     Gets a byte at the specified absolute <see cref="index" /> in this buffer.
        ///     This method does not modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" />
        ///     of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <see cref="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        byte GetByte(int index);

        /// <summary>
        ///     Gets a short at the specified absolute <see cref="index" /> in this buffer.
        ///     This method does not modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" />
        ///     of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <see cref="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        short GetShort(int index);

        /// <summary>
        ///     Gets an ushort at the specified absolute <see cref="index" /> in this buffer.
        ///     This method does not modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" />
        ///     of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <see cref="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        ushort GetUnsignedShort(int index);

        /// <summary>
        ///     Gets an integer at the specified absolute <see cref="index" /> in this buffer.
        ///     This method does not modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" />
        ///     of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <see cref="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        int GetInt(int index);

        /// <summary>
        ///     Gets an unsigned integer at the specified absolute <see cref="index" /> in this buffer.
        ///     This method does not modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" />
        ///     of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <see cref="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        uint GetUnsignedInt(int index);

        /// <summary>
        ///     Gets a long integer at the specified absolute <see cref="index" /> in this buffer.
        ///     This method does not modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" />
        ///     of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <see cref="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        long GetLong(int index);

        /// <summary>
        ///     Gets a char at the specified absolute <see cref="index" /> in this buffer.
        ///     This method does not modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" />
        ///     of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <see cref="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        char GetChar(int index);

        /// <summary>
        ///     Gets a double at the specified absolute <see cref="index" /> in this buffer.
        ///     This method does not modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" />
        ///     of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <see cref="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        double GetDouble(int index);

        /// <summary>
        ///     Transfers this buffers data to the specified <see cref="destination" /> buffer starting at the specified
        ///     absolute <see cref="index" /> until the destination becomes non-writable.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <see cref="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuf GetBytes(int index, IByteBuf destination);

        /// <summary>
        ///     Transfers this buffers data to the specified <see cref="destination" /> buffer starting at the specified
        ///     absolute <see cref="index" /> until the destination becomes non-writable.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <see cref="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuf GetBytes(int index, IByteBuf destination, int length);

        /// <summary>
        ///     Transfers this buffers data to the specified <see cref="destination" /> buffer starting at the specified
        ///     absolute <see cref="index" /> until the destination becomes non-writable.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <see cref="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuf GetBytes(int index, IByteBuf destination, int dstIndex, int length);

        /// <summary>
        ///     Transfers this buffers data to the specified <see cref="destination" /> buffer starting at the specified
        ///     absolute <see cref="index" /> until the destination becomes non-writable.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <see cref="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuf GetBytes(int index, byte[] destination);

        /// <summary>
        ///     Transfers this buffers data to the specified <see cref="destination" /> buffer starting at the specified
        ///     absolute <see cref="index" /> until the destination becomes non-writable.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <see cref="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuf GetBytes(int index, byte[] destination, int dstIndex, int length);

        /// <summary>
        ///     Sets the specified boolean at the specified absolute <see cref="index" /> in this buffer.
        ///     This method does not directly modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <see cref="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuf SetBoolean(int index, bool value);

        /// <summary>
        ///     Sets the specified byte at the specified absolute <see cref="index" /> in this buffer.
        ///     This method does not directly modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <see cref="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuf SetByte(int index, int value);

        /// <summary>
        ///     Sets the specified short at the specified absolute <see cref="index" /> in this buffer.
        ///     This method does not directly modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <see cref="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuf SetShort(int index, int value);

        /// <summary>
        ///     Sets the specified unsigned short at the specified absolute <see cref="index" /> in this buffer.
        ///     This method does not directly modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <see cref="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuf SetUnsignedShort(int index, int value);

        /// <summary>
        ///     Sets the specified integer at the specified absolute <see cref="index" /> in this buffer.
        ///     This method does not directly modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <see cref="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuf SetInt(int index, int value);

        /// <summary>
        ///     Sets the specified unsigned integer at the specified absolute <see cref="index" /> in this buffer.
        ///     This method does not directly modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <see cref="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuf SetUnsignedInt(int index, uint value);

        /// <summary>
        ///     Sets the specified long integer at the specified absolute <see cref="index" /> in this buffer.
        ///     This method does not directly modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <see cref="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuf SetLong(int index, long value);

        /// <summary>
        ///     Sets the specified UTF-16 char at the specified absolute <see cref="index" /> in this buffer.
        ///     This method does not directly modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <see cref="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuf SetChar(int index, char value);

        /// <summary>
        ///     Sets the specified double at the specified absolute <see cref="index" /> in this buffer.
        ///     This method does not directly modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <see cref="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuf SetDouble(int index, double value);

        /// <summary>
        ///     Transfers the <see cref="src" /> byte buffer's contents starting at the specified absolute <see cref="index" />.
        ///     This method does not directly modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <see cref="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuf SetBytes(int index, IByteBuf src);

        /// <summary>
        ///     Transfers the <see cref="src" /> byte buffer's contents starting at the specified absolute <see cref="index" />.
        ///     This method does not directly modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <see cref="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuf SetBytes(int index, IByteBuf src, int length);

        /// <summary>
        ///     Transfers the <see cref="src" /> byte buffer's contents starting at the specified absolute <see cref="index" />.
        ///     This method does not directly modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <see cref="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuf SetBytes(int index, IByteBuf src, int srcIndex, int length);

        /// <summary>
        ///     Transfers the <see cref="src" /> byte buffer's contents starting at the specified absolute <see cref="index" />.
        ///     This method does not directly modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <see cref="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuf SetBytes(int index, byte[] src);

        /// <summary>
        ///     Transfers the <see cref="src" /> byte buffer's contents starting at the specified absolute <see cref="index" />.
        ///     This method does not directly modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if the specified <see cref="index" /> is less than <c>0</c> or
        ///     <c>index + 1</c> greater than <see cref="Capacity" />
        /// </exception>
        IByteBuf SetBytes(int index, byte[] src, int srcIndex, int length);

        /// <summary>
        ///     Gets a boolean at the current <see cref="ReaderIndex" /> and increases the <see cref="ReaderIndex" />
        ///     by <c>1</c> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">if <see cref="ReadableBytes" /> is less than <c>1</c></exception>
        bool ReadBoolean();

        /// <summary>
        ///     Gets a byte at the current <see cref="ReaderIndex" /> and increases the <see cref="ReaderIndex" />
        ///     by <c>1</c> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">if <see cref="ReadableBytes" /> is less than <c>1</c></exception>
        byte ReadByte();


        /// <summary>
        ///     Gets a short at the current <see cref="ReaderIndex" /> and increases the <see cref="ReaderIndex" />
        ///     by <c>2</c> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">if <see cref="ReadableBytes" /> is less than <c>2</c></exception>
        short ReadShort();

        /// <summary>
        ///     Gets an unsigned short at the current <see cref="ReaderIndex" /> and increases the <see cref="ReaderIndex" />
        ///     by <c>2</c> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">if <see cref="ReadableBytes" /> is less than <c>2</c></exception>
        ushort ReadUnsignedShort();

        /// <summary>
        ///     Gets an integer at the current <see cref="ReaderIndex" /> and increases the <see cref="ReaderIndex" />
        ///     by <c>4</c> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">if <see cref="ReadableBytes" /> is less than <c>4</c></exception>
        int ReadInt();

        /// <summary>
        ///     Gets an unsigned integer at the current <see cref="ReaderIndex" /> and increases the <see cref="ReaderIndex" />
        ///     by <c>4</c> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">if <see cref="ReadableBytes" /> is less than <c>4</c></exception>
        uint ReadUnsignedInt();

        long ReadLong();

        /// <summary>
        ///     Gets a 2-byte UTF-16 character at the current <see cref="ReaderIndex" /> and increases the
        ///     <see cref="ReaderIndex" />
        ///     by <c>2</c> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">if <see cref="ReadableBytes" /> is less than <c>2</c></exception>
        char ReadChar();

        /// <summary>
        ///     Gets an 8-byte Decimaling integer at the current <see cref="ReaderIndex" /> and increases the
        ///     <see cref="ReaderIndex" />
        ///     by <c>8</c> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">if <see cref="ReadableBytes" /> is less than <c>8</c></exception>
        double ReadDouble();

        /// <summary>
        ///     Reads <see cref="length" /> bytes from this buffer into a new destination buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if <see cref="ReadableBytes" /> is less than <see cref="length" />
        /// </exception>
        IByteBuf ReadBytes(int length);

        /// <summary>
        ///     Transfers bytes from this buffer's data into the specified destination buffer
        ///     starting at the current <see cref="ReaderIndex" /> until the destination becomes
        ///     non-writable and increases the <see cref="ReaderIndex" /> by the number of transferred bytes.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        ///     if <see cref="destination.WritableBytes" /> is greater than
        ///     <see cref="ReadableBytes" />.
        /// </exception>
        IByteBuf ReadBytes(IByteBuf destination);

        IByteBuf ReadBytes(IByteBuf destination, int length);

        IByteBuf ReadBytes(IByteBuf destination, int dstIndex, int length);

        IByteBuf ReadBytes(byte[] destination);

        IByteBuf ReadBytes(byte[] destination, int dstIndex, int length);

        /// <summary>
        ///     Increases the current <see cref="ReaderIndex" /> by the specified <see cref="length" /> in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException"> if <see cref="length" /> is greater than <see cref="ReadableBytes" />.</exception>
        IByteBuf SkipBytes(int length);

        IByteBuf WriteBoolean(bool value);

        IByteBuf WriteByte(int value);

        IByteBuf WriteShort(int value);

        IByteBuf WriteUnsignedShort(int value);

        IByteBuf WriteInt(int value);

        IByteBuf WriteUnsignedInt(uint value);

        IByteBuf WriteLong(long value);

        IByteBuf WriteChar(char value);

        IByteBuf WriteDouble(double value);

        IByteBuf WriteBytes(IByteBuf src);

        IByteBuf WriteBytes(IByteBuf src, int length);

        IByteBuf WriteBytes(IByteBuf src, int srcIndex, int length);

        IByteBuf WriteBytes(byte[] src);

        IByteBuf WriteBytes(byte[] src, int srcIndex, int length);

        /// <summary>
        ///     Returns the maximum <see cref="ArraySegment{T}" /> of <see cref="Byte" /> that this buffer holds. Note that
        ///     <see cref="GetIoBuffers()" />
        ///     or <see cref="GetIoBuffers(int,int)" /> might return a less number of <see cref="ArraySegment{T}" />s of
        ///     <see cref="Byte" />.
        /// </summary>
        /// <returns>
        ///     <c>-1</c> if this buffer cannot represent its content as <see cref="ArraySegment{T}" /> of <see cref="Byte" />.
        ///     the number of the underlying {@link ByteBuffer}s if this buffer has at least one underlying segment.
        ///     Note that this method does not return <c>0</c> to avoid confusion.
        /// </returns>
        /// <seealso cref="GetIoBuffer()" />
        /// <seealso cref="GetIoBuffer(int,int)" />
        /// <seealso cref="GetIoBuffers()" />
        /// <seealso cref="GetIoBuffers(int,int)" />
        int IoBufferCount { get; }

        /// <summary>
        ///     Exposes this buffer's readable bytes as an <see cref="ArraySegment{T}" /> of <see cref="Byte" />. Returned segment
        ///     shares the content with this buffer. This method is identical
        ///     to <c>buf.GetIoBuffer(buf.ReaderIndex, buf.ReadableBytes)</c>. This method does not
        ///     modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer.  Please note that the
        ///     returned segment will not see the changes of this buffer if this buffer is a dynamic
        ///     buffer and it adjusted its capacity.
        /// </summary>
        /// <exception cref="NotSupportedException">
        ///     if this buffer cannot represent its content as <see cref="ArraySegment{T}" />
        ///     of <see cref="Byte" />
        /// </exception>
        /// <seealso cref="IoBufferCount" />
        /// <seealso cref="GetIoBuffers()" />
        /// <seealso cref="GetIoBuffers(int,int)" />
        ArraySegment<byte> GetIoBuffer();

        /// <summary>
        ///     Exposes this buffer's sub-region as an <see cref="ArraySegment{T}" /> of <see cref="Byte" />. Returned segment
        ///     shares the content with this buffer. This method does not
        ///     modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer. Please note that the
        ///     returned segment will not see the changes of this buffer if this buffer is a dynamic
        ///     buffer and it adjusted its capacity.
        /// </summary>
        /// <exception cref="NotSupportedException">
        ///     if this buffer cannot represent its content as <see cref="ArraySegment{T}" />
        ///     of <see cref="Byte" />
        /// </exception>
        /// <seealso cref="IoBufferCount" />
        /// <seealso cref="GetIoBuffers()" />
        /// <seealso cref="GetIoBuffers(int,int)" />
        ArraySegment<byte> GetIoBuffer(int index, int length);

        /// <summary>
        ///     Exposes this buffer's readable bytes as an array of <see cref="ArraySegment{T}" /> of <see cref="Byte" />. Returned
        ///     segments
        ///     share the content with this buffer. This method does not
        ///     modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer.  Please note that
        ///     returned segments will not see the changes of this buffer if this buffer is a dynamic
        ///     buffer and it adjusted its capacity.
        /// </summary>
        /// <exception cref="NotSupportedException">
        ///     if this buffer cannot represent its content with <see cref="ArraySegment{T}" />
        ///     of <see cref="Byte" />
        /// </exception>
        /// <seealso cref="IoBufferCount" />
        /// <seealso cref="GetIoBuffer()" />
        /// <seealso cref="GetIoBuffer(int,int)" />
        ArraySegment<byte>[] GetIoBuffers();

        /// <summary>
        ///     Exposes this buffer's bytes as an array of <see cref="ArraySegment{T}" /> of <see cref="Byte" /> for the specified
        ///     index and length.
        ///     Returned segments share the content with this buffer. This method does
        ///     not modify <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of this buffer. Please note that
        ///     returned segments will not see the changes of this buffer if this buffer is a dynamic
        ///     buffer and it adjusted its capacity.
        /// </summary>
        /// <exception cref="NotSupportedException">
        ///     if this buffer cannot represent its content with <see cref="ArraySegment{T}" />
        ///     of <see cref="Byte" />
        /// </exception>
        /// <seealso cref="IoBufferCount" />
        /// <seealso cref="GetIoBuffer()" />
        /// <seealso cref="GetIoBuffer(int,int)" />
        ArraySegment<byte>[] GetIoBuffers(int index, int length);

        IByteBuf WriteZero(int length);

        /// <summary>
        ///     Converts the readable contents of the buffer into an array.
        ///     Does not affect the <see cref="ReaderIndex" /> or <see cref="WriterIndex" /> of the <see cref="IByteBuf" />
        /// </summary>
        byte[] ToArray();

        /// <summary>
        ///     Create a full clone of the existing byte buffer.
        /// </summary>
        IByteBuf Copy();

        /// <summary>
        ///     Copy a full clone for the specified segment of the current byte buffer.
        /// </summary>
        /// <param name="index">The starting read position</param>
        /// <param name="length">The length of the buffer.</param>
        /// <returns>A deep clone of the buffer at the specified length.</returns>
        IByteBuf Copy(int index, int length);

        IByteBuf Slice();

        IByteBuf Slice(int index, int length);

        IByteBuf ReadSlice(int length);

        /// <summary>
        ///     Creates a view of the current byte buffer. If you want a deep copy call <see cref="Copy" /> instead.
        /// </summary>
        IByteBuf Duplicate();

        /// <summary>
        ///     Unwraps a nested buffer
        /// </summary>
        IByteBuf Unwrap();

        /// <summary>
        ///     Shifts all of the <see cref="ReadableBytes" /> to the front of the internal buffer
        ///     and resets the <see cref="ReaderIndex" /> to zero and <see cref="WriterIndex" /> to <see cref="ReadableBytes" />.
        ///     Designed to work with frequently re-used buffers that are held for long periods of time.
        /// </summary>
        [Obsolete]
        IByteBuf Compact();

        /// <summary>
        ///     Compacts if and only if the buffer determines that there aren't enough
        ///     <see cref="WritableBytes" /> for it to continue functioning as necessary.
        ///     Designed to work with frequently re-used buffers
        /// </summary>
        [Obsolete]
        IByteBuf CompactIfNecessary();

        string ToString(Encoding encoding);
    }
}