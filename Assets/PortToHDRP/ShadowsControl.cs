using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[ExecuteAlways]
public class ShadowsControl : MonoBehaviour
{
    public static int LastMask = ~0;

    public Light target;

    HDAdditionalLightData m_LightData;
    uint m_UpdateTick;
    Camera m_PreviousCamera;

    void Reset() => target = GetComponent<Light>();

    void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += RenderPipelineManagerOnBeginCameraRendering;

        Setup();
    }

    void OnDisable()
    {
        Cleanup();

        RenderPipelineManager.beginCameraRendering -= RenderPipelineManagerOnBeginCameraRendering;
        LastMask = ~0;
    }

    void Setup()
    {
        if (target != null)
        {
            m_LightData = target.GetComponent<HDAdditionalLightData>();
        }

        if (m_LightData != null)
        {
            if (m_LightData.shadowUpdateMode != ShadowUpdateMode.OnDemand)
            {
                m_LightData.shadowUpdateMode = ShadowUpdateMode.OnDemand;
            }
        }
    }

    void Cleanup()
    {
        if (m_LightData != null)
        {
            if (m_LightData.shadowUpdateMode != ShadowUpdateMode.EveryFrame)
            {
                m_LightData.shadowUpdateMode = ShadowUpdateMode.EveryFrame;
            }
        }
    }

    void Tick(Camera camera)
    {
        if (m_LightData == null)
        {
            return;
        }

        if (FrameDebugger.enabled)
        {
            // Render all cascades when frame debugger is enabled since it's impossible to use when events jump around.
            m_LightData.RequestShadowMapRendering();
        }

        var tick = m_UpdateTick++;

        if (camera != m_PreviousCamera)
        {
            //Debug.Log($"Tick {tick}, full update because camera {camera} != {m_PreviousCamera}");
            m_LightData.RequestShadowMapRendering();
            m_PreviousCamera = camera;
            LastMask = ~0;
        }
        else
        {
            // Update first two cascades + one more in scene view (often larger camera rotations)

            m_LightData.RequestSubShadowMapRendering(0);
            m_LightData.RequestSubShadowMapRendering(1);
            var t = (tick & 1) == 0 ? 2 : 3;
            m_LightData.RequestSubShadowMapRendering(t);
            //Debug.Log($"Tick {tick}, updating 0+1+{t}");
            LastMask = 1 | 2 | ((tick & 1) == 0 ? 4 : 8);
        }
    }

    void RenderPipelineManagerOnBeginCameraRendering(ScriptableRenderContext _, Camera camera)
    {
        Tick(camera);
    }
}
