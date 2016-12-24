// cs1061.cs  
public class TestClass1
{
	// TestClass1 has one method, called WriteSomething.  
	public void WriteSomething(string s)
	{
		System.Console.WriteLine(s);
	}
}

public class TestClass2
{
	// TestClass2 has one method, called DisplaySomething.  
	public void DisplaySomething(string s)
	{
		System.Console.WriteLine(s);
	}
}

public class TestTheClasses
{
	public static void Main()
	{
		TestClass1 tc1 = new TestClass1();
		TestClass2 tc2 = new TestClass2();
		// The following call fails because TestClass1 does not have   
		// a method called DisplaySomething.  
		tc1.DisplaySomething("Hello");      // CS1061  

		// To correct the error, change the method call to either   
		// tc1.WriteSomething or tc2.DisplaySomething.  
		tc1.WriteSomething("Hello from TestClass1");
		tc2.DisplaySomething("Hello from TestClass2");
	}
}