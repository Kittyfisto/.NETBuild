// cs0311.cs  
class B { }
class C { }
class Test<T> where T : C
{ }

class Program
{
	static void Main()
	{
		Test<B> test = new Test<B>(); //CS0311  
	}
}
