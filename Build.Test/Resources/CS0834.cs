// cs0834.cs  
using System;
using System.Linq;
using System.Linq.Expressions;

public class C
{
	public static int Main()
	{
		Expression<Func<int, int>> e = x => { return x; }; // CS0834  
	}
}