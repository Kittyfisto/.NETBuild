﻿// CS0507.cs  
abstract public class clx
{
	virtual protected void f() { }
}

public class cly : clx
{
	public override void f() { }   // CS0507  
	public static void Main() { }
}