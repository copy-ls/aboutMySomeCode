
Stream.Read(buffer, offset, count)
stream的Position开始 ==> 从buffer的offset到(offset+count)

Stream.Write(buffer, offset, count)
stream的Position开始 <== 从buffer的offset到(offset+count)

ps：读取和写入成功,Position会改变。
	如果同时读取和写入，请使用Stream.Seek方法处理好Position关系
	
	Stream.Seek会改变Position
	Stream.SetLength会改变Length