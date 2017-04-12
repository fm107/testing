using System.Collections.Concurrent;

namespace Torrent.Client
{
    public class Cache<T> where T : class, ICacheable, new()
    {
        private readonly ConcurrentQueue<T> _queue;

        public Cache()
        {
            _queue = new ConcurrentQueue<T>();
        }

        public void Put(T item)
        {
            _queue.Enqueue(item);
        }

        public T Get()
        {
            T item;
            if (!_queue.TryDequeue(out item))
                item = new T();
            return (T) item.Init();
        }
    }
}