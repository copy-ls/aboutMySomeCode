思路：
	1. 初始化一个超大的buffer管理器，防止buffer内存碎片话和复用buffer
	2. 
		2.1	单个SocketAsyncEventArgs重复接收客户端Socket数据
		2.2 初始化足够多的SocketAsyncEventArgs来处理Socket的Completed回调
	3. 接收时Semaphore会增加，处理完成才会释放(不太明白有是什么用)
	4. StartAccept接收下一个Socket？
笔记：
	1. 同步Socket链接时会挂起程序等待(链接,发送,接收)
		1) 对应Connect,Send,Receive等方法
	2. 异步socket不会挂起程序，会新开线程执行(链接,发送,接收)
		1) System.Threading.ManualResetEvent 对应Begin* End*(Connect,Send,Receive)等方法
		2) System.Net.Sockets.SocketAsyncEventArgs 对应(Connect,Send,Receive)*Async等方法
注意:
	1. 接收时需要单次或多次接收,且判断一段数据是否接收完成
	   因为每次从网络设备读取时数据可能包含单段或多段的完整或不完整数据
	2. example:
	
		byte[] msg = System.Text.Encoding.ASCII.GetBytes("This is a test");  
		int bytesSent = s.Send(msg); 
		
	Send 方法从缓冲区移除字节，并用网络接口将这些字节排队以便发送到网络设备。 网络接口可能不会立即发送数据，但它最终将发送，只要使用 Shutdown 方法正常关闭连接。
	The Send method removes the bytes from the buffer and queues them with the network interface to be sent to the network device. The network interface might not send the data immediately, but it will send it eventually, as long as the connection is closed normally with the Shutdown method.
		C#开源发送数据部分代码
		
		fixed (byte* pinnedBuffer = buffer) {
                        bytesTransferred = UnsafeNclNativeMethods.OSSOCK.send(
                                        m_Handle.DangerousGetHandle(),
                                        pinnedBuffer+offset,
                                        size,
                                        socketFlags);
                    }
		
		//UnsafeNclNativeMethods.OSSOCK.send
		// This method is always blocking, so it uses an IntPtr.
            [DllImport(WS2_32, SetLastError = true)]
            internal unsafe static extern int recv(
                                         [In] IntPtr      socketHandle,
                                         [In] byte*       pinnedBuffer,
                                         [In] int         len,
                                         [In] SocketFlags socketFlags
                                         );