using System.Collections.Generic;
using System.Net.Sockets;

namespace TestSocketAsyncEventArgs
{
    class BufferManager
    {
        int numBytes;
        byte[] buffer;
        Stack<int> freeIndexPool;
        int currentIndex;
        int bufferSize;

        public BufferManager(int totoalBytes, int bufferSize)
        {
            this.numBytes = totoalBytes;
            this.bufferSize = bufferSize;
            this.currentIndex = 0;
            this.freeIndexPool = new Stack<int>();
        }

        public void InitBuffer()
        {
            this.buffer = new byte[this.numBytes];
        }

        public bool SetBuffer(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            if(this.freeIndexPool.Count > 0)
            {
                socketAsyncEventArgs.SetBuffer(this.buffer, this.freeIndexPool.Pop(), this.bufferSize);
            }
            else
            {
                if(this.numBytes - this.bufferSize < currentIndex)
                {
                    return false;
                }
                socketAsyncEventArgs.SetBuffer(this.buffer, this.currentIndex, this.bufferSize);
                this.currentIndex += bufferSize;
            }
            return true;
        }

        public void FreeBuffer(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            this.freeIndexPool.Push(socketAsyncEventArgs.Offset);
            socketAsyncEventArgs.SetBuffer(null, 0, 0);
        }
    }
}
