// cs0845.cs  
using System;
using System.Linq;
using System.Linq.Expressions;

namespace ConsoleApplication1
{
	class Program
	{
		static void Main(string[] args)
		{
			Expression<Func<object>> e = () => null ?? null; // CS0845  
															 // Try the following line instead.  
															 // Expression<Func<object>> e = () => (object)null ?? null;  
		}
	}
}