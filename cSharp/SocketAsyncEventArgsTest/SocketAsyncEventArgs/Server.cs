using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TestSocketAsyncEventArgs
{
    class AsyncUserToken
    {
        public Socket Socket { get; set; }
    }

    class Server
    {
        private int numConnections;
        private int receiveBufferSize;
        BufferManager bufferManager;
        const int opsToPreAlloc = 2;
        Socket listenSocket;

        SocketAsyncEventArgsPool readWritePool;
        int totalBytesRead;
        int numConnectedSockets;
        Semaphore maxNumberAcceptedClients;

        public Server(int numConnections, int receiveBufferSize)
        {
            this.totalBytesRead = 0;
            this.numConnectedSockets = 0;
            this.numConnections = numConnections;
            this.receiveBufferSize = receiveBufferSize;
            this.bufferManager = new BufferManager(receiveBufferSize * numConnections * opsToPreAlloc,
                receiveBufferSize);

            this.readWritePool = new SocketAsyncEventArgsPool(numConnections);
            this.maxNumberAcceptedClients = new Semaphore(numConnections, numConnections);
        }

        public void Init()
        {
            this.bufferManager.InitBuffer();
            SocketAsyncEventArgs readWriteEventArgs;
            for(int i = 0; i < this.numConnections; i++)
            {
                readWriteEventArgs = new SocketAsyncEventArgs();
                readWriteEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                readWriteEventArgs.UserToken = new AsyncUserToken();

                this.bufferManager.SetBuffer(readWriteEventArgs);

                this.readWritePool.Push(readWriteEventArgs);
            }
        }

        public void Start(IPEndPoint localEndPoint)
        {
            this.listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.listenSocket.Bind(localEndPoint);
            listenSocket.Listen(100);

            StartAccept(null);

            Console.WriteLine("Press any key to terminate the server process....");
            Console.ReadKey();
        }

        private void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if(acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
            }
            else
            {
                acceptEventArg.AcceptSocket = null;
            }

            this.maxNumberAcceptedClients.WaitOne();
            bool willRaiseEvent = this.listenSocket.AcceptAsync(acceptEventArg);
            if (!willRaiseEvent)
            {
                ProcessAccept(acceptEventArg);
            }
        }

        private void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            Interlocked.Increment(ref this.numConnectedSockets);
            Console.WriteLine("Client connection accepted. There are {0} clients connected to the server", this.numConnectedSockets);

            SocketAsyncEventArgs readEventArgs = this.readWritePool.Pop();
            ((AsyncUserToken)readEventArgs.UserToken).Socket = e.AcceptSocket;

            bool willRaiseEvent = e.AcceptSocket.ReceiveAsync(readEventArgs);
            if (!willRaiseEvent)
            {
                ProcessReceive(readEventArgs);
            }

            StartAccept(e);
        }

        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            AsyncUserToken token = (AsyncUserToken)e.UserToken;
            if(e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                Interlocked.Add(ref this.totalBytesRead, e.BytesTransferred);
                Console.WriteLine("The server has read a total of {0} bytes",this.totalBytesRead);

                e.SetBuffer(e.Offset, e.BytesTransferred);
                bool willRaiseEvent = token.Socket.SendAsync(e);
                if (!willRaiseEvent)
                {
                    ProcessSend(e);
                }
            }
            else
            {
                CloseClientSocket(e);
            }
        }

        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if(e.SocketError == SocketError.Success)
            {
                AsyncUserToken token = (AsyncUserToken)e.UserToken;
                bool willRaiseEvent = token.Socket.ReceiveAsync(e);
                if (!willRaiseEvent)
                {
                    ProcessReceive(e);
                }
            }
            else
            {
                CloseClientSocket(e);
            }
        }

        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            AsyncUserToken token = e.UserToken as AsyncUserToken;
            try
            {
                token.Socket.Shutdown(SocketShutdown.Send);
            }
            catch (Exception) { }
            token.Socket.Close();

            Interlocked.Decrement(ref this.numConnectedSockets);

            this.readWritePool.Push(e);

            this.maxNumberAcceptedClients.Release();
            Console.WriteLine("A client has been disconnected from the server. There are {0} clients connected to the server", this.numConnectedSockets);
        }
    }
}
