using System;

namespace ConsoleApplication1
{
	class Program
	{
		static void Main(string[] args)
		{
			ExampleClass ec = new ExampleClass();
			ec.ExampleMethod();
			ec.ExampleMethod(10);
			// The following line causes compiler error CS1501 because   
			// ExampleClass does not contain an ExampleMethod that takes  
			// two arguments.  
			ec.ExampleMethod(10, 20);
		}
	}

	// ExampleClass contains two overloads for ExampleMethod. One of them   
	// has no parameters and one has a single parameter.  
	class ExampleClass
	{
		public void ExampleMethod()
		{
			Console.WriteLine("Zero parameters");
		}

		public void ExampleMethod(int i)
		{
			Console.WriteLine("One integer parameter.");
		}

		//// To fix the error, you must add a method that takes two arguments.  
		//public void ExampleMethod (int i, int j)  
		//{  
		//    Console.WriteLine("Two integer parameters.");  
		//}  
	}
}