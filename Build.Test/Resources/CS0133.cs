﻿// CS0134.cs  
// compile with: /target:library  
class MyTest { }

class MyClass
{
	const MyTest test = new MyTest();   // CS0134  

	//OK  
	const MyTest test2 = null;
	const System.String test3 = "test";
}