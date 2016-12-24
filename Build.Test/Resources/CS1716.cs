// CS1716.cs  
// compile with: /unsafe  
using System;
using System.Runtime.CompilerServices;

public struct UnsafeStruct
{
	[FixedBuffer(typeof(int), 4)]  // CS1716  
	unsafe public int aField;
	// Use this single line instead of the above two lines.  
	// unsafe public fixed int aField[4];  
}

public class TestUnsafe
{
	static int Main()
	{
		UnsafeStruct us = new UnsafeStruct();
		unsafe
		{
			if (us.aField[0] == 0)
				return us.aField[1];
			else
				return us.aField[2];
		}
	}
}