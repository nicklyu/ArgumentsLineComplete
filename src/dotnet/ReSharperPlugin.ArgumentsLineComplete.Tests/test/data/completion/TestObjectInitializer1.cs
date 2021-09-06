namespace ReSharperPlugin.ArgumentsLineComplete.Tests.test.data.completion;

public class Boo
{
    public Boo(int arg1, int arg2, int arg3, int arg4) 
    {
        new Boo({caret})
    }
    public Boo(int arg1, int arg2, int arg3)
    {
            
    }
    public Boo(int arg1, int arg2 )
    {
            
    }
    public Boo(int arg1)
    {
            
    }
}