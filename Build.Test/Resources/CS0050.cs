// CS0050.cs  
class MyClass //accessibility defaults to private  
			  // try the following line instead  
			  // public class MyClass   
{
}

public class MyClass2
{
	public static MyClass MyMethod()   // CS0050  
	{
		return new MyClass();
	}

	public static void Main() { }
}