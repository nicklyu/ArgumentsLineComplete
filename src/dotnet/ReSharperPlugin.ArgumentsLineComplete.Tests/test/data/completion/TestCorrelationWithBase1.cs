class Foo{
    public Foo(int arg1, int arg2, int arg3, int arg4)
    {
            
    }
    public Foo(int arg1, int arg2, int arg3)
    {
            
    }
    public Foo(int arg1, int arg2 )
    {
            
    }
    public Foo(int arg1)
    {
            
    }
}

class Boo : Foo
{
    public Boo(int arg1, int arg2, int arg3, int arg4) : base({caret})
    {
            
    }
}