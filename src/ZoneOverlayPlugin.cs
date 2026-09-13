using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using EFT.Interactive;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace DangerZoneOverlay;

[BepInPlugin("com.local.dangerzoneoverlay", "Danger Zone Overlay", "1.0.0")]
public sealed class ZoneOverlayPlugin : BaseUnityPlugin
{
    private readonly List<ZoneVisual> _visuals = new();
    private bool _sceneScanned;
    private float _nextSceneScan;
    private ConfigEntry<KeyboardShortcut> _toggle = null!;
    private ConfigEntry<KeyboardShortcut> _rescan = null!;
    private ConfigEntry<bool> _showMines = null!;
    private ConfigEntry<bool> _showSniper = null!;
    private ConfigEntry<bool> _showBorders = null!;
    private ConfigEntry<float> _mineOpacity = null!;
    private ConfigEntry<float> _sniperOpacity = null!;
    private ConfigEntry<float> _borderOpacity = null!;
    private bool _visible = true;
    private static Material? _mineMaterial;
    private static Material? _sniperMaterial;
    private static Material? _borderMaterial;

    private void Awake()
    {
        _toggle = Config.Bind("Settings", "Toggle overlay", new KeyboardShortcut(KeyCode.F8), "Toggle the zone overlay on/off.");
        _rescan = Config.Bind("Settings", "Rescan zones", new KeyboardShortcut(KeyCode.F9), "Force re-scan of the current scene for mine / sniper / border zones.");
        _showMines = Config.Bind("Settings", "Show mine zones", true, "Draw Minefield / MineDirectionalColliders zones.");
        _showSniper = Config.Bind("Settings", "Show sniper zones", true, "Draw SniperFiringZone zones.");
        _showBorders = Config.Bind("Settings", "Show border zones", true, "Draw map Xxx_LevelBorders zones.");
        _mineOpacity = Config.Bind("Settings", "Mine opacity", 0.5f, new ConfigDescription("Opacity of mine zone boxes.", new AcceptableValueRange<float>(0f, 1f)));
        _sniperOpacity = Config.Bind("Settings", "Sniper opacity", 0.5f, new ConfigDescription("Opacity of sniper zone boxes.", new AcceptableValueRange<float>(0f, 1f)));
        _borderOpacity = Config.Bind("Settings", "Border opacity", 0.18f, new ConfigDescription("Opacity of map border volumes.", new AcceptableValueRange<float>(0f, 1f)));
        _showMines.SettingChanged += OnZoneSettingChanged;
        _showSniper.SettingChanged += OnZoneSettingChanged;
        _showBorders.SettingChanged += OnZoneSettingChanged;
        _mineOpacity.SettingChanged += OnZoneSettingChanged;
        _sniperOpacity.SettingChanged += OnZoneSettingChanged;
        _borderOpacity.SettingChanged += OnZoneSettingChanged;
        _visible = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        DestroyGameZoneVisuals();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        DestroyGameZoneVisuals();
        _sceneScanned = false;
        _nextSceneScan = Time.unscaledTime + 2f;
        Logger.LogInfo($"Scene loaded: {scene.name}; trigger scan scheduled.");
    }

    private void Update()
    {
        if (_toggle.Value.IsDown())
        {
            _visible = !_visible;
            ApplyZoneSettings();
        }

        if (_rescan.Value.IsDown())
        {
            BuildGameZoneVisuals();
        }

        if (!_sceneScanned && Time.unscaledTime >= _nextSceneScan && Camera.main != null)
        {
            _sceneScanned = true;
            BuildGameZoneVisuals();
        }
    }

    private void OnZoneSettingChanged(object sender, EventArgs e)
    {
        ApplyZoneSettings();
    }

    private const string VisualObjectName = "TZOVisual";

    private void BuildGameZoneVisuals()
    {
        DestroyGameZoneVisuals();
        _sceneScanned = true;

        for (var i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded)
            {
                continue;
            }

            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name.Contains("LevelBorders"))
                {
                    CreateBorderVisuals(root.transform);
                }

