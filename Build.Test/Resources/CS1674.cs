// CS1674.cs  
class C
{
	public static void Main()
	{
		int a = 0;
		a++;

		using (a) { }   // CS1674  
	}
}