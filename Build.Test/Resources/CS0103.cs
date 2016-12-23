using System;

class MyClass1
{
	public static void Main()
	{
		try
		{
			// The following declaration is only available inside the try block.  
			MyClass1 conn = new MyClass1();
		}
		catch (Exception e)
		{
			// The following expression causes error CS0103, because variable  
			// conn only exists in the try block.  
			if (conn != null)
				Console.WriteLine("{0}", e);
		}
	}
}