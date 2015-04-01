using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Helios.Util.Collections
{
    public interface ICircularBuffer<T> : IProducerConsumerCollection<T>
    {
        /// <summary>
        /// The absolute maximum size of the circular buffer
        /// </summary>
        int MaxCapacity { get; }

        /// <summary>
        /// The gets or sets the capacity of the buffer
        /// </summary>
        int Capacity { get; set; }

        /// <summary>
        /// The current size of the buffer.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Read position of the current buffer
        /// </summary>
        int Head { get; }

        /// <summary>
        /// Write position of the current buffer
        /// </summary>
        int Tail { get; }

        void SetHead(int position);

        void SetTail(int position);

        void IncrementHead(int increment);

        void IncrementTail(int increment);

        /// <summary>
        /// Peeks at the next message in the buffer
        /// </summary>
        /// <returns>The object at the start position of the buffer</returns>
        T Peek();

        /// <summary>
        /// Adds an object to the end of the circular buffer
        /// </summary>
        /// <param name="obj">An object of type T</param>
        void Enqueue(T obj);

        /// <summary>
        /// Adds an array of objects to the end of the circular buffer
        /// </summary>
        /// <param name="objs">An array of objects of type T</param>
        void Enqueue(T[] objs);

        /// <summary>
        /// Dequeues an object from the start of the circular buffer
        /// </summary>
        /// <returns>An object of type T</returns>
        T Dequeue();

        /// <summary>
        /// Skip N number of items in the queue
        /// </summary>
        void Skip(int length);

        /// <summary>
        /// Dequeues multiple items at once, if available
        /// </summary>
        /// <param name="count">The maximum number of items to dequeue</param>
        /// <returns>An enumerable list of items</returns>
        IEnumerable<T> Dequeue(int count);

        /// <summary>
        /// Dequeues the entire buffer in one dump
        /// </summary>
        /// <returns>All of the active contents of a circular buffer</returns>
        IEnumerable<T> DequeueAll();

        /// <summary>
        /// Checks an index relative to the <see cref="Head"/> to see if there's a set element there
        /// </summary>
        bool IsElementAt(int index);

        T ElementAt(int index);

        /// <summary>
        /// Sets an element at the specified position relative to <see cref="Head"/>
        /// WITHOUT MODIFYING <see cref="Tail"/>
        /// </summary>
        /// <param name="element">The element we want to add at the specified <see cref="index"/></param>
        /// <param name="index">The index relative to the front of the buffer where we want to add <see cref="element"/></param>
        void SetElementAt(T element, int index);

        /// <summary>
        /// Indexing operator - doesn't explicitly move the head or tail of the underlying buffer.
        /// </summary>
        T this[int index] { get; set; }

        /// <summary>
        /// Clears the contents from the buffer
        /// </summary>
        void Clear();

        /// <summary>
        /// Copies the contents of the Circular Buffer into a new array
        /// </summary>
        /// <param name="array">The destination array for the copy</param>
        void CopyTo(T[] array);

        /// <summary>
        /// Copies the contents of the Circular Buffer into a new array
        /// </summary>
        /// <param name="array">The destination array for the copy</param>
        /// <param name="index">The starting index for copying in the destination array</param>
        /// <param name="count">The number of items to copy from the current buffer (max value = current Size of buffer)</param>
        void CopyTo(T[] array, int index, int count);

        /// <summary>
        /// Copies the elements of <see cref="src"/> directly into the internal buffer
        /// of this ICircularBuffer and automatically adjusts the queue length accordingly.
        /// 
        /// Designed for performance sensitive operations.
        /// </summary>
        void DirectBufferWrite(T[] src);

        /// <summary>
        /// Copies the elements of <see cref="src"/> directly into the internal buffer
        /// of this ICircularBuffer and automatically adjusts the queue length accordingly.
        /// 
        /// Designed for performance sensitive operations.
        /// </summary>
        void DirectBufferWrite(T[] src, int srcLength);

        /// <summary>
        /// Copies the elements of <see cref="src"/> directly into the internal buffer
        /// of this ICircularBuffer and automatically adjusts the queue length accordingly.
        /// 
        /// Designed for performance sensitive operations.
        /// </summary>
        void DirectBufferWrite(T[] src, int srcIndex, int srcLength);

        /// <summary>
        /// Copies the elements of <see cref="src"/> directly into the internal buffer
        /// at the specified <see cref="index"/> of this ICircularBuffer.
        /// 
        /// Does NOT automatically adjust queue length.
        /// 
        /// Designed for performance sensitive operations.
        /// </summary>
        void DirectBufferWrite(int index, T[] src, int srcIndex, int srcLength);

        /// <summary>
        /// Copies elements directly from the CircularBuffer's internal buffer into <see cref="dest"/>
        /// and automatically adjusts the queue read position accordingly.
        /// 
        /// Designed for performance-sensitive operations.
        /// </summary>
        void DirectBufferRead(T[] dest);

        /// <summary>
        /// Copies elements directly from the CircularBuffer's internal buffer into <see cref="dest"/>
        /// and automatically adjusts the queue read position accordingly.
        /// 
        /// Designed for performance-sensitive operations.
        /// </summary>
        void DirectBufferRead(T[] dest, int destLength);

        /// <summary>
        /// Copies elements directly from the CircularBuffer's internal buffer into <see cref="dest"/>
        /// and automatically adjusts the queue read position accordingly.
        /// 
        /// Designed for performance-sensitive operations.
        /// </summary>
        void DirectBufferRead(T[] dest, int destIndex, int destLength);

        /// <summary>
        /// Copies elements directly from the CircularBuffer's internal buffer into <see cref="dest"/>
        /// and beginning from position <see cref="index"/>. Does NOT automatically adjust the queue length.
        /// 
        /// Designed for performance-sensitive operations.
        /// </summary>
        void DirectBufferRead(int index, T[] dest, int destIndex, int destLength);

        /// <summary>
        /// Set a range of values starting from the specified index
        /// </summary>
        void SetRange(int index, T[] values);
    }
}
