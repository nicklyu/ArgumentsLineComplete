class Foo{
    public virtual int this[int arg1, int arg2, int arg3, int arg4] => 1;
}

class Boo : Foo
{
    public override int this[int arg1, int arg2, int arg3, int arg4] => base[{caret}];
}