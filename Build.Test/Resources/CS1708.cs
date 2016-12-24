// CS1708.cs  
// compile with: /unsafe  
using System;

unsafe public struct S
{
	public fixed char name[10];
}

public unsafe class C
{
	public S UnsafeMethod()
	{
		S myS = new S();
		return myS;
	}

	static void Main()
	{
		C myC = new C();
		myC.UnsafeMethod().name[3] = 'a';  // CS1708  
										   // Uncomment the following 2 lines to resolve:  
										   // S myS = myC.UnsafeMethod();  
										   // myS.name[3] = 'a';  

		// The field cannot be static.  
		C._s1.name[3] = 'a';  // CS1708  

		// The field cannot be readonly.  
		myC._s2.name[3] = 'a';  // CS1708  
	}

	static readonly S _s1;
	public readonly S _s2;
}