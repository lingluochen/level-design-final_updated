using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


public class LWRPSSAORendererFeature : UnityEngine.Rendering.Universal.ScriptableRendererFeature
{
    class CustomRenderPass : UnityEngine.Rendering.Universal.ScriptableRenderPass
    {
        public Material SSAOBlurMaterial;
        public Material SSAOMaterial;
        public Material SSAOCombineMaterial;
    
        public BasicSSAOSettings Settings;

        public RenderTargetIdentifier targetColorTex, depthTex;
        private CommandBuffer buffer;

        private int FirstScreenCopy; 
        private int SSAOTarget; 
        private int SSAOTargetV, SSAOTargetH;

        private List<int> TRT = new List<int>();
        private int GetTRT(string name, CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor, FilterMode mode = FilterMode.Bilinear)
        {
            int trti = Shader.PropertyToID(name);
            cmd.GetTemporaryRT(trti, cameraTextureDescriptor.width, cameraTextureDescriptor.height, cameraTextureDescriptor.depthBufferBits, mode, cameraTextureDescriptor.colorFormat, RenderTextureReadWrite.Default, 1, false, RenderTextureMemoryless.None, false);
            TRT.Add(trti);
            return trti;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        { 
            buffer = new CommandBuffer();
            buffer.name = "Basic SSAO";
             
            RenderTextureDescriptor ssaoTargetDesc = new RenderTextureDescriptor();
            ssaoTargetDesc.depthBufferBits = 0;
            ssaoTargetDesc.colorFormat = RenderTextureFormat.ARGBHalf;//cameraTextureDescriptor.colorFormat; //theoretically cameraTextureDescriptor.colorFormat = RenderTextureForamt.ARGBHalf but somehow it is not for rendering api
            ssaoTargetDesc.width = cameraTextureDescriptor.width;
            ssaoTargetDesc.height = cameraTextureDescriptor.height;

            FirstScreenCopy = GetTRT("_FirstScreenCopy", cmd, ssaoTargetDesc);

            ssaoTargetDesc.width = cameraTextureDescriptor.width / Settings.Downsampling;
            ssaoTargetDesc.height = cameraTextureDescriptor.height / Settings.Downsampling;

            SSAOMaterial.SetVector("_ScreenSize", new Vector2(ssaoTargetDesc.width, ssaoTargetDesc.height));
            SSAOTarget = GetTRT("_SSAOTex", cmd, ssaoTargetDesc); 

            SSAOTargetH = GetTRT("_SSAOTexH", cmd, ssaoTargetDesc);
            SSAOTargetV = GetTRT("_SSAOTexV", cmd, ssaoTargetDesc);
        }
         
        private void UpdateUniforms(UnityEngine.Rendering.Universal.RenderingData renderingData)
        {
            SSAOCombineMaterial.SetFloat("_AOIntensity", Settings.AOIntensity);
            SSAOCombineMaterial.SetColor("_AOColor", Settings.SSAOColor);

            SSAOMaterial.SetTexture("_RandomTex", Settings.RandomTexture);
            SSAOMaterial.SetFloat("_SelfShadowingReduction", Settings.SelfShadowingReduction); 
            SSAOMaterial.SetFloat("_Area", Settings.AcceptableDepthDifference);
            SSAOMaterial.SetFloat("_OcclusionFactor", 1.0f / Settings.OcclusionFactor);
            SSAOMaterial.SetFloat("_Radius", Settings.Radius);
            SSAOMaterial.SetFloat("_DepthCutoff", Settings.DepthCutoff);
            SSAOMaterial.SetFloat("_DepthCutoffSmoothTransitionRange", Settings.DepthCutoffSmoothTransitionRange);
            SSAOMaterial.SetFloat("_SamplesCount", Settings.SamplesCount);

            //keyword _ComplexNormals
            if(Settings.UseComplexNormalsReconstruction)
            {
                if (!SSAOMaterial.IsKeywordEnabled("COMPLEX_NORMALS"))
                {
                    SSAOMaterial.EnableKeyword("COMPLEX_NORMALS");
                }
            }
            else
            {
                if (SSAOMaterial.IsKeywordEnabled("COMPLEX_NORMALS"))
                {
                    SSAOMaterial.DisableKeyword("COMPLEX_NORMALS");
                }
            } 
        }

        private static Vector2 BlurV = new Vector2(0, 1);
        private static Vector2 BlurH = new Vector2(1, 0);
        public override void Execute(ScriptableRenderContext context, ref UnityEngine.Rendering.Universal.RenderingData renderingData)
        {
            buffer.Clear();
            Blit(buffer, targetColorTex, FirstScreenCopy);

            if (Settings.EffectEnabled)
            {
                //update uniforms before rendering
                UpdateUniforms(renderingData);

                //render ssao
                Blit(buffer, FirstScreenCopy, SSAOTarget, SSAOMaterial, 0);

                //blur ssao texture
                Blit(buffer, SSAOTarget, SSAOTargetH);

                SSAOBlurMaterial.SetFloat("BlurDepthFalloff", Settings.BlurDepthFalloff);
                SSAOBlurMaterial.SetFloat("BlurRadius", Settings.BlurRadius); 

                //4x4 per pass biliteral blur
                if (Settings.BlurEnabled)
                {
                    for (int i = 0; i < Settings.BlurIterations; i++)
                    {
                        //blur V
                        buffer.SetGlobalVector("_Dir", BlurV);
                        Blit(buffer, SSAOTargetH, SSAOTargetV, SSAOBlurMaterial);

                        //blur H
                        buffer.SetGlobalVector("_Dir", BlurH);
                        Blit(buffer, SSAOTargetV, SSAOTargetH, SSAOBlurMaterial);
                    }
                }
                 
                //connect AO texture with scene tex
                if (!Settings.ShowAOTextureOnly)
                {
                    buffer.SetGlobalTexture("_SSAO", SSAOTargetH);
                    Blit(buffer, FirstScreenCopy, targetColorTex, SSAOCombineMaterial); 
                }
                else
                {
                    Blit(buffer, SSAOTargetH, targetColorTex);
                }
            }
            else
                Blit(buffer, FirstScreenCopy, targetColorTex);

            context.ExecuteCommandBuffer(buffer);
        }

        /// Cleanup any allocated resources that were created during the execution of this render pass.
        public override void FrameCleanup(CommandBuffer cmd)
        {
            foreach (int textureID in TRT)
                cmd.ReleaseTemporaryRT(textureID);

            TRT.Clear();
        } 
    }

