class Program
{
	delegate void Del();
	static void Main(string[] args)
	{
		// The following line causes CS0165 because variable d is used   
		// as an argument before it has been initialized.  
		Del d = delegate () { System.Console.WriteLine(d); };
	}
}