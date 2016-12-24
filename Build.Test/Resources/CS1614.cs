// CS1614.cs  
using System;

// Both of the following classes are valid attributes with valid  
// names (MySpecial and MySpecialAttribute). However, because the lookup  
// rules for attributes involves auto-appending the 'Attribute' suffix  
// to the identifier, these two attributes become ambiguous; that is,  
// if you specify MySpecial, the compiler can't tell if you want  
// MySpecial or MySpecialAttribute.  

public class MySpecial : Attribute
{
	public MySpecial() { }
}

public class MySpecialAttribute : Attribute
{
	public MySpecialAttribute() { }
}

class MakeAWarning
{
	[MySpecial()] // CS1614  
				  // Ambiguous: MySpecial or MySpecialAttribute?  
	public static void Main()
	{
	}

	[@MySpecial()] // This isn't ambiguous, it binds to the first attribute above.  
	public static void NoWarning()
	{
	}

	[MySpecialAttribute()] // This isn't ambiguous, it binds to the second attribute above.  
	public static void NoWarning2()
	{
	}

	[@MySpecialAttribute()] // This is also legal.  
	public static void NoWarning3()
	{
	}
}