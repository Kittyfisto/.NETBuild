// CS1018.cs  
public class C
{
}

public class a : C
{
	public a(int i)
	{
	}

	public a() :   // CS1018  
	// possible resolutions:  
	// public a () resolves by removing the colon  
	// public a () : base() calls C's default constructor  
	// public a () : this(1) calls the assignment constructor of class a  
	{
	}

	public static int Main()
	{
		return 1;
	}
}
