// CS0229.cs  

interface IList
{
	int Count
	{
		get;
		set;
	}

	void Counter();
}

interface Icounter
{
	double Count
	{
		get;
		set;
	}
}

interface IListCounter : IList, Icounter { }

class MyClass
{
	void Test(IListCounter x)
	{
		x.Count = 1;  // CS0229  
					  // Try one of the following lines instead:  
					  // ((IList)x).Count = 1;  
					  // or  
					  // ((Icounter)x).Count = 1;  
	}

	public static void Main() { }
}
