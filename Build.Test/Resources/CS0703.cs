// CS0703.cs  
internal interface I { }
public class C<T> where T : I  // CS0703 – I is internal; C<T> is public  
{
}
