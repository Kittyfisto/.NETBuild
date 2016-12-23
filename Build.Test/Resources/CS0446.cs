// CS0446.cs  
using System;
class Tester
{
	static void Main()
	{
		int[] intArray = new int[5];
		foreach (int i in M) { } // CS0446  
	}
	static void M() { }
}