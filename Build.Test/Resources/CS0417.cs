// CS0417  
class ExampleClass<T> where T : new()
{
	// The following line causes CS0417.  
	T instance1 = new T(1);

	// The following line doesn't cause the error.  
	T instance2 = new T();
}