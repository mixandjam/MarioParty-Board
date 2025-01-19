using UnityEngine;

public class EyeHandler : MonoBehaviour
{
    [SerializeField] private Renderer jammoRenderer;
    private Material eyeMaterialInstance;

    private void Start()
    {
        eyeMaterialInstance = new Material(jammoRenderer.sharedMaterials[1]);

        Material[] materials = jammoRenderer.materials;
        materials[1] = eyeMaterialInstance;
        jammoRenderer.materials = materials;
    }

    public void ModifyEyes(string type)
    {
        if (eyeMaterialInstance == null)
        {
            Debug.LogError("Eye material instance not initialized!");
            return;
        }

        Vector2 offset = Vector2.zero;
        switch (type)
        {
            case "happy":
                offset = new Vector2(.33f, 0);
                break;
            case "sad":
                offset = new Vector2(0, -.33f);
                break;
            case "default":
                offset = Vector2.zero;
                break;
        }

        eyeMaterialInstance.SetTextureOffset("_MainTex", offset);
    }
}
