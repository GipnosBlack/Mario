namespace Mario1.GameObject.Gameobject.Creature
{
    [Flags]
    public enum CreatureState
    {
        None = 0,
        Stands = 1,
        DeadFall = 2,
        Intangible = 4,
        AttackOnEveryone = 8,
        WaitingForMario = 16,
        Jump = 32,
        DoesntKill = 64,
        CoinFalse = 128,
        CoinUp = 256
    }
}
