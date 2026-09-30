using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace FarSight
{
    [Serializable]
    public class Settings
    {
        public bool enabled = true;
        public float treeRangeMult = 1.69f;
        public bool dumpInfo = true;
        // A Unity KeyCode name, optionally with modifiers: "F11", "Ctrl+F11", "Ctrl+Shift+F7". Modifiers must match exactly.
        public string toggleKey = "Ctrl+F11";
    }

    public static class Main
    {
        internal static Settings S;
        internal static string Dir;
        internal static string SavePath => Path.Combine(Dir, "FarSight.user.json");

        public static void Init(string directory, string settingsJSON)
        {
            Dir = directory;
            S = TryParse(File.Exists(SavePath) ? File.ReadAllText(SavePath) : null)
                ?? TryParse(settingsJSON)
                ?? new Settings();

            var go = new GameObject("FarSight");
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.AddComponent<Applier>();
        }

        static Settings TryParse(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            try { return JsonUtility.FromJson<Settings>(json); } catch { return null; }
        }

        internal static void Save() => File.WriteAllText(SavePath, JsonUtility.ToJson(S, true));

        internal static void Log(string msg) =>
            File.AppendAllText(Path.Combine(Dir, "FarSight.log"), msg + "\n");
    }

    public class Applier : MonoBehaviour
    {
        static Type rtType;
        static FieldInfo rangeField;

        UnityEngine.Object[] trees = new UnityEngine.Object[0];
        readonly Dictionary<int, float> vanilla = new Dictionary<int, float>();
        KeyCode key = KeyCode.F11;
        bool needCtrl = true, needShift, needAlt;
        bool show;
        Rect win = new Rect(20, 20, 280, 0);
        float fps;

        void Start()
        {
            ParseKey(Main.S.toggleKey);

            rtType = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType("BattleTech.Rendering.Trees.RenderTrees", false))
                .FirstOrDefault(t => t != null);
            rangeField = rtType?.GetField("distanceRange", BindingFlags.Instance | BindingFlags.Public);

            if (Main.S.dumpInfo)
                Main.Log($"RenderTrees type={(rtType != null)} distanceRange field={(rangeField != null)}");

            StartCoroutine(Loop());
        }

        void ParseKey(string spec)
        {
            bool ctrl = false, shift = false, alt = false;
            KeyCode k = KeyCode.None;
            foreach (var part in (spec ?? "").Split('+'))
            {
                var p = part.Trim();
                if (p.Equals("ctrl", StringComparison.OrdinalIgnoreCase) || p.Equals("control", StringComparison.OrdinalIgnoreCase)) ctrl = true;
                else if (p.Equals("shift", StringComparison.OrdinalIgnoreCase)) shift = true;
                else if (p.Equals("alt", StringComparison.OrdinalIgnoreCase)) alt = true;
                else { try { k = (KeyCode)Enum.Parse(typeof(KeyCode), p, true); } catch { } }
            }
            if (k == KeyCode.None) { Main.Log("Unknown toggleKey '" + spec + "', using Ctrl+F11"); k = KeyCode.F11; ctrl = true; shift = alt = false; }
            key = k; needCtrl = ctrl; needShift = shift; needAlt = alt;
        }

        bool ChordPressed()
        {
            if (!Input.GetKeyDown(key)) return false;
            bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            return ctrl == needCtrl && shift == needShift && alt == needAlt;
        }

        void Update()
        {
            if (ChordPressed()) show = !show;
            if (Time.unscaledDeltaTime > 0)
                fps = Mathf.Lerp(fps, 1f / Time.unscaledDeltaTime, 0.05f);
        }

        IEnumerator Loop()
        {
            var wait = new WaitForSeconds(2f);
            while (true) { Apply(); yield return wait; }
        }

        void Apply()
        {
            if (rangeField == null) return;

            // RenderTrees lives on the combat camera; re-find it when the scene changes
            if (trees.Length == 0 || trees.Any(t => t == null))
                trees = FindObjectsOfType(rtType);

            var s = Main.S;
            foreach (var t in trees)
            {
                int id = t.GetInstanceID();
                if (!vanilla.TryGetValue(id, out var v))
                {
                    v = (float)rangeField.GetValue(t);
                    vanilla[id] = v;
                    if (s.dumpInfo) Main.Log($"RenderTrees on {t.name}: vanilla distanceRange={v}");
                }

                float want = v * (s.enabled ? s.treeRangeMult : 1f);
                if ((float)rangeField.GetValue(t) != want) rangeField.SetValue(t, want);
            }
        }

        void OnGUI()
        {
            if (show) win = GUILayout.Window(0x4641, win, DrawWindow, "FarSight");
        }

        void DrawWindow(int id)
        {
            var s = Main.S;
            GUILayout.Label($"FPS: {fps:0}   (1.0 = vanilla)");
            s.enabled = GUILayout.Toggle(s.enabled, " Enabled (off = vanilla)");
            GUILayout.Label($"Shrub/tree range: x{s.treeRangeMult:0.00}");
            s.treeRangeMult = GUILayout.HorizontalSlider(s.treeRangeMult, 1f, 4f);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save")) Main.Save();
            if (GUILayout.Button("Close")) show = false;
            GUILayout.EndHorizontal();

            if (GUI.changed) Apply();
            GUI.DragWindow();
        }
    }
}