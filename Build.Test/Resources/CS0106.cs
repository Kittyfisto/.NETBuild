// CS0106.cs  
namespace MyNamespace
{
	interface I
	{
		void m();
		static public void f();   // CS0106  
	}

	public class MyClass
	{
		public void I.m() { }   // CS0106  
		public static void Main() { }
	}
}