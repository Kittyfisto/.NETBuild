// CS0552.cs  
public interface ii
{
}

public class a
{
	// delete the routine to resolve CS0552  
	public static implicit operator ii(a aa) // CS0552  
	{
		return new ii();
	}

	public static void Main()
	{
	}
}