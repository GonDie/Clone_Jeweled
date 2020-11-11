using UnityEngine;

public static class Events
{
    public static IntEvent OnGamePrepare;
    public static SimpleEvent OnGameStart;
    public static SimpleEvent OnGameEnd;
    public static SimpleEvent OnGameTimeout;
    public static SimpleEvent OnGameWon;
    public static SimpleEvent OnGameReset;

    public static FloatEvent OnTimeUpdate;
    public static FloatVector3Event OnPieceKill;

    public static FloatEvent OnScoreUpdate;
    public static FloatEvent OnTargetScoreUpdate;
    public static FloatEvent OnScoreUpdateToNextLevelPercent;

    public static MouseStateEvent OnMouseStateEvent;

    public static IntEvent OnCameraShake;
}

public delegate void SimpleEvent();
public delegate void IntEvent(int i);
public delegate void FloatEvent(float f);
public delegate void BoolEvent(bool b);
public delegate void GenericEvent<T>(T t);

public delegate void MouseStateEvent(MouseState ms);
public delegate void FloatVector3Event(float f, Vector3 v3);
