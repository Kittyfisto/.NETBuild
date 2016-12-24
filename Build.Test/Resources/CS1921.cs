// cs1921.cs  
using System.Collections;
public class C : CollectionBase
{
	public static void Add(int i)
	{
	}
}
public class Test
{
	public static void Main()
	{
		var collection = new C { 1, 2, 3 }; // CS1921  
	}
}