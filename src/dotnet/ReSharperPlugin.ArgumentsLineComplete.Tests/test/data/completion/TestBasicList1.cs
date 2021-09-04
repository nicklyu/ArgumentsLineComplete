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
        
    void Test2(int arg1, int arg2, int arg3, int arg4)
    {
        Test({caret});
    }
}