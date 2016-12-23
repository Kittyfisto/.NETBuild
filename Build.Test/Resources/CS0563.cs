// CS0563.cs  
public class iii
{
	public static implicit operator int(iii x)
	{
		return 0;
	}
	public static implicit operator iii(int x)
	{
		return null;
	}
	public static int operator +(int aa, int bb)   // CS0563   
												   // Use the following line instead:  
												   // public static int operator +(int aa, iii bb)      
	{
		return 0;
	}
	public static void Main()
	{
	}
}
