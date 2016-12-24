// cs0843.cs  
struct S
{
	public int AIProp { get; set; }
	public S(int i) { } //CS0843  
						// Try the following lines instead.  
						// public S(int i) : this()  
						// {  
						//     AIProp = i;  
						// }  
}

class Test
{
	static int Main()
	{
		return 1;
	}
}