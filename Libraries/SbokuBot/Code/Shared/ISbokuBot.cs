using Sandbox.AI.Default;
using Sandbox.Sboku;
using Sandbox.Sboku.Shared;
using System.Collections.Generic;

namespace Sandbox.Shared;
public interface ISbokuBot
{
    CharacterController Character { get; set; }
    Vector3 EyePos { get; }
    Vector3 HeightToAimAt { get; }
    bool IsNavigating { get; }
    bool IsOffline { get; set; }
    bool IsReloading { get; set; }
    bool IsShooting { get; set; }
    int MaxFightRange { get; set; }
    int MinFightRange { get; set; }
    int SearchRange { get; set; }
    SbokuSettings Settings { get; }
    ISbokuTarget Target { get; set; }
    ISbokuWeapon Weapon { get; }

    bool IsActiveActionState<T>() where T : IActionState;
    bool IsActiveCombatState<T>() where T : ICombatState;
    void MoveTo(List<Vector3> path);
    void MoveTo(Vector3 targetPosition);
    void SetActionState<T>() where T : IActionState;
    void SetCombatState<T>() where T : ICombatState;
    void StopNavigating();
}