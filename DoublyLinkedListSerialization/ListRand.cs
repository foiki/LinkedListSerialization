using System.IO;

namespace DoubleLinkedListSerialization
{ 
    public class ListRand
    {
        public ListNode Head;
        public ListNode Tail;
        public int count = 0;

        public void Serialize(Stream s)
        {
            ListSerializer serializer = new ListSerializer(this);
            serializer.Serialize(s);
        }

        public void Deserialize(Stream s) 
        {
            ListSerializer serializer = new ListSerializer(this);
            serializer.Deserialize(s);
        }
    }
}
