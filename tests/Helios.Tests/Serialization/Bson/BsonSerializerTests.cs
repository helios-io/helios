using System.IO;
using System.Linq;
using System.Text;
using Helios.Serialization;
using Helios.Serialization.Bson;
using NUnit.Framework;

namespace Helios.Tests.Serialization.Bson
{
    /// <summary>
    /// Tests for ensuring that our BSON serializer works properly
    /// </summary>
    [TestFixture]
    public class BsonSerializerTests
    {
        #region Setup / Teardown

        // input from example at http://bsonspec.org/#/specification
/*
        string expectedJson =
        @"{
                ""hello"" : ""world""
        }";
*/

        byte[] expectedBinary = Encoding.UTF8.GetBytes(
                                "\x16\x00\x00\x00\x02hello\x00" +
                                "\x06\x00\x00\x00world\x00\x00");

        HelloWorld expectedObj = new HelloWorld() {hello = "world"};

        public class HelloWorld
        {
            public string hello { get; set; }
        }

        #endregion

        #region Tests

        [Test]
        public void Should_serialize_obj_to_Bson()
        {
            //arrange
            ITransportSerializer bsonSerializer = new BsonTransportSerializer();
            var stream = new MemoryStream();

            //act
            bsonSerializer.Serialize(expectedObj, stream);
            var bytes = stream.ToArray();

            //assert
            Assert.IsTrue(bytes.SequenceEqual(expectedBinary));
        }

        [Test]
        public void Should_serialize_Bson_to_Obj()
        {
            //arrange
            ITransportSerializer bsonSerializer = new BsonTransportSerializer();
            var stream = new MemoryStream(expectedBinary, 0, expectedBinary.Length);

            //act
            var helloObj = bsonSerializer.Deserialize<HelloWorld>(stream);

            //assert
            Assert.AreEqual(helloObj.hello, expectedObj.hello);
        }

        #endregion
    }
}
