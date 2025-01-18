using Sandbox.Sboku.Shared;
using SWB.Base;
using System;

namespace Sandbox.Sboku;
internal class WeaponAdapter : ISbokuWeapon
{
    private Weapon weapon;
    public WeaponAdapter(Weapon weapon)
    {
        this.weapon = weapon;
    }

    public bool HasAmmo()
        => weapon.HasAmmo();

    public override bool Equals(object obj)
    {
        if (obj is ISbokuWeapon wep)
        {
            return wep == weapon;
        }
        return false;
    }

    public override int GetHashCode()
        => HashCode.Combine(weapon);
}
