using System.Collections.Generic;
using System.IO;
using BusJam.Core;
using BusJam.Data;
using UnityEditor;
using UnityEngine;

namespace BusJam.Editor
{
    public class LevelEditorWindow : EditorWindow
    {
        private const int CELL_SIZE = 40;
        private const int COLOR_BOX_SIZE = 18;
        private const int MAX_COLORS = 6;
        private const string PALETTE_PATH = "Assets/_Game/SO/ColorPallete.asset";
        private const string CAMERA_CONFIG_PATH = "Assets/_Game/SO/CameraConfig.asset";

        private static readonly string[] TOOL_NAMES = { "Passenger", "Blocked", "Tunnel" };
        private static readonly string[] ORIENTATION_LABELS = { "Up", "Right", "Down", "Left" };
        private static readonly string[] ARROW_LABELS = { "^", ">", "v", "<" };

        private string[] _colorNames;
        private ColorPalette _colorPalette;
        private CameraConfig _cameraConfig;
        private LevelData _levelData;
        private int _levelId = 1;
        private Vector2 _scrollPosition;
        private int _selectedColor;
        private int _selectedTool;
        private int _tunnelOrientation;
        private bool _isEraseMode;

        [MenuItem("BusJam/Level Editor")]
        public static void ShowWindow()
        {
            GetWindow<LevelEditorWindow>("Level Editor");
        }

        private void OnEnable()
        {
            _colorPalette = AssetDatabase.LoadAssetAtPath<ColorPalette>(PALETTE_PATH);
            _cameraConfig = AssetDatabase.LoadAssetAtPath<CameraConfig>(CAMERA_CONFIG_PATH);
            BuildColorNames();
        }

        private void BuildColorNames()
        {
            _colorNames = new string[MAX_COLORS];
            for (int i = 0; i < MAX_COLORS; i++)
            {
                _colorNames[i] = ((ColorType)i).ToString();
            }
        }

        private Color GetColor(int colorIndex)
        {
            if (_colorPalette == null)
            {
                return Color.gray;
            }
            return _colorPalette.GetColor((ColorType)colorIndex);
        }
        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            DrawLevelControls();
            EditorGUILayout.Space(10);

            if (_levelData != null)
            {
                DrawToolbar();
                EditorGUILayout.Space(10);
                DrawGrid();
                EditorGUILayout.Space(5);
                DrawTunnelDetails();
                EditorGUILayout.Space(10);
                DrawBusSection();
                EditorGUILayout.Space(10);
                DrawSettings();
                EditorGUILayout.Space(10);
                DrawCameraSection();
                EditorGUILayout.Space(10);
                DrawValidation();
                EditorGUILayout.Space(10);
                DrawSaveButton();
                EditorGUILayout.Space(5);
                DrawSetLevelButton();
                EditorGUILayout.Space(5);
                DrawPlayButton();
            }

            EditorGUILayout.EndScrollView();
        }
        private void DrawLevelControls()
        {
            EditorGUILayout.LabelField("Level", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            _levelId = EditorGUILayout.IntField("ID", _levelId);
            if (GUILayout.Button("Load", GUILayout.Width(60)))
            {
                LoadLevel();
            }
            if (GUILayout.Button("New", GUILayout.Width(60)))
            {
                CreateNewLevel();
            }
            EditorGUILayout.EndHorizontal();

            if (_levelData == null)
            {
                return;
            }

            EditorGUILayout.BeginHorizontal();
            int newWidth = EditorGUILayout.IntField("Width", _levelData.gridWidth);
            int newHeight = EditorGUILayout.IntField("Height", _levelData.gridHeight);
            EditorGUILayout.EndHorizontal();

            newWidth = Mathf.Clamp(newWidth, 2, 10);
            newHeight = Mathf.Clamp(newHeight, 2, 10);

            if (newWidth != _levelData.gridWidth || newHeight != _levelData.gridHeight)
            {
                ResizeGrid(newWidth, newHeight);
            }
        }
        private void DrawToolbar()
        {
            EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);
            _selectedTool = GUILayout.Toolbar(_selectedTool, TOOL_NAMES);

            if (_selectedTool == 0)
            {
                DrawColorPicker();
            }
            else if (_selectedTool == 2)
            {
                _tunnelOrientation = EditorGUILayout.Popup(
                    "Direction", _tunnelOrientation, ORIENTATION_LABELS);
            }
        }

