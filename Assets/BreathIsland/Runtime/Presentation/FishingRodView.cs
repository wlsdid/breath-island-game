using UnityEngine;

namespace BreathIsland
{
    // Deforms a real tapered tube and moves line guides with its centreline.
    public sealed class FishingRodView : MonoBehaviour
    {
        public MeshFilter shaft;
        public Transform reelHandle;
        public Transform[] guides;
        public LineRenderer thread;
        private Mesh mesh;
        private readonly Vector3[] vertices = new Vector3[25 * 8];
        public Vector3 Tip { get; private set; }

        private void Awake()
        {
            mesh = new Mesh { name = "Flexible fishing rod" };
            var triangles = new int[24 * 8 * 6];
            int index = 0;
            for (int ring = 0; ring < 24; ring++) for (int side = 0; side < 8; side++)
            {
                int a = ring * 8 + side, b = ring * 8 + (side + 1) % 8;
                triangles[index++] = a; triangles[index++] = b; triangles[index++] = a + 8;
                triangles[index++] = b; triangles[index++] = b + 8; triangles[index++] = a + 8;
            }
            mesh.vertices = vertices; mesh.triangles = triangles;
            shaft.sharedMesh = mesh;
            Pose(0, 0);
        }

        public void Pose(float tension, float reelAngle)
        {
            for (int ring = 0; ring <= 24; ring++)
            {
                float t = ring / 24f, radius = Mathf.Lerp(.023f, .006f, t);
                Vector3 center = FishingPose.RodPoint(t, tension);
                for (int side = 0; side < 8; side++)
                {
                    float angle = side * Mathf.PI / 4;
                    vertices[ring * 8 + side] = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
                }
            }
            mesh.vertices = vertices; mesh.RecalculateNormals(); mesh.RecalculateBounds();
            for (int i = 0; i < guides.Length; i++)
                guides[i].localPosition = FishingPose.RodPoint((i + 1f) / guides.Length, tension) + Vector3.down * .025f;
            thread.SetPosition(0, transform.TransformPoint(new Vector3(0, -.16f, -.1f)));
            for (int i = 0; i < guides.Length; i++) thread.SetPosition(i + 1, guides[i].position);
            reelHandle.localRotation = Quaternion.Euler(reelAngle, 0, 0);
            Tip = transform.TransformPoint(FishingPose.RodPoint(1, tension));
        }
        private void OnDestroy() { if (mesh != null) Destroy(mesh); }
    }
}
