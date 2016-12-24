using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CS1656_2
{

	class Book
	{
		public string Title;
		public string Author;
		public double Price;
		public Book(string t, string a, double p)
		{
			Title = t;
			Author = a;
			Price = p;

		}
	}

	class Program
	{
		private List<Book> list;
		static void Main(string[] args)
		{
			Program prog = new Program();
			prog.list = new List<Book>();
			prog.list.Add(new Book("The C# Programming Language",
									"Hejlsberg, Wiltamuth, Golde",
									 29.95));
			prog.list.Add(new Book("The C++ Programming Language",
									"Stroustrup",
									 29.95));
			prog.list.Add(new Book("The C Programming Language",
									"Kernighan, Ritchie",
									29.95));
			foreach (Book b in prog.list)
			{
				// Cannot modify an entire element in a foreach loop   
				// even with reference types.  
				// Use a for or while loop instead  
				if (b.Title == "The C Programming Language")
					// Cannot assign to 'b' because it is a 'foreach   
					// iteration variable'  
					b = new Book("Programming Windows, 5th Ed.", "Petzold", 29.95); //CS1656  
			}

			//With a for loop you can modify elements  
			//for(int x = 0; x < prog.list.Count; x++)  
			//{  
			//    if(prog.list[x].Title== "The C Programming Language")  
			//        prog.list[x] = new Book("Programming Windows, 5th Ed.", "Petzold", 29.95);  
			//}  
			//foreach(Book b in prog.list)  
			//    Console.WriteLine(b.Title);  

		}
	}
}