using System.Collections.Generic;

namespace LaserPewer.Grbl
{
    public class GrblRequestQueue
    {
        public int CharacterCount { get; private set; }

        public bool IsEmpty { get { return backingQueue.Count == 0; } }

        private readonly Queue<GrblRequest> backingQueue;

        public GrblRequestQueue()
        {
            CharacterCount = 0;
            backingQueue = new Queue<GrblRequest>();
        }

        public void Enqueue(GrblRequest request)
        {
            backingQueue.Enqueue(request);
            CharacterCount += request.Message.Length;
        }

        public GrblRequest Peek()
        {
            return backingQueue.Peek();
        }

        public GrblRequest Dequeue()
        {
            GrblRequest request = backingQueue.Dequeue();
            CharacterCount -= request.Message.Length;
            return request;
        }
    }
}
