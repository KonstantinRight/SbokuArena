using Sandbox.Navigation;
using System;
using System.Collections.Generic;

namespace Sandbox.Sboku;
internal static class Extensions
{
    // Sometimes I get `Failed to compare two elements in the array.` for some reason
    public static List<Vector3> GetSimplePathSafe(this NavMesh mesh, Vector3 from, Vector3 to)
    {
        try
        {
            return mesh.GetSimplePath(from, to);
        }
        catch (Exception e)
        {
            var ex = new Exception("NavMesh fail: " + e.Message, e.InnerException);
            Log.Error(ex);
            return new();
        }
    }
}
