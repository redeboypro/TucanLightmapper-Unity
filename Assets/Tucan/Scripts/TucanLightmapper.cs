using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using static TucanAPI;

public class TucanLightmapper : MonoBehaviour
{
    const string sOutputDir = "Assets\\Tucan\\BakedData";

    [SerializeField]
    private MeshFilter[] m_filters;

    [SerializeField]
    private Vector3      m_lightDirection;

    [SerializeField]
    private float        m_ambientInstensity       = 0.8F;

    [SerializeField, Range(5.0F, 90.0F)]
    private float        m_shadowAngle             = 20.0F;

    [SerializeField]
    private int          m_numberOfPasses          = 12;

    [SerializeField]
    private int          m_numberOfIndirectSamples = 256;

    [SerializeField]
    private int          m_numberOfDirectSamples   = 32;

    [SerializeField]
    private float        m_ambientOcclusionRadius  = 1.0F;

    [SerializeField]
    private Vector2Int   m_lightmapSize            = new(255, 255);

    private void Start()
    {
        var sSceneName              = SceneManager.GetActiveScene().name;
        var sSceneCombinedOutputDir = $"{sOutputDir}\\{sSceneName}";

        if (!Directory.Exists(sSceneCombinedOutputDir))
            Directory.CreateDirectory(sSceneCombinedOutputDir);

        var pScene      = CreateSceneInstance(m_lightmapSize.x, m_lightmapSize.y);
        var asFileNames = new string[m_filters.Length];

        for (int iF = 0; iF < m_filters.Length; ++iF)
        {
            var filter        = m_filters[iF];
            var hMesh         = filter.mesh;
            int cxNumVertices = hMesh.vertexCount;
            int cxNumIndices  = hMesh.triangles.Length;

            var hTransform = filter.transform;

            var afVertices = new float[cxNumVertices * 3];
            for (int iV = 0; iV < cxNumVertices; ++iV)
            {
                var f3Vertex           = hTransform.TransformPoint(hMesh.vertices[iV]);
                afVertices[iV * 3]     = f3Vertex.x;
                afVertices[iV * 3 + 1] = f3Vertex.y;
                afVertices[iV * 3 + 2] = f3Vertex.z;
            }

            var afTexCoords = new float[cxNumVertices * 2];
            for (int iV = 0; iV < cxNumVertices; ++iV)
            {
                var f2TexCoord          = hMesh.uv[iV];
                afTexCoords[iV * 2]     = f2TexCoord.x;
                afTexCoords[iV * 2 + 1] = f2TexCoord.y;
            }

            var auIndices = new uint[cxNumIndices];
            for (int iI = 0; iI < cxNumIndices; iI++)
            {
                auIndices[iI] = (uint) hMesh.triangles[iI];
            }

            unsafe
            {
                fixed (float* pfVertices = afVertices, pfTexCoords = afTexCoords)
                {
                    fixed (uint* puIndices = auIndices)
                    {
                        AttachMesh(
                            pScene,
                            new IntPtr(pfVertices),
                            new IntPtr(pfTexCoords),
                            new IntPtr(puIndices),
                            (short) cxNumVertices,
                            (short) cxNumIndices);
                    }
                }
            }

            asFileNames[iF] = filter.name + ".png";
        }

        FinalizeScene(pScene);
        Bake(pScene, new()
        {
            m_f3LightDir = m_lightDirection,
            m_fAmbientIntensity = m_ambientInstensity,
            m_fSmoothness = Mathf.Sin(m_shadowAngle * Mathf.Deg2Rad),
            m_cxNumIter = m_numberOfPasses,
            m_cxNumIndirSamples = m_numberOfIndirectSamples,
            m_cxNumDirSamples = m_numberOfDirectSamples,
            m_fAORadius = m_ambientOcclusionRadius
        });

        int cbBufferSize = m_lightmapSize.x * m_lightmapSize.y * 4;
        for (int iF = 0; iF < m_filters.Length; ++iF)
        {
            byte[] abManagedArray = new byte[cbBufferSize];
            Marshal.Copy(GetLightmap(pScene, iF), abManagedArray, 0, cbBufferSize);

            var hLightmapTexture = new Texture2D(m_lightmapSize.x, m_lightmapSize.y, TextureFormat.RGBA32, false);
            hLightmapTexture.LoadRawTextureData(abManagedArray);
            hLightmapTexture.Apply();

            var bytes = hLightmapTexture.EncodeToPNG();
            File.WriteAllBytes($"{sSceneCombinedOutputDir}\\{asFileNames[iF]}", bytes);
        }

        Release(pScene);
    }
}
