// CS0686.cs  
interface I
{
	int get_P();
}

class C : I
{
	public int P
	{
		get { return 1; }  // CS0686  
	}
}
// But the following is valid:  
class D : I
{
	int I.get_P() { return 1; }
	public static void Main() { }
}