using System;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WastelandForge.App
{
    public sealed class WastelandForgeAppShell : MonoBehaviour
    {
        [SerializeField] private Sprite panelSprite;
        [SerializeField] private Sprite outlineSprite;
        [SerializeField] private TMP_FontAsset regularFont;
        [SerializeField] private TMP_FontAsset boldFont;

        private readonly Color background = Hex("0E141B");
        private readonly Color surface = Hex("151F29");
        private readonly Color surfaceAlt = Hex("1D2A35");
        private readonly Color accent = Hex("FFAF00");
        private readonly Color text = Hex("F5F7FA");
        private readonly Color muted = Hex("AAB3BD");
        private readonly Color danger = Hex("FF4B4B");
        private readonly Color success = Hex("4DDB8C");

        private TMP_InputField projectRootInput;
        private TMP_Text backendStatusText;
        private TMP_Text capabilityStatusText;
        private TMP_Text validationStatusText;
        private TMP_Text reportText;
        private TMP_Text logText;
        private ForgeCommandRunner runner;

        public void ConfigureAssets(Sprite heatPanelSprite, Sprite heatOutlineSprite, TMP_FontAsset heatRegularFont, TMP_FontAsset heatBoldFont)
        {
            panelSprite = heatPanelSprite;
            outlineSprite = heatOutlineSprite;
            regularFont = heatRegularFont;
            boldFont = heatBoldFont;
        }

        private void Awake()
        {
            runner = new ForgeCommandRunner();
            EnsureEventSystem();
            BuildInterface();
        }

        private async void Start()
        {
            await RunVersionAsync();
        }

        private void BuildInterface()
        {
            var canvasObject = new GameObject("WastelandForge Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.matchWidthOrHeight = 0.5f;

            var root = CreateRect("App Root", canvasObject.transform);
            Stretch(root);
            var rootImage = root.gameObject.AddComponent<Image>();
            rootImage.color = background;

            var shell = CreateRect("Shell", root);
            Stretch(shell, 26, 26, -26, -26);
            var shellLayout = shell.gameObject.AddComponent<HorizontalLayoutGroup>();
            shellLayout.spacing = 18;
            shellLayout.padding = new RectOffset(0, 0, 0, 0);
            shellLayout.childForceExpandHeight = true;
            shellLayout.childForceExpandWidth = false;

            BuildRail(shell);
            BuildMain(shell);
        }

        private void BuildRail(Transform parent)
        {
            var rail = CreateSurface("Navigation Rail", parent, surface);
            rail.GetComponent<LayoutElement>().preferredWidth = 290;

            var layout = rail.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 26, 26);
            layout.spacing = 14;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            CreateText("WASTELANDFORGE", rail, 18, FontStyles.UpperCase, accent, boldFont);
            CreateText("Forge Workbench", rail, 34, FontStyles.Normal, text, boldFont).lineSpacing = -14;
            CreateText("Premium shell over deterministic Forge backend", rail, 17, FontStyles.Normal, muted, regularFont);

            AddSpacer(rail, 20);
            CreateRailButton(rail, "Project", "Select and validate a Forge project");
            CreateRailButton(rail, "Doctor", "Capability and environment dashboard");
            CreateRailButton(rail, "Reports", "Validation output and machine-readable JSON");
            CreateRailButton(rail, "Logs", "Backend command transcript");
            AddFlexibleSpacer(rail);

            backendStatusText = CreateText("Backend: checking", rail, 16, FontStyles.Normal, muted, regularFont);
            CreateText("Heat UI 1.1.8 local import", rail, 15, FontStyles.Normal, accent, regularFont);
        }

        private void BuildMain(Transform parent)
        {
            var main = CreateRect("Main Content", parent);
            main.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

            var layout = main.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 18;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var topBar = CreateSurface("Top Bar", main, surface);
            topBar.GetComponent<LayoutElement>().preferredHeight = 92;
            var topLayout = topBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            topLayout.padding = new RectOffset(24, 24, 16, 16);
            topLayout.spacing = 16;
            topLayout.childAlignment = TextAnchor.MiddleLeft;
            topLayout.childForceExpandWidth = false;

            var titleBlock = CreateRect("Title Block", topBar);
            titleBlock.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            var titleLayout = titleBlock.gameObject.AddComponent<VerticalLayoutGroup>();
            titleLayout.spacing = 2;
            titleLayout.childForceExpandHeight = false;
            CreateText("WastelandForge.exe", titleBlock, 30, FontStyles.Normal, text, boldFont);
            CreateText("Project selector, Doctor dashboard, validation reports, and logs", titleBlock, 16, FontStyles.Normal, muted, regularFont);

            CreateActionButton(topBar, "Version", async () => await RunVersionAsync(), accent);
            CreateActionButton(topBar, "Capabilities", async () => await RunCapabilitiesAsync(), surfaceAlt);
            CreateActionButton(topBar, "Validate", async () => await RunValidateAsync(), danger);

            var body = CreateRect("Body", main);
            body.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1;
            var bodyLayout = body.gameObject.AddComponent<HorizontalLayoutGroup>();
            bodyLayout.spacing = 18;
            bodyLayout.childForceExpandHeight = true;

            BuildProjectPanel(body);
            BuildReportPanel(body);
        }

        private void BuildProjectPanel(Transform parent)
        {
            var column = CreateRect("Left Work Column", parent);
            column.gameObject.AddComponent<LayoutElement>().preferredWidth = 520;
            var layout = column.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 18;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var projectPanel = CreateSurface("Project Selector", column, surface);
            projectPanel.GetComponent<LayoutElement>().preferredHeight = 250;
            var projectLayout = projectPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            projectLayout.padding = new RectOffset(22, 22, 20, 20);
            projectLayout.spacing = 14;
            CreateText("Project Selector", projectPanel, 24, FontStyles.Normal, text, boldFont);
            CreateText("Choose a Forge project root. The app shell keeps canonical truth in the project files and runs forge.exe for validation.", projectPanel, 16, FontStyles.Normal, muted, regularFont);
            projectRootInput = CreateInput(projectPanel, "D:\\documents\\GitHub\\WasteLandForge\\fixtures\\projects\\ExampleMod");
            CreateActionButton(projectPanel, "Validate Project", async () => await RunValidateAsync(), accent);

            var doctorPanel = CreateSurface("Doctor Dashboard", column, surface);
            doctorPanel.GetComponent<LayoutElement>().preferredHeight = 260;
            var doctorLayout = doctorPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            doctorLayout.padding = new RectOffset(22, 22, 20, 20);
            doctorLayout.spacing = 12;
            CreateText("Doctor / Capability Dashboard", doctorPanel, 24, FontStyles.Normal, text, boldFont);
            capabilityStatusText = CreateText("Capabilities: waiting for scan", doctorPanel, 17, FontStyles.Normal, muted, regularFont);
            validationStatusText = CreateText("Validation: no project run yet", doctorPanel, 17, FontStyles.Normal, muted, regularFont);
            CreateMetricRow(doctorPanel, "Backend", "forge.exe", "local worker");
            CreateMetricRow(doctorPanel, "Output", "JSON", "machine-readable");
            CreateMetricRow(doctorPanel, "Mode", "Offline", "AI optional");

            var logPanel = CreateSurface("Advanced Logs", column, surface);
            logPanel.GetComponent<LayoutElement>().flexibleHeight = 1;
            var logLayout = logPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            logLayout.padding = new RectOffset(22, 22, 20, 20);
            logLayout.spacing = 12;
            CreateText("Advanced Logs", logPanel, 24, FontStyles.Normal, text, boldFont);
            logText = CreateText("Waiting for backend command...", logPanel, 15, FontStyles.Normal, muted, regularFont);
        }

        private void BuildReportPanel(Transform parent)
        {
            var reportPanel = CreateSurface("Validation Report", parent, surface);
            reportPanel.GetComponent<LayoutElement>().flexibleWidth = 1;
            var layout = reportPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 22, 22);
            layout.spacing = 14;
            CreateText("Validation Report View", reportPanel, 28, FontStyles.Normal, text, boldFont);
            CreateText("Raw JSON and backend summaries render here for the MVP. Later gates can replace this with structured report cards and filters.", reportPanel, 16, FontStyles.Normal, muted, regularFont);
            reportText = CreateText("Run capabilities or validation to populate this view.", reportPanel, 16, FontStyles.Normal, muted, regularFont);
            reportText.fontSize = 15;
        }

        private async Task RunVersionAsync()
        {
            backendStatusText.text = "Backend: checking";
            var result = await runner.RunAsync(new[] { "--version" });
            AppendLog(result);
            backendStatusText.text = result.Succeeded
                ? $"Backend: {FirstLine(result.StandardOutput)}"
                : "Backend: unavailable";
            backendStatusText.color = result.Succeeded ? success : danger;
        }

        private async Task RunCapabilitiesAsync()
        {
            capabilityStatusText.text = "Capabilities: running";
            var result = await runner.RunAsync(new[] { "capabilities", "list", "--format", "json" });
            AppendLog(result);
            capabilityStatusText.text = result.Succeeded ? "Capabilities: loaded JSON catalogue" : "Capabilities: command failed";
            capabilityStatusText.color = result.Succeeded ? success : danger;
            reportText.text = result.Succeeded ? result.StandardOutput : result.StandardError;
        }

        private async Task RunValidateAsync()
        {
            var projectRoot = projectRootInput.text?.Trim();
            if (string.IsNullOrWhiteSpace(projectRoot))
            {
                validationStatusText.text = "Validation: project root required";
                validationStatusText.color = danger;
                return;
            }

            validationStatusText.text = "Validation: running";
            var result = await runner.RunAsync(new[] { "validate", projectRoot, "--format", "json" });
            AppendLog(result);
            validationStatusText.text = result.Succeeded ? "Validation: passed" : $"Validation: exit {result.ExitCode}";
            validationStatusText.color = result.Succeeded ? success : danger;
            reportText.text = string.IsNullOrWhiteSpace(result.StandardOutput) ? result.StandardError : result.StandardOutput;
        }

        private void AppendLog(ForgeCommandResult result)
        {
            logText.text =
                $"$ {result.CommandLine}\n" +
                $"exit: {result.ExitCode}\n\n" +
                $"{TrimForLog(result.StandardOutput)}\n" +
                $"{TrimForLog(result.StandardError)}";
        }

        private RectTransform CreateSurface(string name, Transform parent, Color color)
        {
            var rect = CreateRect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            if (panelSprite != null)
            {
                image.sprite = panelSprite;
                image.type = Image.Type.Sliced;
            }

            rect.gameObject.AddComponent<LayoutElement>();
            return rect;
        }

        private Button CreateActionButton(Transform parent, string label, Func<Task> action, Color color)
        {
            var rect = CreateRect(label + " Button", parent);
            rect.sizeDelta = new Vector2(180, 46);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            if (outlineSprite != null)
            {
                image.sprite = outlineSprite;
                image.type = Image.Type.Sliced;
            }

            var button = rect.gameObject.AddComponent<Button>();
            button.onClick.AddListener(async () => await action());

            var textElement = CreateText(label, rect, 17, FontStyles.Normal, color == accent ? background : text, boldFont);
            Stretch(textElement.rectTransform);
            textElement.alignment = TextAlignmentOptions.Center;
            return button;
        }

        private void CreateRailButton(Transform parent, string label, string subLabel)
        {
            var row = CreateSurface(label + " Rail Button", parent, surfaceAlt);
            row.GetComponent<LayoutElement>().preferredHeight = 70;
            var layout = row.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 10, 10);
            layout.spacing = 0;
            CreateText(label, row, 18, FontStyles.Normal, text, boldFont);
            CreateText(subLabel, row, 13, FontStyles.Normal, muted, regularFont);
        }

        private TMP_InputField CreateInput(Transform parent, string initialValue)
        {
            var rect = CreateSurface("Project Root Input", parent, surfaceAlt);
            rect.GetComponent<LayoutElement>().preferredHeight = 48;

            var textObject = CreateRect("Text", rect);
            Stretch(textObject, 14, 0, -14, 0);
            var textComponent = textObject.gameObject.AddComponent<TextMeshProUGUI>();
            ApplyTextDefaults(textComponent, 16, FontStyles.Normal, text, regularFont);
            textComponent.alignment = TextAlignmentOptions.MidlineLeft;

            var input = rect.gameObject.AddComponent<TMP_InputField>();
            input.textViewport = rect;
            input.textComponent = textComponent;
            input.text = initialValue;
            return input;
        }

        private void CreateMetricRow(Transform parent, string label, string value, string note)
        {
            var row = CreateRect(label + " Metric", parent);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 38;
            var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleLeft;
            CreateText(label, row, 15, FontStyles.Normal, muted, regularFont).rectTransform.sizeDelta = new Vector2(100, 30);
            CreateText(value, row, 18, FontStyles.Normal, accent, boldFont).rectTransform.sizeDelta = new Vector2(120, 30);
            CreateText(note, row, 14, FontStyles.Normal, muted, regularFont);
        }

        private TMP_Text CreateText(string value, Transform parent, float size, FontStyles style, Color color, TMP_FontAsset font)
        {
            var rect = CreateRect(value + " Text", parent);
            var textComponent = rect.gameObject.AddComponent<TextMeshProUGUI>();
            ApplyTextDefaults(textComponent, size, style, color, font);
            textComponent.text = value;
            return textComponent;
        }

        private void ApplyTextDefaults(TMP_Text textComponent, float size, FontStyles style, Color color, TMP_FontAsset font)
        {
            textComponent.font = font != null ? font : textComponent.font;
            textComponent.fontSize = size;
            textComponent.fontStyle = style;
            textComponent.color = color;
            textComponent.enableWordWrapping = true;
            textComponent.overflowMode = TextOverflowModes.Ellipsis;
            textComponent.lineSpacing = 0;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        private static void Stretch(RectTransform rect, float left = 0, float top = 0, float right = 0, float bottom = 0)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(right, -top);
        }

        private static void AddSpacer(Transform parent, float height)
        {
            var spacer = CreateRect("Spacer", parent);
            spacer.gameObject.AddComponent<LayoutElement>().preferredHeight = height;
        }

        private static void AddFlexibleSpacer(Transform parent)
        {
            var spacer = CreateRect("Flexible Spacer", parent);
            spacer.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1;
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static string FirstLine(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "(no output)"
                : value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? value;
        }

        private static string TrimForLog(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return value.Length <= 6000 ? value : value.Substring(0, 6000) + "\n...";
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var color);
            return color;
        }
    }
}
