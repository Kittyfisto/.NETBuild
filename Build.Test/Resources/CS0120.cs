// CS0120_1.cs  
public class MyClass
{
	// Non-static field  
	public int i;
	// Non-static method  
	public void f() { }
	// Non-static property  
	int Prop
	{
		get
		{
			return 1;
		}
	}

	public static void Main()
	{
		i = 10;   // CS0120  
		f();   // CS0120  
		int p = Prop;   // CS0120  
						// try the following lines instead  
						// MyClass mc = new MyClass();  
						// mc.i = 10;  
						// mc.f();  
						// int p = mc.Prop;  
	}
}
