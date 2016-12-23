// CS0116.cs  
namespace x
{
	using System;

	// method must be in class/struct  
	void Method(string str) // CS0116  
	{
		Console.WriteLine(str);
	}
	// To fix the error, you must  
	// enclose a method in a class:  
	class Program
	{
		void Method2(string str)
		{
			Console.WriteLine(str);
		}
	}
}