using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Net;
using Unity.VisualScripting;
using Random = UnityEngine.Random;

public class CreateTerrainBase : EditorWindow
{
    string assetName = "TerrainTexture";
    private Vector2 scrollPosition;

    Texture2D terrainTexture;
    private Terrain terrain;
    private int N;
    
    
    Terrain terrainmid;
    private float MidAmplitud;
    private float MidFalloff;
    private int MidSeed;
    
    private Terrain terrainDC;
    private float DCAmplitud;
    private float DCFalloff;
    private int DCSeed;

    private Terrain terrainFBM;
    private float FBMA;
    private float FBMF;
    private int FBMOctaves;
    private int FBMdFrecuencia;
    private float FBMdAmplitud;
    private int FBMSeed;
    private bool Picos = false;
    
    
    //Erosion
    private float T;
    private float c;
    private Terrain localterrain;


    // Add menu item to show the window
    [MenuItem ("TPA/Create terrain base")]
    private static void ShowWindow() {
        var window = GetWindow<CreateTerrainBase>();
        window.titleContent = new GUIContent("Create Terrain Base");
        window.Show();
    }

    private void OnEnable()
    {
        N = 5;
        MidAmplitud = 0.5f;
        MidFalloff = 0.5f;
        MidSeed = 1;

        DCAmplitud = 0.5f;
        DCFalloff = 0.5f;
        DCSeed = 1;
        
        FBMA = 0.5f;
        FBMF = 0.5f;
        FBMdAmplitud = 0.5f;
        FBMdFrecuencia = 2;
        FBMOctaves = 8;
        FBMSeed = 1;
        Picos = false;
        
        T = 0.001f;
        c = 0.5f;
    }