        private void DrawColorPicker()
        {
            EditorGUILayout.BeginHorizontal();
            Color oldBg = GUI.backgroundColor;

            GUI.backgroundColor = _isEraseMode ? Color.white : Color.gray;
            if (GUILayout.Button("Erase", GUILayout.Width(50), GUILayout.Height(25)))
            {
                _isEraseMode = true;
            }

            for (int i = 0; i < MAX_COLORS; i++)
            {
                bool isSelected = !_isEraseMode && _selectedColor == i;
                GUI.backgroundColor = isSelected ? Color.white : GetColor(i);
                if (GUILayout.Button(_colorNames[i], GUILayout.Height(25)))
                {
                    _selectedColor = i;
                    _isEraseMode = false;
                }
            }

            GUI.backgroundColor = oldBg;
            EditorGUILayout.EndHorizontal();
        }
        private void DrawGrid()
        {
            EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);
            Color oldBg = GUI.backgroundColor;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(30);
            for (int x = 0; x < _levelData.gridWidth; x++)
            {
                GUI.backgroundColor = new Color(0.4f, 0.8f, 1f);
                GUILayout.Button("EXIT", GUILayout.Width(CELL_SIZE), GUILayout.Height(18));
            }
            GUI.backgroundColor = oldBg;
            EditorGUILayout.EndHorizontal();

            for (int y = 0; y < _levelData.gridHeight; y++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(y.ToString(), GUILayout.Width(25));

                for (int x = 0; x < _levelData.gridWidth; x++)
                {
                    CellData cell = _levelData.rows[y].cells[x];
                    GetCellDisplay(cell, out string label, out Color bgColor);

                    GUI.backgroundColor = bgColor;
                    if (GUILayout.Button(label,
                        GUILayout.Width(CELL_SIZE), GUILayout.Height(CELL_SIZE)))
                    {
                        HandleCellClick(x, y);
                        Repaint();
                    }
                }

                GUI.backgroundColor = oldBg;
                EditorGUILayout.EndHorizontal();
            }

