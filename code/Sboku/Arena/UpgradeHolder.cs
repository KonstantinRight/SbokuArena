using System;

namespace Sandbox.Sboku.Arena;
public class UpgradeHolder : Component
{
    public const int LevelCap = 5;

    [Property]
    public int FreePoints { get; set; } = 1;

    public Powerup Speed { get; }
    public Powerup Damage { get; }
    public Powerup Armor { get; }

    public UpgradeHolder()
    {
        Speed = new(this, "Speed");
        Damage = new(this, "Damage");
        Armor = new(this, "Armor");
    }

    public class Powerup
    {
        private int level;
        public int Level { get => level; set => Set(ref level, value); }
        public string Display { get; }

        private UpgradeHolder holder;
        public Powerup(UpgradeHolder holder, string display)
        {
            this.holder = holder;
            Display = display;
        }
        private void Set(ref int field, int value)
            => holder.Set(ref field, value);
        public void Inc()
            => Level++;
        public void Dec()
            => Level--;
    }

    private void Set(ref int field, int value)
    {
        var dif = value - field;
        if (Math.Abs(dif) != 1)
        {
            throw new ArgumentException("Level must be only incremented or decremented");
        }

        if (dif > 0)
        {
            if (field < LevelCap && FreePoints > 0)
            {
                FreePoints--;
                field++;
            }
        }
        else
        {
            if (field > 0)
            {
                FreePoints++;
                field--;
            }
        }
    }

    public void DistributeRandomly()
    {
        // TODO:
        throw new NotImplementedException();
    }
}
