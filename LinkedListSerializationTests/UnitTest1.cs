using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using DoubleLinkedListSerialization;
using System.IO.Pipes;

namespace LinkedListSerializationTests
{
    [TestClass]
    public class SerializationTests
    {
        private static void AssertStreamDataEquals(Stream stream, string data)
        {
            byte[] streamBytes = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(streamBytes, 0, (int)stream.Length);
            string base64String = Convert.ToBase64String(streamBytes);
            Assert.AreEqual(data, base64String);
        }

        [TestMethod]
        public void EmptyList()
        {
            Stream stream = new FileStream("test1.txt", FileMode.OpenOrCreate);
            ListRand list = new ListRand();
            list.Serialize(stream);
            AssertStreamDataEquals(stream, "RE9VQkxFTElOS0VETElTVAAAAAA=");
        }

        [TestMethod]
        public void ListWithCycle()
        {
            Stream stream = new FileStream("test2.txt", FileMode.OpenOrCreate);
            ListNode first = new ListNode { Data = "First" };
            ListNode second = new ListNode { Data = "Second", Previous = first, Random = first, Next = first };
            first.Next = second;
            first.Random = second;
            first.Previous = second;
            ListRand list = new ListRand { Head = first, Tail = second, count = 2 };
            list.Serialize(stream);
            AssertStreamDataEquals(stream, "RE9VQkxFTElOS0VETElTVAIAAAACAAAAAgAAAAIAAAAFAAAARmlyc3QBAAAAAQAAAAEAAAAGAAAAU2Vjb25k");
        }

        [TestMethod]
        public void ListWithFourNodes()
        {
            Stream stream = new FileStream("test3.txt", FileMode.OpenOrCreate);
            ListNode first = new ListNode { Data = "First" };
            ListNode second = new ListNode { Data = "Second", Previous = first, Random = first };
            ListNode third = new ListNode { Data = "Third", Previous = second, Random = first };
            ListNode fourth = new ListNode { Data = "Fourth", Previous = third, Random = second, Next = first };
            first.Next = second;
            first.Random = second;
            first.Previous = fourth;
            second.Next = third;
            third.Next = fourth;
            ListRand list = new ListRand { Head = first, Tail = fourth, count = 4 };
            list.Serialize(stream);
            AssertStreamDataEquals(stream, "RE9VQkxFTElOS0VETElTVAQAAAAEAAAAAgAAAAIAAAAFAAAARmlyc3QBAAAAAwAAAAEAAAAGAAAAU2Vjb25kAgAAAAQAAAABAAAABQAAAFRoaXJkAwAAAAEAAAACAAAABgAAAEZvdXJ0aGQ=");
        }
    }

    [TestClass]
    public class DeserializationTests
    {
        [TestMethod]
        public void EmptyListDeserialization()
        {
            Stream stream = new FileStream("test21.txt", FileMode.OpenOrCreate);
            ListRand list = new ListRand();
            list.Serialize(stream);
            stream.Seek(0, SeekOrigin.Begin);
            list.Deserialize(stream);
            Assert.IsNull(list.Head);
            Assert.IsNull(list.Tail);
            Assert.AreEqual(list.count, 0);
        }

        [TestMethod]
        public void ListWithCycleDeserialization()
        {
            Stream stream = new FileStream("test22.txt", FileMode.OpenOrCreate);
            ListNode first = new ListNode { Data = "First" };
            ListNode second = new ListNode { Data = "Second", Previous = first, Random = first, Next = first };
            first.Next = second;
            first.Random = second;
            first.Previous = second;
            ListRand list = new ListRand { Head = first, Tail = second, count = 2 };
            list.Serialize(stream);
            list.Deserialize(stream);
            Assert.AreEqual(list.count, 2);
            Assert.AreEqual(list.Head.Data, "First");
            Assert.AreEqual(list.Tail.Data, "Second");
            Assert.AreEqual(list.Head.Next.Data, "Second");
            Assert.AreEqual(list.Tail.Random.Data, "First");
        }

        [TestMethod]
        public void ListWithFourNodesDeserialization()
        {
            Stream stream = new FileStream("test23.txt", FileMode.OpenOrCreate);
            ListNode first = new ListNode { Data = "First" };
            ListNode second = new ListNode { Data = "Second", Previous = first, Random = first };
            ListNode third = new ListNode { Data = "Third", Previous = second, Random = first };
            ListNode fourth = new ListNode { Data = "Fourth", Previous = third, Random = second, Next = first };
            first.Next = second;
            first.Random = second;
            first.Previous = fourth;
            second.Next = third;
            third.Next = fourth;
            ListRand list = new ListRand { Head = first, Tail = fourth, count = 4 };
            list.Serialize(stream);
            list.Deserialize(stream);
            Assert.AreEqual(list.count, 4);
            Assert.AreEqual(list.Head.Data, "First");
            Assert.AreEqual(list.Tail.Data, "Fourth");
            Assert.AreEqual(list.Head.Next.Data, "Second");
            Assert.AreEqual(list.Tail.Random.Data, "Second");
            Assert.AreEqual(list.Tail.Previous.Data, "Third");
        }
    }

}
