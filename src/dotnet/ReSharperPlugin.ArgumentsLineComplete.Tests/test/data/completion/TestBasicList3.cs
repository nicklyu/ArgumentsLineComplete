class Foo
{
    void Test(int arg1, int arg2, int arg3, int arg4)
    {
            
    }
        
    void Test(int arg1, int arg2, int arg3)
    {
            
    }
        
    void Test(int arg1, int arg2)
    {
            
    }
        
    void Test3(int arg1, int arg3)
    {
        var arg2 = 1;
        Test({caret});
    }
}