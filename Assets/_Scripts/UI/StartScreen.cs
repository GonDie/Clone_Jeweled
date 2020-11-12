
public class StartScreen : BaseScreen
{
    public void StartGame()
    {
        ToggleScreen(false, () => Events.CallStartGame?.Invoke());
    }
}