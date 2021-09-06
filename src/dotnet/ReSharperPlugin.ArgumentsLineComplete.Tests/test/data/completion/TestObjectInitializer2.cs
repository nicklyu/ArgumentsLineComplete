namespace ReSharperPlugin.ArgumentsLineComplete.Tests.test.data.completion;

public class Boo
{
    public Boo(int arg1, int arg2, int arg3, int arg4) 
    {
    }
    public Boo(int arg1, int arg2, int arg3)
    {
            
    }
    public Boo(int arg1, int arg2 )
    {
            
    }
    public Boo(int arg1)
    {
        var arg4 = 1;
        var arg2 = 2;
        var arg3 = 3;
        new Boo({caret})
    }
}