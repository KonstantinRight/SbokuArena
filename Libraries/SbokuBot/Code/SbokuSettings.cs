namespace Sandbox.Sboku;
[Group("Sboku Bot")]
public class SbokuSettings : Component
{
    private static object lockObject = new object();
    /// <summary>
    /// Use it once per component
    /// </summary>
    /// <param name="scene"></param>
    /// <returns></returns>
    public static SbokuSettings CreateOrFind(Scene scene)
    {
        lock (lockObject)
        {
            var conf = scene.GetComponentInChildren<SbokuSettings>();
            if (conf != null)
                return conf;

            var obj = scene.CreateObject();
            obj.Name = "Sboku Settings Holder";
            return obj.AddComponent<SbokuSettings>();
        }
    }

    [Property]
    public float ThinkingInterval { get; set; } = 0.5f;
    [Property]
    public float CoverScanAngle { get; set; } = 30f;
    [Property]
    public bool ShowDebugOverlay { get; set; } = false;
}
