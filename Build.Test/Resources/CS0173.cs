// CS0173.cs  
public class C { }

public class A
{
	//// The following code defines an implicit conversion operator from    
	//// type C to type A.  
	//public static implicit operator A(C c)  
	//{  
	//    A a = new A();  
	//    a = c;  
	//    return a;  
	//}  
}

public class MyClass
{
	public static void F(bool b)
	{
		A a = new A();
		C c = new C();

		// The following line causes CS0173 because there is no implicit  
		// conversion from a to c or from c to a.  
		object o = b ? a : c;

		// To resolve the error, you can cast a and c.  
		//object o = b ? (object)a : (object)c;  

		// Alternatively, you can add a conversion operator from class C to  
		// class A, or from class A to class C, but not both.  
	}

	public static void Main()
	{
		F(true);
	}
}