    private void OnGUI() {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height - 20));
        
        
        EditorGUIUtility.labelWidth = 200;
        GUILayout.Label("1.Terrain texture configuration", EditorStyles.boldLabel);
        terrainTexture = EditorGUILayout.ObjectField("Terrain texture", terrainTexture, typeof(Texture2D), true) as Texture2D;
        terrain = EditorGUILayout.ObjectField("Terrain", terrain, typeof(Terrain), true) as Terrain;
        if (GUILayout.Button("Apply texture to terrain")) {
            
            if (terrainTexture == null) {
                Debug.LogError("Texture is null");
                return;
            }
            if (terrain == null) {
                Debug.LogError("Terrain is null");
                return;
            }
            Debug.Log("Applying texture to terrain...");

            
        }
        
        GUILayout.Space(20);
        N = EditorGUILayout.IntSlider("Number of divisions(N)", N, 5,12 );
        GUILayout.Space(20);
        GUILayout.Label("2.Terrain Mid-Point Configuration", EditorStyles.boldLabel);
        GUILayout.Space(5);
        MidAmplitud = EditorGUILayout.Slider("Noise mid-point amplitude ", MidAmplitud, 0.0f, 1.0f);
        MidFalloff = EditorGUILayout.Slider("Amplitude mid-point fallof ", MidFalloff, 0.0f, 1.0f);
        MidSeed = EditorGUILayout.IntField("Mid-point Seed", MidSeed);
        terrainmid  = EditorGUILayout.ObjectField("Mid-point Terrain", terrainmid, typeof(Terrain), true) as Terrain;
        if (GUILayout.Button("Midpoint")) {
            PuntoMedio(MidFalloff, MidAmplitud, N, MidSeed);
        }
        
        GUILayout.Space(20);
        GUILayout.Label("3.Terrain Diamond Square Configuration", EditorStyles.boldLabel);
        GUILayout.Space(5);
        DCAmplitud = EditorGUILayout.Slider("Noise diamond-square amplitude", DCAmplitud, 0.0f, 1.0f);
        DCFalloff = EditorGUILayout.Slider("Amplitude diamond-square fallof", DCFalloff, 0.0f, 1.0f);
        DCSeed = EditorGUILayout.IntField("Diamond-Square Seed", DCSeed);
        terrainDC  = EditorGUILayout.ObjectField("Diamond-Square Terrain", terrainDC, typeof(Terrain), true) as Terrain;
        if (GUILayout.Button("diamante-cuadrado")) {
           DiamanteCuadrado(DCFalloff, DCAmplitud, N, DCSeed);
        }
        
        GUILayout.Space(20);    
        GUILayout.Label("4. Fractal Brownian Motion");
        FBMA = EditorGUILayout.Slider("Amplitud FBM", FBMA, 0.0f, 1.0f);
        FBMF =EditorGUILayout.Slider("Frecuencia FBM", FBMF, 0.0f, 8.0f);
        FBMdAmplitud = EditorGUILayout.Slider("Ganancia Amplitud FBM", FBMdAmplitud, 0.0f, 1.0f);
        FBMdFrecuencia = EditorGUILayout.IntSlider("Ganancia Frecuencia FBM", FBMdFrecuencia, 1, 8);
        FBMOctaves = EditorGUILayout.IntSlider("Octavas FBM", FBMOctaves, 1, 10);
        FBMSeed = EditorGUILayout.IntField("Seed FBM", FBMSeed);
        Picos = EditorGUILayout.Toggle("Enfatizar los picos", Picos);
        terrainFBM  = EditorGUILayout.ObjectField("Terrain", terrainFBM, typeof(Terrain), true) as Terrain;
        
        if (GUILayout.Button("FBM")) {
            FBM(FBMF,FBMA,FBMdAmplitud,FBMdFrecuencia,FBMOctaves,FBMSeed, Picos);  
        }
        
        GUILayout.Space(20);
        GUILayout.Label("5. Erosion");
        c = EditorGUILayout.Slider("Factor de Erosion (c)", c, 0.0f, 1.0f);
        T = EditorGUILayout.Slider("Umbral de Erosion (T)", T, 0.0f, 1.0f);
        localterrain  = EditorGUILayout.ObjectField("Terrain", localterrain, typeof(Terrain), true) as Terrain;
        
        if (GUILayout.Button("Erosion")) {
            float[,] h = localterrain.terrainData.GetHeights(0, 0, localterrain.terrainData.heightmapResolution, localterrain.terrainData.heightmapResolution);
            Erosion(h, T, c, localterrain);
        }
        
        EditorGUILayout.EndScrollView();
    }
        
    void ApplyHeightMap(float[,] h,Terrain terrain)
    {
        TerrainData data = terrain.terrainData;
        Vector3 tam = data.size;
        data.heightmapResolution = h.GetLength(0);
        //NormalizeHeightMap(h);
        data.SetHeights(0, 0, h);
        data.size = tam;
        terrain.terrainData= data;
    }

    void PuntoMedio(float H, float A, int N, int seed)
    {
        int nVertices = (int)Mathf.Pow(2, N) + 1;
        float[,] h = new float[nVertices, nVertices];   

        UnityEngine.Random.InitState(seed);
        int n = 0;
        h[0, 0] = 0.5f + zValue(n, A, H);
        h[0, nVertices - 1] = 0.5f + zValue(n, A, H);
        h[nVertices - 1, 0] = 0.5f + zValue(n, A, H);
        h[nVertices - 1, nVertices - 1] = 0.5f + zValue(n, A, H);

        for (n = 0; n < N; n++)
        {
            int d = (int)Mathf.Pow(2, N - n);
            int d2 = d / 2;
            
            for(int j = 0 ; j < nVertices - 1; j += d)
            {
                for(int i = 0; i + d< nVertices - 1; i += d)
                {
                    h[i + d2, j ] = (h[i, j] + h[i + d, j] ) * 0.5f + zValue(n, A, H);
                }
            }
            
            for(int i = 0 ; i < nVertices - 1; i += d)
            {
                for(int j = 0; j + d< nVertices - 1; j += d)
                {
                    h[i, j + d2] = (h[i, j] + h[i, j + d] ) * 0.5f + zValue(n, A, H);
                }
            }

            for (int i = 0; i + d < nVertices - 1; i += d)
            {
                for (int j = 0; j + d < nVertices - 1; j += d)
                {
                    h[i + d2, j + d2] = (h[i + d2, j] + h[i + d2, j + d] + h[i, j + d2] + h[i + d, j + d2]) * 0.25f + zValue(n, A, H);
                }
            }
            
        }
        ApplyHeightMap(h, terrainmid);
        
    }
    
    float zValue(float n, float A, float H)
    {   
        float factorRuido = Mathf.Pow(2, -n * H) * 0.5f; // Reducir ruido en iteraciones avanzadas
        return Random.Range(-1.0f, 1.0f) * A * factorRuido;
    }


    void DiamanteCuadrado(float H, float A, int N, int seed)
    {
        int nVertices = (int)Mathf.Pow(2, N) + 1;
        float[,] h = new float[nVertices, nVertices];   

        UnityEngine.Random.InitState(seed);
        int n = 0;
        h[0, 0] = 0.5f + zValue(n, A, H);
        h[0, nVertices - 1] = 0.5f + zValue(n, A, H);
        h[nVertices - 1, 0] = 0.5f + zValue(n, A, H);
        h[nVertices - 1, nVertices - 1] = 0.5f + zValue(n, A, H);

        for (n = 0; n < N; n++)
        {
            int d = (int)Mathf.Pow(2, N - n);
            int d2 = d / 2;

            for (int i = 0; i < nVertices - 1; i += d)
            {
                for (int j = 0; j < nVertices - 1; j += d)
                {
                    h[i + d2, j + d2] = (h[i, j] + h[i + d, j] + h[i, j + d] + h[i + d, j + d]) * 0.25f + zValue(n, A, H);
                }
            }

            for (int j = 0; j < nVertices; j += d)
            {
                for (int i = 0; i + d < nVertices; i += d)
                {
                    if (j == 0)
                        h[i + d2, j] = (h[i, j] + h[i + d, j] + h[i + d2,j + d2]) * 0.33f + zValue(n, A, H);
                    else if (j == nVertices - 1)
                        h[i + d2, j] = (h[i , j] + h[i + d, j] + h[i + d2,j - d2]) * 0.33f + zValue(n, A, H);
                    else
                        h[i + d2, j] = (h[i , j] + h[i + d, j ] + h[i + d2, j + d2] + h[i + d2, j - d2]) * 0.25f + zValue(n, A, H);
                }
            }

            for (int i = 0; i < nVertices; i += d)
            {
                for (int j = 0; j + d < nVertices; j += d)
                {
                    if (i == 0)
                        h[i, j + d2] = (h[i, j] + h[i, j + d] + h[i + d2,j + d2]) * 0.33f + zValue(n, A, H);
                    else if (i == nVertices - 1)
                        h[i, j + d2] = (h[i, j] + h[i, j + d] + h[i - d2,j + d2]) * 0.33f + zValue(n, A, H);
                    else
                        h[i, j + d2] = (h[i , j] + h[i , j + d] + h[i - d2, j + d2] + h[i + d2, j + d2]) * 0.25f + zValue(n, A, H);
                }
            }

        }


        ApplyHeightMap(h,terrainDC);
    }

     
    void FBM(float F, float A, float dA, int dF, int Octaves, int seed, bool Picos)
{
    int nVertices = (int)Mathf.Pow(2, N) + 1;
    float[,] h = new float[nVertices, nVertices];

    UnityEngine.Random.InitState(seed);

    float FactorN = 1.0f / (nVertices - 1);  // Factor de normalización

    for (int i = 0; i < nVertices; i++)
    {
        for (int j = 0; j < nVertices; j++)
        {
            float total = 0f;
            float frequency = F;
            float amplitude = A;
            float weight = 1.0f;  // Para Picos

            for (int k = 0; k < Octaves; k++)
            {
                float x = i * FactorN * frequency;
                float y = j * FactorN * frequency;
                
                float noiseValue = Mathf.PerlinNoise(x, y);

                if (Picos)
                {
                    total += weight * amplitude * noiseValue;
                    weight = total;  // Acumulación en caso de Picos
                    
                }
                else
                {
                    total += amplitude * noiseValue;
                }

                frequency *= dF;
                amplitude *= dA;
            }
            //Debug.Log(weight);

            h[i, j] = total;
        }
    }

    ApplyHeightMap(h, terrainFBM);
}



    void Erosion(float[,] h, float T , float c, Terrain localterrain)
    {
        int nVertices = (int)Mathf.Pow(2, N) + 1;
        float d1 = 0.0f;
        float d2 = 0.0f;
        float d3 = 0.0f;
        float d4 = 0.0f;
        float dmax = 0.0f;  
        float dtotal = 0.0f;
        
        for (int i = 0; i < nVertices; i++)
        {
            for (int j = 0; j < nVertices; j++)
            {
                if (i == 0)
                {
                    d2 = 0;
                }
                else
                {
                    d2 = h[i, j] - h[i - 1, j ];
                }

                if (j == 0)
                {
                    d4 = 0;
                }
                else
                {
                    d4 = h[i, j] - h[i, j - 1];
                }

                if (i == nVertices - 1)
                {
                    d1 = 0;
                }
                else
                {
                    d1 = h[i, j] - h[i + 1, j ];
                }

                if (j == nVertices - 1)
                {
                    d3 = 0;
                }
                else
                {
                    d3 = h[i, j] - h[i, j + 1];
                }
                
                
                
                
                dmax = Mathf.Max(d1, d2, d3, d4);
                
                if(d1 > T)
                    dtotal += d1 ;
                if(d2 > T)
                    dtotal += d2 ;
                if(d3 > T)
                    dtotal += d3 ;
                if(d4 > T)
                    dtotal += d4 ;
                
                

                h[i, j] -= c * (dmax - T);
                
                if(d1 > T)
                    h[i + 1, j ] += c*(dmax - T)*(d1/dtotal);
                if(d2 > T)
                    h[i - 1, j ] += c*(dmax - T)*(d2/dtotal);
                if(d3 > T)
                    h[i, j + 1] += c*(dmax - T)*(d3/dtotal);
                if(d4 > T)
                    h[i, j - 1] += c*(dmax - T)*(d4/dtotal);
                
            }
            dtotal = 0;
            
        }
        
        ApplyHeightMap(h, localterrain);
    }

}


