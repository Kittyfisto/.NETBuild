
// CS0304.cs  
// Compile with: /target:library.  
class C<T>
{
	// The following line generates CS0304.  
	T t = new T();
}