// CS0413.cs  
// compile with: /target:library  
class A { }
class B : A { }

class CMain
{
	A a = null;
	public void G<T>()
	{
		a = new A();
		System.Console.WriteLine(a as T);  // CS0413  
	}

	// OK  
	public void H<T>() where T : A
	{
		a = new A();
		System.Console.WriteLine(a as T);
	}
}