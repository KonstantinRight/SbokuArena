using System;
using System.Collections.Generic;

namespace Sandbox.Sboku.Arena;
[Group("Sboku Arena")]
public class UpgradeHolder : Component
{
    #region Leveling

    public const int LevelCap = 5;
    private static Random rand = new Random();

    [Property]
    public int FreePoints { get; set; } = 1;

    public Powerup Speed { get; private set; }
    public Powerup Damage { get; private set; }
    public Powerup Armor { get; private set; }

    public UpgradeHolder()
    {
        Clear();
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
        var points = FreePoints;
        var indexes = new List<int> { 0, 1, 2 };
        while (points > 0)
        {
            var selected = indexes[rand.Next(indexes.Count)];
            switch (selected)
            {
                case 0:
                    points--;
                    Speed.Inc();
                    if (Speed.Level >= LevelCap)
                        indexes.Remove(selected);
                    break;

                case 1:
                    points--;
                    Damage.Inc();
                    if (Damage.Level >= LevelCap)
                        indexes.Remove(selected);
                    break;

                case 2:
                    points--;
                    Armor.Inc();
                    if (Armor.Level >= LevelCap)
                        indexes.Remove(selected);
                    break;
            }
        }
    }

    public void Clear()
    {
        FreePoints = 1;
        Speed = new(this, "Speed");
        Damage = new(this, "Damage");
        Armor = new(this, "Armor");
    }

    #endregion

    #region Powerups

    public enum Level { A, B, C, D, E, F }
    public Level SpeedClass { get => IntToLevel(Speed.Level); } 
    public Level DamageClass { get => IntToLevel(Damage.Level); } 
    public Level ArmorClass { get => IntToLevel(Armor.Level); }

    private Level IntToLevel(int level) => level switch
    {
        0 => Level.F,
        1 => Level.E,
        2 => Level.D,
        3 => Level.C,
        4 => Level.B,
        5 => Level.A,
        _ => throw new NotImplementedException("No level for " + level + " number")
    };

    public float SpeedMultiplier { get => SpeedClass switch
        {
            Level.A => 1.75f,
            Level.B => 1.5f,
            Level.C => 1.25f,
            Level.D => 1.05f,
            Level.E => 0.9f,
            Level.F => 0.75f,
            _ => throw new NotImplementedException("No multiplier for " + SpeedClass),
        };
    }
    public float DamageMultiplier { get => DamageClass switch
        {
            Level.A => 8f,
            Level.B => 5f,
            Level.C => 3f,
            Level.D => 1.5f,
            Level.E => 1.1f,
            Level.F => 1f,
            _ => throw new NotImplementedException("No multiplier for " + DamageClass),
        };
    }
    public float ArmorMultiplier { get => ArmorClass switch
        {
            Level.A => 0.1f,
            Level.B => 0.15f,
            Level.C => 0.25f,
            Level.D => 0.5f,
            Level.E => 0.75f,
            Level.F => 1f,
            _ => throw new System.NotImplementedException("No multiplier for " + ArmorClass),
        };
    }

    #endregion
}