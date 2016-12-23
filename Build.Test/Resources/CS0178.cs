// CS0178.cs  
class MyClass
{
	public static void Main()
	{
		int a = new int[5][,][][5;   // CS0178  
		int[,] b = new int[3, 2];   // OK  

		int[][] c = new int[10][];
		c[0] = new int[5][5];   // CS0178  
		c[0] = new int[2];   // OK  
		c[1] = new int[2] { 1, 2 };   // OK  
	}
}
