// CS0545.cs  
// compile with: /target:library  
// CS0545  
public class a
{
	public virtual int i
	{
		set { }

		// Uncomment the following line to resolve.  
		// get { return 0; }  
	}
}

public class b : a
{
	public override int i
	{
		get { return 0; }
		set { }   // OK  
	}
}