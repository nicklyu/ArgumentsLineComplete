class Foo
{
    void Test(int arg1, int arg2, int arg3, ref int arg4)
    {
            
    }
        
    void Test(int arg1, int arg2, int arg3)
    {
            
    }
        
    void Test(int arg1, int arg2)
    {
            
    }
        
    void Test2(int arg1, int arg2)
    {
        var arg3 = 1;
        var arg4 = 2;
        Test({caret});
    }
}