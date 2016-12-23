// CS0039.cs  
using System;
class A
{
}
class B : A
{
}
class C : A
{
}
class M
{
	static void Main()
	{
		A a = new C();
		B b = new B();
		C c;

		// This is valid; there is a built-in reference  
		// conversion from A to C.  
		c = a as C;

		//The following generates CS0039; there is no  
		// built-in reference conversion from B to C.  
		c = b as C;  // CS0039  
	}
}