using System;

public class PlayerInstanceManager
{
    private static Player player;

    public static Player Player
    {
        get => player;
        set
        {
            if (value != null)
                player = value;
            else
                throw new NullReferenceException("Player cannot be null");
        }
    }

    void Update()
    {

    }
}
