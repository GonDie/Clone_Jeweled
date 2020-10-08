
public static class Events
{
    public static event SimpleEvent OnGameStart;
}

public delegate void SimpleEvent();
public delegate void IntEvent(int i);
public delegate void FloatEvent(float f);
public delegate void BoolEvent(bool b);
public delegate void GenericEvent<T>(T t);