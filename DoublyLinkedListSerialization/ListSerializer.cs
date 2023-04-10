using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DoubleLinkedListSerialization
{
    class ListNodePrototype
    {
        public const int MINIMUM_NODE_SIZE = 0x10;
        public uint Previous;
        public uint Next;
        public uint Random;
        public string Data;
    }

    public class ListSerializer
    {
        private const string HEADER_MAGIC = "DOUBLELINKEDLIST";
        private const int HEADER_MAGIC_SIZE = 0x10;
        private const int HEADER_ELEMENTS_COUNT_SIZE = 0x4;
        private static readonly byte[] HEADER_MAGIC_BYTES = new byte[] { 0x44, 0x4f, 0x55, 0x42, 0x4c, 0x45, 0x4c, 0x49, 0x4e, 0x4b, 0x45, 0x44, 0x4c, 0x49, 0x53, 0x54 };


        private ListRand list;
        private readonly Dictionary<ListNode, uint> listNodeToIndexDict;

        public ListSerializer(ListRand list)
        {
            this.list = list;
            this.listNodeToIndexDict = GetListNodesToIndexDictionary(list);
        }

        private static Dictionary<ListNode, uint> GetListNodesToIndexDictionary(ListRand list)
        {
            Dictionary<ListNode, uint> result = new Dictionary<ListNode, uint>();

            ListNode current = list.Head;
            //0 if null element
            uint currentIndex = 1; 

            while (current != null && !result.ContainsKey(current))
            {
                result.Add(current, currentIndex++);
                current = current.Next;
            }

            return result;
        }

        private static void Write(Stream s, byte[] data)
        {
            s.Write(data, 0, data.Length);
        }

        private void WriteListHeader(Stream s)
        {
            Write(s, HEADER_MAGIC_BYTES);
            Write(s, BitConverter.GetBytes(list.count));
        }

        private uint GetIndexForElement(ListNode node)
        {
            if (node == null)
            {
                return 0;
            }

            return listNodeToIndexDict[node];
        }

        private byte[] GetNodeBytes(ListNode node)
        {
            return BitConverter.GetBytes(GetIndexForElement(node));
        }

        private void Serialize(ListNode node, Stream s)
        {
            Write(s, GetNodeBytes(node.Previous));
            Write(s, GetNodeBytes(node.Next));
            Write(s, GetNodeBytes(node.Random));
            Write(s, BitConverter.GetBytes(node.Data.Length));
            Write(s, Encoding.UTF8.GetBytes(node.Data));
        }

        public void Serialize(Stream s)
        {
            WriteListHeader(s);

            HashSet<ListNode> visited = new HashSet<ListNode>();
            ListNode current = list.Head;

            while (current != null)
            {
                if (visited.Contains(current))
                {
                    break;
                }

                visited.Add(current);
                Serialize(current, s);
                current = current.Next;
            }
        }

        private bool IsListHeaderMagicAvailable(Stream s)
        {
            byte[] header = new byte[HEADER_MAGIC_SIZE];
            int dataRead = s.Read(header, 0, HEADER_MAGIC_SIZE);

            if (dataRead != HEADER_MAGIC_SIZE)
            {
                Console.WriteLine("The number of available bytes in the stream is less than the header magic size");
                return false;
            }

            string headerString = Encoding.ASCII.GetString(header);

            if (!Equals(HEADER_MAGIC, headerString))
            {
                Console.WriteLine("Header magic not found");
                return false;
            }

            return true;
        }

        private bool IsListHeaderAvailable(Stream s, out int elementsCount)
        {
            if (!IsListHeaderMagicAvailable(s))
            {
                elementsCount = 0;
                return false;
            }

            byte[] elementsCountBytes = new byte[HEADER_ELEMENTS_COUNT_SIZE];
            int dataRead = s.Read(elementsCountBytes, 0, HEADER_ELEMENTS_COUNT_SIZE);

            if (dataRead != HEADER_ELEMENTS_COUNT_SIZE)
            {
                Console.WriteLine("The number of available bytes in the stream is less than the header elements count size");
                elementsCount = 0;
                return false;
            }

            elementsCount = BitConverter.ToInt32(elementsCountBytes, 0);
            return true;
        }

        private bool DeserializaNodeData(Stream s, int dataLen, out string data)
        {
            byte[] dataBytes = new byte[dataLen];
            int bytesRead = s.Read(dataBytes, 0, dataLen);

            if (bytesRead != dataLen)
            {
                Console.WriteLine("Failed to read node data");
                data = "";
                return false;
            }

            data = Encoding.UTF8.GetString(dataBytes);
            return true;
        }

        private ListNodePrototype DeserializeNode(Stream s)
        {
            byte[] nodeData = new byte[ListNodePrototype.MINIMUM_NODE_SIZE];
            int bytesRead = s.Read(nodeData, 0, ListNodePrototype.MINIMUM_NODE_SIZE);

            if (bytesRead != ListNodePrototype.MINIMUM_NODE_SIZE)
            {
                Console.WriteLine("Failed to read node data");
                return null;
            }

            int offset = 0;
            uint prev = BitConverter.ToUInt32(nodeData, offset);
            offset += 0x4;

            uint next = BitConverter.ToUInt32(nodeData, offset);
            offset += 0x4;

            uint random = BitConverter.ToUInt32(nodeData, offset);
            offset += 0x4;

            int dataLen = BitConverter.ToInt32(nodeData, offset);

            if (!DeserializaNodeData(s, dataLen, out string data))
            {
                return null;
            }

            return new ListNodePrototype { Previous = prev, Next = next, Random = random, Data = data};
        }

        private ListRand ConnectNodes(List<ListNodePrototype> prototypes, Dictionary<uint, ListNode> nodes)
        {
            uint index = 1;
            foreach (ListNodePrototype prototype in prototypes)
            {
                ListNode current = nodes[index++];

                if (prototype.Previous != 0)
                {
                    current.Previous = nodes[prototype.Previous];
                }
                if (prototype.Next != 0)
                {
                    current.Next = nodes[prototype.Next];
                }
                if (prototype.Random != 0)
                {
                    current.Random = nodes[prototype.Random];
                }
            }

            ListNode head = null;
            ListNode tail = null;
            
            if (nodes.Count != 0)
            {
                head = nodes[1];
                tail = nodes[index - 1];
            }

            ListRand newList = new ListRand
            {
                Head = head,
                Tail = tail,
                count = nodes.Count
            };

            return newList;
        }

        public void Deserialize(Stream s)
        {
            if (!IsListHeaderAvailable(s, out int elementCount))
            {
                return;
            }

            Dictionary<uint, ListNode> nodes = new Dictionary<uint, ListNode>();
            List<ListNodePrototype> prototypes = new List<ListNodePrototype>();

            for (uint index = 1; index <= elementCount; ++index)
            {
                ListNodePrototype nodePrototype = DeserializeNode(s);

                ListNode newNode = new ListNode { Data = nodePrototype.Data };

                prototypes.Add(nodePrototype);
                nodes.Add(index, newNode);
            }

            list = ConnectNodes(prototypes, nodes);
        }
    }
}
