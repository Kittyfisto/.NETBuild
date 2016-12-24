// cs0826.cs  
public class C
{
	public static int Main()
	{
		var x = new[] { 1, "str" }; // CS0826  

		char c = 'c';
		short s1 = 0;
		short s2 = -0;
		short s3 = 1;
		short s4 = -1;

		var array1 = new[] { s1, s2, s3, s4, c, '1' }; // CS0826  
		return 1;
	}
}