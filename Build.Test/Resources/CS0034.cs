namespace CS0034
{
	class TestClass2
	{
		public void Test()
		{
			TestClass o1 = new TestClass();
			TestClass o2 = new TestClass();
			TestClass o3 = o1 & o2; //CS0034  
		}
	}

	class TestClass
	{
		public static implicit operator int(TestClass o)
		{
			return 1;
		}

		public static implicit operator TestClass(int v)
		{
			return new TestClass();
		}

		public static implicit operator bool(TestClass o)
		{
			return true;
		}
	}

}