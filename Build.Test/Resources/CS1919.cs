// cs1919.cs  
// Compile with: /unsafe  
unsafe public class C
{
	public static int Main()
	{
		var col1 = new int* { }; // CS1919  
		var col2 = new char* { }; // CS1919  
		return 1;
	}
}