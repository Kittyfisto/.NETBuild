// CS0270.cs  
// compile with: /t:module  

public class Test
{
	int[10] a;   // CS0270  
				 // To resolve, use the following line instead:  
				 // int[] a = new int[10];  
}