    CustomRenderPass m_ScriptablePass;
      
    public BasicSSAOSettings Settings;

    public Material SSAOMaterial;
    public Material SSAOBlurMaterial;
    public Material SSAOCombineMaterial;

    public UnityEngine.Rendering.Universal.RenderPassEvent renderQueue = UnityEngine.Rendering.Universal.RenderPassEvent.BeforeRenderingPostProcessing;
       
    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass();
         
        m_ScriptablePass.renderPassEvent = renderQueue;
    }

    //Function used to generate kernels for ssao calculations, for now there is static array decalred inside in shader, no need to change it over time
    /*private List<Vector4> MakeKernels(int numSamples)
    {
        List<Vector3> ssaoKernel = new List<Vector3>();

        for (int i = 0; i < numSamples; i++)
        {
            float x = (float)(Random.value * 2.0 - 1.0);
            float y = (float)(Random.value * 2.0 - 1.0);
            float z = (float)(Random.value);

            Vector3 sample = (new Vector3(x, y, z)).normalized;

            float ratio = i / (float)numSamples;
            float scale = Mathf.Lerp(0.1f, 1.0f, ratio * ratio);
            sample *= scale;
            ssaoKernel.Add(sample);
        }

        List<Vector4> outList = new List<Vector4>();
        string kernels = "static float3 kernels[64] = {";
        foreach (Vector3 kernel in ssaoKernel)
        {
            outList.Add(new Vector4(kernel.x, kernel.y, kernel.z, 1));
            kernels += "\n float3(" + kernel.x + ", " + kernel.y + ", " + kernel.z + "),"; 
        }
        Debug.Log(kernels);
        return outList;
    }
    */


    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(UnityEngine.Rendering.Universal.ScriptableRenderer renderer, ref UnityEngine.Rendering.Universal.RenderingData renderingData)
    {
        //check if we can create render pass
        if (SSAOBlurMaterial == null || SSAOMaterial == null || SSAOBlurMaterial == null)
        {
            Debug.LogError("(Basic SSAO) Some material isntances are null please attach them!");
            return;
        } 
        if (Settings == null)
        {
            Debug.LogError("(Basic SSAO) Please attach Basic SSAO Settings!");
            return;
        }

        //pass targets
        m_ScriptablePass.targetColorTex = renderer.cameraColorTarget;
        m_ScriptablePass.depthTex = renderer.cameraDepthTarget;

        //pass materials
        m_ScriptablePass.SSAOMaterial = SSAOMaterial;
        m_ScriptablePass.SSAOBlurMaterial = SSAOBlurMaterial;
        m_ScriptablePass.SSAOCombineMaterial = SSAOCombineMaterial;
        
        //pass settings
        m_ScriptablePass.Settings = Settings;

        renderer.EnqueuePass(m_ScriptablePass);
    }
}


