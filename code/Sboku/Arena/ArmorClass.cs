namespace Sandbox.Sboku.Arena;
internal class ArmorClass : Component
{
    public enum Identifier { A, B, C, D, E }

    [Property]
    public Identifier Class { get; set; } = Identifier.E;
    public float Multipler => LetterToMultiplier(Class);

    public static float LetterToMultiplier(Identifier armor) => armor switch
    {
        Identifier.A => 0.05f,
        Identifier.B => 0.1f,
        Identifier.C => 0.25f,
        Identifier.D => 0.5f,
        Identifier.E => 1f,
        _ => throw new System.NotImplementedException("No multiplier for " + armor),
    };
}
