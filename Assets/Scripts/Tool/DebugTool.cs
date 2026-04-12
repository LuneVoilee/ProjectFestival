using UnityEngine;

namespace Tool
{
    public static class DebugTool
    {
        public static void CreateCube(Vector3 worldPosition)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "DebugCube";
            cube.transform.position = worldPosition;
            cube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        }
    }
}
