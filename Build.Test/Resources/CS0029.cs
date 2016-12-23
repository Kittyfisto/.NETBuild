// CS0029.cs  
public class MyInt
{
	private int x = 0;

	// Uncomment this conversion routine to resolve CS0029  
	/*  
    public static implicit operator int(MyInt i)  
    {  
       return i.x;  
    }  
    */

	public static void Main()
	{
		MyInt myInt = new MyInt();
		int i = myInt; // CS0029  
	}
}