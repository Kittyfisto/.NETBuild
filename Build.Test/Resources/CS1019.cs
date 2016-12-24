// CS1019.cs  
public class ii
{
	int i
	{
		get
		{
			return 0;
		}
	}
}

public class a
{
	public int i;
	// Generates CS1019: "ii" is not a unary operator.  
	public static a operator ii(a aa)

	// Use the following line instead:  
	//public static a operator ++(a aa)  
	{
		aa.i++;
		return aa;
	}

	public static void Main()
	{
	}
}
