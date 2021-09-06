class Foo{
    public virtual int this[int arg1, int arg2, int arg3, int arg4] => 1;
}

class Boo : Foo
{
    public Boo(int arg1)
    {
        var arg4 = 1;
        var arg2 = 2;
        var arg3 = 3;
        this[{caret}]
    }
}