                AddZoneVisuals(root.transform);
            }
        }

        ApplyZoneSettings();
        Logger.LogInfo($"ShowLandMines visuals: {_visuals.Count} objects created.");
    }

    private void AddZoneVisuals(Transform transform)
    {
        if (transform.name == VisualObjectName)
        {
            return;
        }

        var collider = transform.GetComponent<BoxCollider>();
        if (collider != null)
        {
            if (transform.GetComponent<Minefield>() != null)
            {
                AddPrimitive(PrimitiveType.Cube, transform, collider.center, Quaternion.identity, collider.size, ZoneKind.Mine);
            }

            if (transform.GetComponent<SniperFiringZone>() != null)
            {
                AddPrimitive(PrimitiveType.Cube, transform, collider.center, Quaternion.identity, collider.size, ZoneKind.Sniper);
            }

            if (transform.GetComponent<MineDirectionalColliders>() != null)
            {
                AddPrimitive(PrimitiveType.Cube, transform, collider.center, Quaternion.identity, collider.size, ZoneKind.Mine);
            }
        }

        foreach (Transform child in transform)
        {
            AddZoneVisuals(child);
        }
    }

    private void CreateBorderVisuals(Transform border)
    {
        foreach (Transform child in border)
        {
            if (child.name == VisualObjectName)
            {
                continue;
            }

            var box = child.GetComponent<BoxCollider>();
            var sphere = child.GetComponent<SphereCollider>();
            var capsule = child.GetComponent<CapsuleCollider>();
            if (box != null)
            {
                AddPrimitive(PrimitiveType.Cube, child, box.center, Quaternion.identity, box.size, ZoneKind.Border);
            }
            else if (sphere != null)
            {
                AddPrimitive(PrimitiveType.Sphere, child, sphere.center, Quaternion.identity, Vector3.one * (sphere.radius * 2f), ZoneKind.Border);
            }
            else if (capsule != null)
            {
                var scale = new Vector3(capsule.radius * 2f, capsule.height / 2f, capsule.radius * 2f);
                var rotation = capsule.direction switch
                {
                    0 => Quaternion.Euler(0f, 0f, 90f),
                    2 => Quaternion.Euler(90f, 0f, 0f),
                    _ => Quaternion.identity
                };
                AddPrimitive(PrimitiveType.Capsule, child, capsule.center, rotation, scale, ZoneKind.Border);
            }

            CreateBorderVisuals(child);
        }
    }

    private void AddPrimitive(PrimitiveType type, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, ZoneKind kind)
    {
        var gameObject = GameObject.CreatePrimitive(type);
        gameObject.name = VisualObjectName;
        UnityEngine.Object.Destroy(gameObject.GetComponent<Collider>());
        gameObject.transform.SetParent(parent, false);
        gameObject.transform.localPosition = localPosition;
        gameObject.transform.localRotation = localRotation;
        gameObject.transform.localScale = localScale;
        gameObject.GetComponent<MeshRenderer>().material = MaterialFor(kind);
        _visuals.Add(new ZoneVisual(gameObject, kind));
    }

    private void DestroyGameZoneVisuals()
    {
        foreach (var visual in _visuals)
        {
            if (visual.GameObject != null)
            {
                UnityEngine.Object.Destroy(visual.GameObject);
            }
        }

        _visuals.Clear();
    }

    private void ApplyZoneSettings()
    {
        var mineAlpha = _mineOpacity.Value;
        var sniperAlpha = _sniperOpacity.Value;
        var borderAlpha = _borderOpacity.Value;
        _mineMaterial ??= CreateZoneMaterial(MineBaseColor, mineAlpha);
        _sniperMaterial ??= CreateZoneMaterial(SniperBaseColor, sniperAlpha);
        _borderMaterial ??= CreateZoneMaterial(BorderBaseColor, borderAlpha);
        _mineMaterial.color = WithAlpha(MineBaseColor, mineAlpha);
        _sniperMaterial.color = WithAlpha(SniperBaseColor, sniperAlpha);
        _borderMaterial.color = WithAlpha(BorderBaseColor, borderAlpha);

        foreach (var visual in _visuals)
        {
            var gameObject = visual.GameObject;
            if (gameObject == null)
            {
                continue;
            }

            var renderer = gameObject.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                continue;
            }

            var alpha = OpacityFor(visual.Kind);
            renderer.enabled = _visible && EnabledFor(visual.Kind) && alpha > 0.001f;
            renderer.material.color = WithAlpha(BaseColorFor(visual.Kind), alpha);
        }
    }

    private bool EnabledFor(ZoneKind kind)
    {
        return kind switch
        {
            ZoneKind.Mine => _showMines.Value,
            ZoneKind.Sniper => _showSniper.Value,
            _ => _showBorders.Value
        };
    }

    private float OpacityFor(ZoneKind kind)
    {
        return kind switch
        {
            ZoneKind.Mine => _mineOpacity.Value,
            ZoneKind.Sniper => _sniperOpacity.Value,
            _ => _borderOpacity.Value
        };
    }

    private static Color BaseColorFor(ZoneKind kind)
    {
        return kind switch
        {
            ZoneKind.Mine => MineBaseColor,
            ZoneKind.Sniper => SniperBaseColor,
            _ => BorderBaseColor
        };
    }

    private static Material MaterialFor(ZoneKind kind)
    {
        return kind switch
        {
            ZoneKind.Mine => GetMineMaterial(),
            ZoneKind.Sniper => GetSniperMaterial(),
            _ => GetBorderMaterial()
        };
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, alpha);
    }

    private static readonly Color MineBaseColor = new Color(1f, 0f, 0f);
    private static readonly Color SniperBaseColor = new Color(0f, 0f, 1f);
    private static readonly Color BorderBaseColor = new Color(0.78f, 0.8f, 0.82f);

    private static Material GetMineMaterial()
    {
        return _mineMaterial ??= CreateZoneMaterial(MineBaseColor, 0.5f);
    }

    private static Material GetSniperMaterial()
    {
        return _sniperMaterial ??= CreateZoneMaterial(SniperBaseColor, 0.5f);
    }

    private static Material GetBorderMaterial()
    {
        return _borderMaterial ??= CreateZoneMaterial(BorderBaseColor, 0.18f);
    }

    private static Material CreateZoneMaterial(Color color, float alpha)
    {
        var material = new Material(Shader.Find("Standard"))
        {
            color = WithAlpha(color, alpha),
            hideFlags = HideFlags.HideAndDontSave
        };
        material.SetFloat("_Mode", 3f);
        material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.SetColor("_EmissionColor", new Color(color.r * 0.55f, color.g * 0.55f, color.b * 0.55f, 0f));
        material.EnableKeyword("_EMISSION");
        material.renderQueue = 3000;
        return material;
    }

    private enum ZoneKind
    {
        Mine,
        Sniper,
        Border
    }

    private sealed class ZoneVisual
    {
        public ZoneVisual(GameObject gameObject, ZoneKind kind)
        {
            GameObject = gameObject;
            Kind = kind;
        }

        public GameObject GameObject { get; }
        public ZoneKind Kind { get; }
    }

}
