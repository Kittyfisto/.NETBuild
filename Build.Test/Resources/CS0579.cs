// CS0579.cs  
using System;
public class MyAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public class MyAttribute2 : Attribute
{
}

public class z
{
	[MyAttribute, MyAttribute]     // CS0579  
	public void zz()
	{
	}

	[MyAttribute2, MyAttribute2]   // OK  
	public void zzz()
	{
	}

	public static void Main()
	{
	}
}