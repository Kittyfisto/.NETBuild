// CS0051.cs  
public class A
{
	// Try making B public since F is public  
	// B is implicitly private here  
	class B
	{
	}

	public static void F(B b)  // CS0051  
	{
	}

	public static void Main()
	{
	}
}
