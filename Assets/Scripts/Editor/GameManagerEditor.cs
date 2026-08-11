using UnityEditor;

[CustomEditor(typeof(GameManagerBehavior))]
public class GameManagerEditor : Editor
{
    private Editor editorInstance;

    private void OnEnable()
    {
        editorInstance = null;
    }

    public override void OnInspectorGUI()
    {
        GameManagerBehavior gameManager = (GameManagerBehavior)target;
        if (editorInstance == null)
            editorInstance = CreateEditor(gameManager.gameSettings);

        base.OnInspectorGUI();

        editorInstance.DrawDefaultInspector();
    }
}
