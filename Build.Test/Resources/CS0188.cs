// CS0188.cs  
// compile with: /t:library  
namespace MyNamespace
{
	class MyClass
	{
		struct S
		{
			public int a;

			void MyMethod()
			{
			}

			S(int i)
			{
				// a = i;  
				MyMethod();  // CS0188  
			}
		}
		public static void Main()
		{ }

	}
}