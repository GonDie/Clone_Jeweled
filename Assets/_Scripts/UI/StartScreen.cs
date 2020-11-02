
public class StartScreen : BaseScreen
{
    public void StartGame()
    {
        ToggleScreen(false, () => GameManager.Instance.StartGame());
    }
}