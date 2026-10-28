// Team Apple: fixes an ALREADY EXISTING empty OfficePhone GameObject.
// Compatible with Unity 6 / Unity 2022+. Put THIS file in Assets/Editor.
// If an older TeamAppleOfficePhoneCreator.cs exists, REPLACE it (do not keep both).
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TeamAppleOfficePhoneCreator
{
    [MenuItem("Tools/Team Apple/Make OfficePhone Visible")]
    public static void FromToolsMenu() { MakePhoneVisible(); }

    // Alternative menu: the user's Mac layout may not show a Tools menu yet.
    [MenuItem("GameObject/Team Apple/Make OfficePhone Visible", false, 12)]
    public static void FromGameObjectMenu() { MakePhoneVisible(); }

    // Existing menu name now also upgrades an empty OfficePhone instead of exiting early.
    [MenuItem("Tools/Team Apple/Create Office Phone")]
    public static void CreateOrRepair() { MakePhoneVisible(); }

    private static void MakePhoneVisible()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Office Phone", "Stop Play mode first, then try again.", "OK");
            return;
        }

        Sprite phoneSprite = LoadPhoneSprite();
        if (phoneSprite == null)
        {
            EditorUtility.DisplayDialog("Phone image missing",
                "Copy Assets/OfficePhoneAssets/office_phone.png into your Unity project, then use GameObject > Team Apple > Make OfficePhone Visible again.", "OK");
            return;
        }

        GameObject phone = null;
        if (Selection.activeGameObject != null && Selection.activeGameObject.name == "OfficePhone" &&
            Selection.activeGameObject.scene.IsValid())
            phone = Selection.activeGameObject;
        if (phone == null)
            phone = GameObject.Find("OfficePhone");

        bool created = phone == null;
        if (created)
        {
            phone = new GameObject("OfficePhone");
            Undo.RegisterCreatedObjectUndo(phone, "Create OfficePhone");
        }
        else
            Undo.RegisterFullObjectHierarchyUndo(phone, "Make OfficePhone Visible");

        // Do not create an extra phone. Add a visible SpriteRenderer to the EXISTING one.
        SpriteRenderer spriteRenderer = phone.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = Undo.AddComponent<SpriteRenderer>(phone);
        spriteRenderer.sprite = phoneSprite;
        spriteRenderer.color = Color.white;
        spriteRenderer.enabled = true;
        spriteRenderer.sortingLayerName = "Default";
        spriteRenderer.sortingOrder = 25;

        AudioSource audio = phone.GetComponent<AudioSource>();
        if (audio == null) audio = Undo.AddComponent<AudioSource>(phone);
        audio.clip = LoadPhoneAudio();
        audio.playOnAwake = false;
        audio.loop = true;
        audio.spatialBlend = 0f;
        audio.volume = 0.8f;
        audio.enabled = true;

        RingingPhone ring = phone.GetComponent<RingingPhone>();
        if (ring == null) ring = Undo.AddComponent<RingingPhone>(phone);
        ring.ringAudio = audio;
        ring.enabled = true;
        ring.useDistance = 2.0f;
        ring.hearingDistance = 8.0f;
        ring.distractionDuration = 6.0f;
        ring.cooldown = 10.0f;
        ring.distractionPoints = 5;

        GameObject player = null;
        try { player = GameObject.FindGameObjectWithTag("Player"); }
        catch (UnityException) { }
        if (player == null) player = GameObject.Find("Player");
        if (player != null)
        {
            ring.player = player.transform;
            // An empty newly created phone at (0,0,0) is often invisible / away from the player.
            // If a user has already positioned it, preserve that chosen position.
            if (created || (phone.transform.position == Vector3.zero))
                phone.transform.position = new Vector3(player.transform.position.x + 1.5f,
                    player.transform.position.y + 0.4f, player.transform.position.z);
        }

        EditorUtility.SetDirty(phone);
        EditorUtility.SetDirty(spriteRenderer);
        EditorUtility.SetDirty(audio);
        EditorUtility.SetDirty(ring);
        if (phone.scene.IsValid()) EditorSceneManager.MarkSceneDirty(phone.scene);
        Selection.activeGameObject = phone;
        EditorGUIUtility.PingObject(phone);

        string details = (created ? "Created OfficePhone" : "Updated your existing OfficePhone") +
            " with a phone sprite, Audio Source, and RingingPhone script.\n\n" +
            "In Scene view select OfficePhone and press F to focus.\n" +
            "Move the phone to a clear floor spot, save (Cmd+S), and press Play.\n\n" +
            "Remember: enemy prefabs need PhoneEnemyResponder to investigate.";
        if (audio.clip == null) details += "\n\nWarning: phone_ring.wav was not found in Assets/OfficePhoneAssets.";
        if (player == null) details += "\n\nWarning: No Player was found; assign Player in the RingingPhone inspector.";
        Debug.Log(details, phone);
        EditorUtility.DisplayDialog("Office Phone Is Ready", details, "OK");
    }

    private static Sprite LoadPhoneSprite()
    {
        string path = FindFile("office_phone", "t:Texture2D", "Assets/OfficePhoneAssets");
        if (string.IsNullOrEmpty(path)) return null;
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            bool changed = importer.textureType != TextureImporterType.Sprite ||
                           importer.spriteImportMode != SpriteImportMode.Single ||
                           importer.spritePixelsPerUnit != 128 ||
                           importer.filterMode != FilterMode.Point ||
                           importer.mipmapEnabled;
            if (changed)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 128;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static AudioClip LoadPhoneAudio()
    {
        string path = FindFile("phone_ring", "t:AudioClip", "Assets/OfficePhoneAssets");
        return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }

    private static string FindFile(string baseName, string assetType, string folder)
    {
        string[] matches = AssetDatabase.IsValidFolder(folder)
            ? AssetDatabase.FindAssets(baseName + " " + assetType, new[] { folder })
            : new string[0];
        if (matches.Length == 0) matches = AssetDatabase.FindAssets(baseName + " " + assetType);
        foreach (string guid in matches)
        {
            string candidate = AssetDatabase.GUIDToAssetPath(guid);
            if (String.Equals(System.IO.Path.GetFileNameWithoutExtension(candidate), baseName, StringComparison.OrdinalIgnoreCase))
                return candidate;
        }
        return matches.Length > 0 ? AssetDatabase.GUIDToAssetPath(matches[0]) : null;
    }
}
