// CS0523.cs  
// compile with: /target:library  
struct RecursiveLayoutStruct1
{
	public RecursiveLayoutStruct2 field;
}

struct RecursiveLayoutStruct2
{
	public RecursiveLayoutStruct1 field;   // CS0523  
}