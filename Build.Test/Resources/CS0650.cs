// CS0650.cs  
public class MyClass
{
	public static void Main()
	{
		// Generates CS0650. Incorrect array declaration syntax:  
		int myarray[2];

		// Correct declaration.  
		int[] myarray2;

		// Declaration and initialization in one statement  
		int[] myArray3 = new int[2] { 1, 2 }
 
	  // Access an array element.  
		myarray3[0] = 0;
	}
}