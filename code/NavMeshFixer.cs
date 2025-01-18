public sealed class NavMeshFixer : Component
{
    protected override void OnUpdate()
    {
        if (Scene.NavMesh.IsGenerating)
        {
            Scene.NavMesh.Generate(Scene.PhysicsWorld);
            Enabled = false;
        }
        else
        {
            Scene.NavMesh.Generate(Scene.PhysicsWorld);
            Enabled = false;
        }
    }
}