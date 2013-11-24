using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helios.Core.Util.Collections;
using NUnit.Framework;

namespace Helios.Tests.Core.Util.Collections
{
    [TestFixture]
    public class ConcurrentFixedSizeStackTests
    {
        #region Setup / Teardown
        #endregion

        #region Tests

        /// <summary>
        /// Should get a default value, rather than an exception, if we peek or pop when the stack is empty
        /// </summary>
        [Test]
        public void Should_get_default_value_on_Stack_Peek_or_Pop_when_Empty()
        {
            //arrange
            var stack = new ConcurrentFixedSizeStack<int>(5);

            //act

            //assert
            Assert.AreEqual(default(int), stack.Pop());
            Assert.AreEqual(default(int), stack.Peek());
        }

        /// <summary>
        /// If we call Stack.ToArray() when the stack has no items, we should get a non-null array of size 0
        /// </summary>
        [Test]
        public void Should_get_array_with_no_items_on_ToArray_when_Empty()
        {
            //arrange
            var stack = new ConcurrentFixedSizeStack<int>(5);

            //act
            var array = stack.ToArray();

            //assert
            Assert.IsNotNull(array);
            Assert.AreEqual(0, array.Length);
        }

        /// <summary>
        /// Should be able to fill a concurrent stack to capacity with no problems
        /// </summary>
        [Test]
        public void Should_add_items_to_Stack_up_to_Capacity()
        {
            //arrange
            var items = new[] { 1, 2, 3, 4, 5 };
            var stack = new ConcurrentFixedSizeStack<int>(5);

            //act
            Assert.AreEqual(0, stack.Count);
            Assert.AreEqual(items.Length, stack.Capacity);

            foreach (var item in items)
            {
                stack.Push(item);
                Assert.AreEqual(item, stack.Peek());
            }

            //assert
            Assert.AreEqual(stack.Capacity, stack.Count, "Stack should be full to capacity");

            var length = stack.Capacity - 1;
            for (var i = 0; i < stack.Capacity; i++, length--)
            {
                var stackItem = stack.Pop();
                Assert.AreEqual(items[length], stackItem);
            }
        }

        /// <summary>
        /// When a stack has been filled to capacity, we should get an array of all items back
        /// </summary>
        [Test]
        public void Should_get_array_of_all_items_when_Stack_is_at_Capacity()
        {
            //arrange
            var items = new[] { 1, 2, 3, 4, 5 };
            var stack = new ConcurrentFixedSizeStack<int>(5);

            //act
            Assert.AreEqual(0, stack.Count);
            Assert.AreEqual(items.Length, stack.Capacity);

            foreach (var item in items)
            {
                stack.Push(item);
                Assert.AreEqual(item, stack.Peek());
            }

            var array = stack.ToArray();

            //assert
            Assert.AreEqual(stack.Capacity, stack.Count, "Stack should STILL be full to capacity");
            Assert.AreEqual(array.Length, stack.Count, "Resultant array and original Stack should be of same size");

            var arrCount = items.Length - 1;
            foreach (var i in array)
            {
                Assert.AreEqual(items[arrCount], i);
                arrCount--;
            }
        }

        /// <summary>
        /// Should be able to fill a concurrent beyond capacity with no problems
        /// </summary>
        [Test]
        public void Should_add_items_to_Stack_is_over_Capacity()
        {
            //arrange
            var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var stack = new ConcurrentFixedSizeStack<int>(5);

            //act
            Assert.AreEqual(0, stack.Count);

            foreach (var item in items)
            {
                stack.Push(item);
                Assert.AreEqual(item, stack.Peek());
            }

            //assert
            Assert.AreEqual(stack.Capacity, stack.Count, "Stack should be full to capacity");

            var length = items.Length - 1;
            for (var i = 0; i < stack.Capacity; i++, length--)
            {
                var stackItem = stack.Pop();
                Assert.AreEqual(items[length], stackItem);
            }
            Assert.IsTrue(length == items.Length / 2 - 1);
        }

        /// <summary>
        /// When a stack has been filled over capacity, we should get an array of the most recent items back
        /// </summary>
        [Test]
        public void Should_get_array_of_all_items_when_Stack_is_over_Capacity()
        {
            //arrange
            var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var stack = new ConcurrentFixedSizeStack<int>(5);

            //act
            Assert.AreEqual(0, stack.Count);

            foreach (var item in items)
            {
                stack.Push(item);
                Assert.AreEqual(item, stack.Peek());
            }

            var array = stack.ToArray();

            //assert
            Assert.AreEqual(stack.Capacity, stack.Count, "Stack should STILL be full to capacity");
            Assert.AreEqual(array.Length, stack.Count, "Resultant array and original Stack should be of same size");

            var arrCount = items.Length - 1;
            foreach (var i in array)
            {
                Assert.AreEqual(items[arrCount], i);
                arrCount--;
            }
            Assert.IsTrue(arrCount == items.Length / 2 - 1);
        }

        #endregion
    }
}
