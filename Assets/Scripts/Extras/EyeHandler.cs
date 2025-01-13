using UnityEngine;

public class EyeHandler : MonoBehaviour
{

    [SerializeField] private Renderer jammoRenderer;
    public void ModifyEyes(string type)
    {
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

        jammoRenderer.sharedMaterials[1].SetTextureOffset("_MainTex", offset);
    }


}