            GUI.backgroundColor = oldBg;
        }

        private void GetCellDisplay(CellData cell, out string label, out Color bgColor)
        {
            if (cell.isBlocked)
            {
                label = "X";
                bgColor = new Color(0.2f, 0.2f, 0.2f);
                return;
            }

            if (cell.isTunnel)
            {
                int count = cell.passengers != null ? cell.passengers.Length : 0;
                label = ARROW_LABELS[cell.orientation] + " " + count;
                bgColor = new Color(0.6f, 0.4f, 0.2f);
                return;
            }

            if (cell.passengers != null && cell.passengers.Length > 0)
            {
                int colorIndex = cell.passengers[0].color;
                if (colorIndex >= 0 && colorIndex < MAX_COLORS)
                {
                    label = _colorNames[colorIndex][0].ToString();
                    bgColor = GetColor(colorIndex);
                }
                else
                {
                    label = "?";
                    bgColor = Color.gray;
                }
                return;
            }

            label = "";
            bgColor = new Color(0.85f, 0.85f, 0.85f);
        }

        private void DrawTunnelDetails()
        {
            bool hasTunnels = false;
            for (int y = 0; y < _levelData.gridHeight; y++)
            {
                for (int x = 0; x < _levelData.gridWidth; x++)
                {
                    if (_levelData.rows[y].cells[x].isTunnel)
                    {
                        hasTunnels = true;
                        break;
                    }
                }
                if (hasTunnels)
                {
                    break;
                }
            }

            if (!hasTunnels)
            {
                return;
            }

            EditorGUILayout.LabelField("Tunnel Queue", EditorStyles.boldLabel);
            Color oldBg = GUI.backgroundColor;

            for (int y = 0; y < _levelData.gridHeight; y++)
            {
                for (int x = 0; x < _levelData.gridWidth; x++)
                {
                    CellData cell = _levelData.rows[y].cells[x];
                    if (!cell.isTunnel)
                    {
                        continue;
                    }

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(
                        "[" + x + "," + y + "] " + ARROW_LABELS[cell.orientation],
                        GUILayout.Width(60));

                    if (cell.passengers != null)
                    {
                        for (int p = 0; p < cell.passengers.Length; p++)
                        {
                            int color = cell.passengers[p].color;
                            GUI.backgroundColor = GetColor(color);
                            GUILayout.Button(_colorNames[color][0].ToString(),
                                GUILayout.Width(COLOR_BOX_SIZE),
                                GUILayout.Height(COLOR_BOX_SIZE));
                        }
                    }

                    GUI.backgroundColor = oldBg;

                    if (cell.passengers == null || cell.passengers.Length == 0)
                    {
                        EditorGUILayout.LabelField("(empty)");
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            GUI.backgroundColor = oldBg;
        }
        private void HandleCellClick(int x, int y)
        {
            CellData cell = _levelData.rows[y].cells[x];

            if (_selectedTool == 0)
            {
                HandlePassengerTool(cell);
            }
            else if (_selectedTool == 1)
            {
                HandleBlockedTool(cell);
            }
            else if (_selectedTool == 2)
            {
                HandleTunnelTool(cell);
            }
        }

        private void HandlePassengerTool(CellData cell)
        {
            if (_isEraseMode)
            {
                if (cell.isTunnel && cell.passengers != null && cell.passengers.Length > 0)
                {
                    List<PassengerData> list = new(cell.passengers);
                    list.RemoveAt(list.Count - 1);
                    cell.passengers = list.ToArray();
                }
                else
                {
                    cell.passengers = new PassengerData[0];
                    cell.isBlocked = false;
                    cell.isTunnel = false;
                }
                return;
            }

            if (cell.isTunnel)
            {
                PassengerData[] current = cell.passengers != null
                    ? cell.passengers : new PassengerData[0];
                List<PassengerData> list = new(current);
                list.Add(new PassengerData { color = _selectedColor });
                cell.passengers = list.ToArray();
            }
            else
            {
                cell.passengers = new PassengerData[] { new() { color = _selectedColor } };
                cell.isBlocked = false;
            }
        }

        private void HandleBlockedTool(CellData cell)
        {
            cell.isBlocked = !cell.isBlocked;
            if (cell.isBlocked)
            {
                cell.passengers = new PassengerData[0];
                cell.isTunnel = false;
            }
        }

        private void HandleTunnelTool(CellData cell)
        {
            if (cell.isTunnel)
            {
                cell.isTunnel = false;
                cell.passengers = new PassengerData[0];
            }
            else
            {
                cell.isTunnel = true;
                cell.orientation = _tunnelOrientation;
                cell.isBlocked = false;
                cell.passengers = new PassengerData[0];
            }
        }

        private void DrawBusSection()
        {
            EditorGUILayout.LabelField("Buses (drag order = spawn order)", EditorStyles.boldLabel);

            if (_levelData.buses == null)
            {
                _levelData.buses = new BusData[0];
            }

            List<BusData> busList = new(_levelData.buses);
            int removeIndex = -1;
            int moveUp = -1;
            int moveDown = -1;

            for (int i = 0; i < busList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("#" + (i + 1), GUILayout.Width(30));

                GUI.enabled = i > 0;
                if (GUILayout.Button("\u25B2", GUILayout.Width(20)))
                {
                    moveUp = i;
                }
                GUI.enabled = i < busList.Count - 1;
                if (GUILayout.Button("\u25BC", GUILayout.Width(20)))
                {
                    moveDown = i;
                }
                GUI.enabled = true;

                busList[i].color = EditorGUILayout.Popup(
                    busList[i].color, _colorNames, GUILayout.Width(80));
                busList[i].capacity = EditorGUILayout.IntSlider(busList[i].capacity, 1, 3);
                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    removeIndex = i;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (moveUp > 0)
            {
                (busList[moveUp], busList[moveUp - 1]) = (busList[moveUp - 1], busList[moveUp]);
            }
            if (moveDown >= 0 && moveDown < busList.Count - 1)
            {
                (busList[moveDown], busList[moveDown + 1]) = (busList[moveDown + 1], busList[moveDown]);
            }

            if (removeIndex >= 0)
            {
                busList.RemoveAt(removeIndex);
            }

            if (GUILayout.Button("+ Add Bus"))
            {
                busList.Add(new BusData { color = 0, capacity = 2 });
            }

            _levelData.buses = busList.ToArray();
        }

        private void DrawSettings()
        {
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            _levelData.timeLimit = EditorGUILayout.IntSlider("Time Limit (sec)", _levelData.timeLimit, 30, 300);
            _levelData.waitingCellCount = EditorGUILayout.IntSlider("Waiting Cells", _levelData.waitingCellCount, 3, 10);
        }

        private void DrawCameraSection()
        {
            EditorGUILayout.LabelField("Camera", EditorStyles.boldLabel);

            if (_cameraConfig == null)
            {
                EditorGUILayout.HelpBox("CameraConfig not found at " + CAMERA_CONFIG_PATH, MessageType.Warning);
                return;
            }

            List<CameraConfig.CameraPreset> presetsList = _cameraConfig.PresetsList;
            int removeIndex = -1;

            for (int i = 0; i < presetsList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(presetsList[i].gridWidth + "x" + presetsList[i].gridHeight, GUILayout.Width(40));
                presetsList[i].gridWidth = EditorGUILayout.IntField(presetsList[i].gridWidth, GUILayout.Width(30));
                presetsList[i].gridHeight = EditorGUILayout.IntField(presetsList[i].gridHeight, GUILayout.Width(30));
                presetsList[i].position = EditorGUILayout.Vector3Field("", presetsList[i].position);
                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    removeIndex = i;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (removeIndex >= 0)
            {
                presetsList.RemoveAt(removeIndex);
                EditorUtility.SetDirty(_cameraConfig);
            }

            if (GUILayout.Button("+ Add Preset"))
            {
                presetsList.Add(new CameraConfig.CameraPreset
                {
                    gridWidth = _levelData.gridWidth,
                    gridHeight = _levelData.gridHeight,
                    position = Vector3.zero
                }); EditorUtility.SetDirty(_cameraConfig);
            }

            _cameraConfig.DefaultPosition = EditorGUILayout.Vector3Field("Default Position", _cameraConfig.DefaultPosition);

            EditorGUILayout.Space(5);

            int matchIndex = _cameraConfig.FindPresetIndex(
                _levelData.gridWidth, _levelData.gridHeight);

            if (matchIndex >= 0)
            {
                _levelData.cameraPreset = matchIndex;
                EditorGUILayout.LabelField("Active", presetsList[matchIndex].gridWidth + "x" + presetsList[matchIndex].gridHeight + " (auto)");
            }
            else
            {
                _levelData.cameraPreset = -1;
                EditorGUILayout.LabelField("Active", "Default");
            }
        }
        private void DrawValidation()
        {
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);

            var passengerCounts = new int[MAX_COLORS];
            var busCounts = new int[MAX_COLORS];

            for (int y = 0; y < _levelData.gridHeight; y++)
            {
                for (int x = 0; x < _levelData.gridWidth; x++)
                {
                    CellData cell = _levelData.rows[y].cells[x];
                    if (cell.passengers == null)
                    {
                        continue;
                    }
                    for (int p = 0; p < cell.passengers.Length; p++)
                    {
                        int color = cell.passengers[p].color;
                        if (color >= 0 && color < MAX_COLORS)
                        {
                            passengerCounts[color]++;
                        }
                    }
                }
            }

            if (_levelData.buses != null)
            {
                for (int i = 0; i < _levelData.buses.Length; i++)
                {
                    int color = _levelData.buses[i].color;
                    if (color >= 0 && color < MAX_COLORS)
                    {
                        busCounts[color] += _levelData.buses[i].capacity;
                    }
                }
            }

            var isValid = true;
            var hasAnyColor = false;

            for (int i = 0; i < MAX_COLORS; i++)
            {
                if (passengerCounts[i] == 0 && busCounts[i] == 0)
                {
                    continue;
                }
                hasAnyColor = true;

                if (passengerCounts[i] != busCounts[i])
                {
                    EditorGUILayout.HelpBox(
                        _colorNames[i] + ": " + passengerCounts[i] + " passengers / " + busCounts[i] + " bus seats", MessageType.Warning);
                    isValid = false;
                }
                else
                {
                    EditorGUILayout.LabelField("  " + _colorNames[i] + ": " + passengerCounts[i] + " / " + busCounts[i]);
                }
            }
            if (isValid && hasAnyColor)
            {
                EditorGUILayout.HelpBox("Level is valid!", MessageType.Info);
            }
        }
        private void DrawSaveButton()
        {
            _levelData.id = _levelId;
            var oldBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.3f, 0.9f, 0.3f);
            if (GUILayout.Button("Save Level", GUILayout.Height(30)))
            {
                SaveLevel();
            }
            GUI.backgroundColor = oldBg;
        }

        private void DrawSetLevelButton()
        {
            if (GUILayout.Button("Set As Start Level", GUILayout.Height(25)))
            {
                PlayerPrefs.SetInt("CurrentLevel", _levelId);
                PlayerPrefs.Save();
                Debug.Log("Start level set to " + _levelId);
            }
        }

        private void DrawPlayButton()
        {
            if (EditorApplication.isPlaying)
            {
                Color oldBg = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.3f, 0.6f, 1f);
                if (GUILayout.Button("Play Level", GUILayout.Height(30)))
                {
                    SaveLevel();
                    GameManager gm = Object.FindObjectOfType<GameManager>();
                    if (gm != null)
                    {
                        gm.ReloadLevel(_levelId);
                    }
                }
                GUI.backgroundColor = oldBg;
            }
            else
            {
                EditorGUILayout.HelpBox("Enter Play mode to test the level instantly with the Play Level button.", MessageType.Info);
            }
        }

        private void CreateNewLevel()
        {
            _levelData = new LevelData
            {
                id = _levelId,
                gridWidth = 5,
                gridHeight = 5,
                timeLimit = 120,
                waitingCellCount = 5,
                buses = new BusData[0]
            };
            _levelData.rows = new RowData[_levelData.gridHeight];

            for (int y = 0; y < _levelData.gridHeight; y++)
            {
                _levelData.rows[y] = new RowData
                {
                    cells = new CellData[_levelData.gridWidth]
                };
                for (int x = 0; x < _levelData.gridWidth; x++)
                {
                    _levelData.rows[y].cells[x] = new CellData
                    {
                        passengers = new PassengerData[0],
                        isBlocked = false,
                        isTunnel = false,
                        orientation = 0
                    };
                }
            }
        }

        private void LoadLevel()
        {
            string path = Application.dataPath + "/_Game/Resources/Levels/Level_" + _levelId + ".json";

            if (!File.Exists(path))
            {
                EditorUtility.DisplayDialog("Error", "Level_" + _levelId + ".json not found!", "OK");
                return;
            }
            string json = File.ReadAllText(path);
            _levelData = JsonUtility.FromJson<LevelData>(json);
        }

        private void SaveLevel()
        {
            _levelData.id = _levelId;
            string json = JsonUtility.ToJson(_levelData, true);
            string directoryPath = Application.dataPath + "/_Game/Resources/Levels";

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string filePath = directoryPath + "/Level_" + _levelId + ".json";
            File.WriteAllText(filePath, json);
            AssetDatabase.Refresh();
        }

        private void ResizeGrid(int newWidth, int newHeight)
        {
            RowData[] newRows = new RowData[newHeight];

            for (int y = 0; y < newHeight; y++)
            {
                newRows[y] = new RowData { cells = new CellData[newWidth] };
                for (int x = 0; x < newWidth; x++)
                {
                    if (y < _levelData.gridHeight && x < _levelData.gridWidth)
                    {
                        newRows[y].cells[x] = _levelData.rows[y].cells[x];
                    }
                    else
                    {
                        newRows[y].cells[x] = new CellData
                        {
                            passengers = new PassengerData[0],
                            isBlocked = false,
                            isTunnel = false,
                            orientation = 0
                        };
                    }
                }
            }
            _levelData.rows = newRows;
            _levelData.gridWidth = newWidth;
            _levelData.gridHeight = newHeight;
        }
    }
}
