using Sandbox.Sboku.Shared;
using SWB.Base;

namespace Sandbox.Sboku;
internal class WeaponAdapter : ISbokuWeapon
{
    private Weapon Weapon { get; }
    public WeaponAdapter(Weapon weapon)
    {
        Weapon = weapon;
    }

    public bool HasAmmo()
    {
        return Weapon.HasAmmo();
    }
}
