// CS1009-a.cs  
class MyClass
{
	static void Main()
	{
		// The following line causes CS1009.  
		string a = "\m";
		// Try the following line instead.  
		// string a = "\t";  
	}
}