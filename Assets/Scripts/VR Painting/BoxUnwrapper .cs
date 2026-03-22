using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class BoxUnwrapper : MonoBehaviour
{
    void Awake()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        Vector3[] verts = mesh.vertices;
        Vector3[] norms = mesh.normals;
        int[]     tris  = mesh.triangles;

        Vector3[] newVerts = new Vector3[tris.Length];
        Vector3[] newNorms = new Vector3[tris.Length];
        Vector2[] newUVs   = new Vector2[tris.Length];
        int[]     newTris  = new int[tris.Length];

        // Calcule les dimensions réelles du mesh pour adapter les UV
        Bounds bounds = mesh.bounds;
        float sizeX = bounds.size.x;
        float sizeY = bounds.size.y;
        float sizeZ = bounds.size.z;

        for (int i = 0; i < tris.Length; i++)
        {
            newVerts[i] = verts[tris[i]];
            newNorms[i] = norms[tris[i]];
            newTris[i]  = i;
        }

        float uOffset = 0f;
        float uSize   = 1f / 6f;

        for (int tri = 0; tri < tris.Length; tri += 3)
        {
            Vector3 n = (newNorms[tri] + newNorms[tri+1] + newNorms[tri+2]) / 3f;
            int face  = GetFaceIndex(n);

            uOffset = face * uSize;

            Vector3 v0 = newVerts[tri];
            Vector3 v1 = newVerts[tri + 1];
            Vector3 v2 = newVerts[tri + 2];

            // Projette chaque vertex sur les 2 axes de sa face
            // en tenant compte des dimensions réelles (pas juste 0-1)
            newUVs[tri]     = ProjectUV(v0, face, uOffset, uSize, sizeX, sizeY, sizeZ);
            newUVs[tri + 1] = ProjectUV(v1, face, uOffset, uSize, sizeX, sizeY, sizeZ);
            newUVs[tri + 2] = ProjectUV(v2, face, uOffset, uSize, sizeX, sizeY, sizeZ);
        }

        mesh.vertices  = newVerts;
        mesh.normals   = newNorms;
        mesh.triangles = newTris;
        mesh.uv        = newUVs;
        mesh.RecalculateBounds();

        var col = GetComponent<MeshCollider>();
        if (col != null) col.sharedMesh = mesh;
    }

    // Projette un vertex sur les bons axes 2D selon la face touchée
    // Les UV sont mis à l'échelle selon les vraies dimensions du mesh
    Vector2 ProjectUV(Vector3 v, int face, float uOffset, float uSize, float sx, float sy, float sz)
    {
        float u, vCoord;

        switch (face)
        {
            case 0: case 1: // droite / gauche — plan YZ
                u      = (v.z / sz + 0.5f) * uSize + uOffset;
                vCoord =  v.y / sy + 0.5f;
                break;
            case 2: case 3: // haut / bas — plan XZ
                u      = (v.x / sx + 0.5f) * uSize + uOffset;
                vCoord =  v.z / sz + 0.5f;
                break;
            default:         // avant / arrière — plan XY
                u      = (v.x / sx + 0.5f) * uSize + uOffset;
                vCoord =  v.y / sy + 0.5f;
                break;
        }

        return new Vector2(u, vCoord);
    }

    int GetFaceIndex(Vector3 normal)
    {
        float ax = Mathf.Abs(normal.x);
        float ay = Mathf.Abs(normal.y);
        float az = Mathf.Abs(normal.z);

        if (ax > ay && ax > az) return normal.x > 0 ? 0 : 1;
        if (ay > ax && ay > az) return normal.y > 0 ? 2 : 3;
        return normal.z > 0 ? 4 : 5;
    }
}