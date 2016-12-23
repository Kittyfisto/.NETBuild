// CS0310.cs  
using System;

class G<T> where T : new()
{
	T t;

	public G()
	{
		t = new T();
		Console.WriteLine(t);
	}
}

class B
{
	private B() { }
	// Try this instead:  
	// public B() { }  
}

class CMain
{
	public static void Main()
	{
		G<B> g = new G<B>();   // CS0310  
		Console.WriteLine(g.ToString());
	}
}