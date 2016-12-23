// CS0233.cs  
using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct S
{
	public int a;
}

public class MyClass
{
	public static void Main()
	{
		S myS = new S();
		Console.WriteLine(sizeof(S));   // CS0233  
										// Try the following line instead:  
										// Console.WriteLine(Marshal.SizeOf(myS));  
	}
}