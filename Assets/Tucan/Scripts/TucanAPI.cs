using System;
using System.Runtime.InteropServices;
using UnityEngine;

public static class TucanAPI
{
    #region [NativeImports]
    const string sDllName = "tucanlib";

    [DllImport(sDllName)]
    public static extern IntPtr CreateSceneInstance(int iTexW, int iTexH);

    [DllImport(sDllName)]
    public static extern uint AttachMesh(
        IntPtr pScene,
        IntPtr pfVertices,
        IntPtr pfTexCoords,
        IntPtr puIndices,
        short cxNumVertices,
        short cxNumIndices
        );

    [DllImport(sDllName)]
    public static extern void FinalizeScene(IntPtr pScene);

    [DllImport(sDllName)]
    public static extern void Bake(IntPtr pScene, BakeParameters parameters);

    [DllImport(sDllName)]
    public static extern void Release(IntPtr pScene);

    [DllImport(sDllName)]
    public static extern IntPtr GetLightmap(IntPtr pScene, int iLightmapIndex);
    #endregion

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct BakeParameters
    {
        public Vector3  m_f3LightDir;
        public float    m_fAmbientIntensity;
        public float    m_fSmoothness;
        public int      m_cxNumIter;
        public int      m_cxNumIndirSamples;
        public int      m_cxNumDirSamples;
        public float    m_fAORadius;
    }
}
