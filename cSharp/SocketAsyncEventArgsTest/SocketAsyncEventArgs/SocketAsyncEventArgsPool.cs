using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace TestSocketAsyncEventArgs
{
    class SocketAsyncEventArgsPool
    {
        Stack<SocketAsyncEventArgs> pool;

        public SocketAsyncEventArgsPool(int capacity)
        {
            this.pool = new Stack<SocketAsyncEventArgs>(capacity);
        }

        public void Push(SocketAsyncEventArgs item)
        {
            if(item == null)
            {
                throw new ArgumentNullException("Item add to a ScoketAsyncEventArgsPool cannot be null");
            }
            lock (this.pool)
            {
                this.pool.Push(item);
            }
        }

        public SocketAsyncEventArgs Pop()
        {
            lock (this.pool)
            {
                return this.pool.Pop();
            }
        }

        public int Count
        {
            get { return this.pool.Count; }
        }
    }
}
