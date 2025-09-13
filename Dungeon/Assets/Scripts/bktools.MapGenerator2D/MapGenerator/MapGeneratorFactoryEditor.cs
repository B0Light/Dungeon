using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
/// <summary>
/// MapGeneratorFactory의 커스텀 에디터
/// 4종류의 맵 생성 알고리즘을 버튼으로 선택하고 해당 설정을 표시합니다.
/// </summary>
[UnityEditor.CustomEditor(typeof(MapGeneratorFactory))]
public class MapGeneratorFactoryEditor : UnityEditor.Editor
{
    private bool showBasicSettings = true;
    private bool showAutoGenerationSettings = true;
    private bool showIsaacSettings = false;
    private bool showDelaunaySettings = false;
    private bool showBSPSettings = false;
    private bool showBSPFullSettings = false;
    
    public override void OnInspectorGUI()
    {
        MapGeneratorFactory factory = (MapGeneratorFactory)target;
        
        // 맵 생성기 타입 선택 버튼들
        EditorGUILayout.LabelField("맵 생성기 선택", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("BSP", GUILayout.Height(30)))
        {
            factory.SetGeneratorType(MapGeneratorType.BSP);
            showBSPSettings = true;
            showBSPFullSettings = false;
            showIsaacSettings = false;
            showDelaunaySettings = false;
        }
        if (GUILayout.Button("BSP Full", GUILayout.Height(30)))
        {
            factory.SetGeneratorType(MapGeneratorType.BSPFull);
            showBSPFullSettings = true;
            showBSPSettings = false;
            showIsaacSettings = false;
            showDelaunaySettings = false;
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Isaac", GUILayout.Height(30)))
        {
            factory.SetGeneratorType(MapGeneratorType.Isaac);
            showIsaacSettings = true;
            showBSPSettings = false;
            showBSPFullSettings = false;
            showDelaunaySettings = false;
        }
        if (GUILayout.Button("Delaunay", GUILayout.Height(30)))
        {
            factory.SetGeneratorType(MapGeneratorType.Delaunay);
            showDelaunaySettings = true;
            showBSPSettings = false;
            showBSPFullSettings = false;
            showIsaacSettings = false;
        }
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(20);
        
        // 기본 설정 (항상 표시)
        showBasicSettings = EditorGUILayout.Foldout(showBasicSettings, "기본 설정", true);
        if (showBasicSettings)
        {
            EditorGUI.indentLevel++;
            SerializedProperty gridSizeProp = serializedObject.FindProperty("gridSize");
            SerializedProperty cubeSizeProp = serializedObject.FindProperty("cubeSize");
            SerializedProperty seedProp = serializedObject.FindProperty("seed");
            SerializedProperty slotProp = serializedObject.FindProperty("slot");
            SerializedProperty tileMappingDataProp = serializedObject.FindProperty("tileMappingDataSO");
            
            EditorGUILayout.PropertyField(gridSizeProp);
            EditorGUILayout.PropertyField(cubeSizeProp);
            EditorGUILayout.PropertyField(seedProp);
            EditorGUILayout.PropertyField(slotProp);
            EditorGUILayout.PropertyField(tileMappingDataProp);
            EditorGUI.indentLevel--;
        }
        
        GUILayout.Space(10);
        
        // 자동 생성 설정
        showAutoGenerationSettings = EditorGUILayout.Foldout(showAutoGenerationSettings, "시작 시 자동 생성 설정", true);
        if (showAutoGenerationSettings)
        {
            EditorGUI.indentLevel++;
            
            SerializedProperty autoGenerateProp = serializedObject.FindProperty("autoGenerateOnStart");
            EditorGUILayout.PropertyField(autoGenerateProp, new GUIContent("시작 시 자동 생성"));
            
            if (autoGenerateProp.boolValue)
            {
                GUILayout.Space(5);
                
                SerializedProperty autoModeProp = serializedObject.FindProperty("autoMapGenerationMode");
                SerializedProperty specificTypeProp = serializedObject.FindProperty("specificMapType");
                SerializedProperty randomTypesProp = serializedObject.FindProperty("randomMapTypes");
                
                // 자동 생성 모드 선택
                EditorGUILayout.PropertyField(autoModeProp, new GUIContent("자동 생성 모드"));
                
                // 모드에 따른 설정 표시
                AutoMapGenerationMode currentMode = (AutoMapGenerationMode)autoModeProp.enumValueIndex;
                
                switch (currentMode)
                {
                    case AutoMapGenerationMode.UseCurrentType:
                        EditorGUILayout.HelpBox("현재 설정된 맵 타입을 사용합니다.", MessageType.Info);
                        break;
                        
                    case AutoMapGenerationMode.UseSpecificType:
                        EditorGUILayout.PropertyField(specificTypeProp, new GUIContent("특정 맵 타입"));
                        EditorGUILayout.HelpBox($"항상 {specificTypeProp.enumNames[specificTypeProp.enumValueIndex]} 맵으로 시작합니다.", MessageType.Info);
                        break;
                        
                    case AutoMapGenerationMode.UseRandomType:
                        EditorGUILayout.PropertyField(randomTypesProp, new GUIContent("랜덤 맵 타입들"));
                        
                        if (randomTypesProp.arraySize == 0)
                        {
                            EditorGUILayout.HelpBox("랜덤 선택할 맵 타입을 추가해주세요.", MessageType.Warning);
                        }
                        else
                        {
                            string typeList = "";
                            for (int i = 0; i < randomTypesProp.arraySize; i++)
                            {
                                if (i > 0) typeList += ", ";
                                var element = randomTypesProp.GetArrayElementAtIndex(i);
                                typeList += System.Enum.GetName(typeof(MapGeneratorType), element.enumValueIndex);
                            }
                            EditorGUILayout.HelpBox($"다음 타입 중 랜덤 선택: {typeList}", MessageType.Info);
                        }
                        break;
                }
                
                GUILayout.Space(5);
                
                // 미리보기 버튼
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("다음 생성될 맵 타입 확인", GUILayout.Height(25)))
                {
                    MapGeneratorType nextType = factory.GetNextAutoMapType();
                    EditorUtility.DisplayDialog("다음 맵 타입", $"다음에 생성될 맵 타입: {nextType}", "확인");
                }
                
                if (GUILayout.Button("랜덤 맵 즉시 생성", GUILayout.Height(25)))
                {
                    factory.GenerateRandomMap();
                }
                EditorGUILayout.EndHorizontal();
                
                if (GUILayout.Button("자동 선택 맵 생성", GUILayout.Height(30)))
                {
                    factory.GenerateAutoSelectedMap();
                }
                
                GUILayout.Space(5);
                
                // 빠른 설정 버튼들
                EditorGUILayout.LabelField("빠른 설정", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("현재 타입 모드", GUILayout.Height(20)))
                {
                    autoModeProp.enumValueIndex = (int)AutoMapGenerationMode.UseCurrentType;
                }
                if (GUILayout.Button("특정 타입 모드", GUILayout.Height(20)))
                {
                    autoModeProp.enumValueIndex = (int)AutoMapGenerationMode.UseSpecificType;
                }
                if (GUILayout.Button("랜덤 타입 모드", GUILayout.Height(20)))
                {
                    autoModeProp.enumValueIndex = (int)AutoMapGenerationMode.UseRandomType;
                }
                EditorGUILayout.EndHorizontal();
                
                // 랜덤 타입 프리셋 버튼들
                if (currentMode == AutoMapGenerationMode.UseRandomType)
                {
                    EditorGUILayout.LabelField("랜덤 타입 프리셋", EditorStyles.miniLabel);
                    
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("전체", GUILayout.Height(18)))
                    {
                        SetRandomTypesPreset(randomTypesProp, new[] { 
                            MapGeneratorType.BSP, MapGeneratorType.BSPFull, 
                            MapGeneratorType.Isaac, MapGeneratorType.Delaunay 
                        });
                    }
                    if (GUILayout.Button("BSP류", GUILayout.Height(18)))
                    {
                        SetRandomTypesPreset(randomTypesProp, new[] { 
                            MapGeneratorType.BSP, MapGeneratorType.BSPFull 
                        });
                    }
                    if (GUILayout.Button("기본", GUILayout.Height(18)))
                    {
                        SetRandomTypesPreset(randomTypesProp, new[] { 
                            MapGeneratorType.BSP, MapGeneratorType.Isaac, MapGeneratorType.Delaunay 
                        });
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            EditorGUI.indentLevel--;
        }
        
        GUILayout.Space(10);
        
        // Isaac 맵 생성기 설정
        if (showIsaacSettings)
        {
            EditorGUILayout.LabelField("Isaac 맵 생성기 설정", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            SerializedProperty isaacMaxRoomsProp = serializedObject.FindProperty("isaacMaxRooms");
            SerializedProperty isaacSpecialRoomCountProp = serializedObject.FindProperty("isaacSpecialRoomCount");
            SerializedProperty isaacHorizontalSizeProp = serializedObject.FindProperty("isaacHorizontalSize");
            SerializedProperty isaacVerticalSizeProp = serializedObject.FindProperty("isaacVerticalSize");
            
            EditorGUILayout.PropertyField(isaacMaxRoomsProp);
            EditorGUILayout.PropertyField(isaacSpecialRoomCountProp);
            EditorGUILayout.PropertyField(isaacHorizontalSizeProp);
            EditorGUILayout.PropertyField(isaacVerticalSizeProp);
            
            // 경로 설정
            GUILayout.Space(10);
            EditorGUILayout.LabelField("경로 설정", EditorStyles.boldLabel);
            SerializedProperty isaacPathTypeProp = serializedObject.FindProperty("isaacPathType");
            SerializedProperty isaacPathValueProp = serializedObject.FindProperty("isaacPathValue");
            
            if (isaacPathTypeProp != null)
                EditorGUILayout.PropertyField(isaacPathTypeProp, new UnityEngine.GUIContent("경로 타입"));
            if (isaacPathValueProp != null)
                EditorGUILayout.PropertyField(isaacPathValueProp, new UnityEngine.GUIContent("경로 생성 확률"));
                
            EditorGUI.indentLevel--;
        }
        
        // Delaunay 맵 생성기 설정
        if (showDelaunaySettings)
        {
            EditorGUILayout.LabelField("Delaunay 맵 생성기 설정", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            SerializedProperty delaunayMinRoomSizeProp = serializedObject.FindProperty("delaunayMinRoomSize");
            SerializedProperty delaunayMaxRoomSizeProp = serializedObject.FindProperty("delaunayMaxRoomSize");
            SerializedProperty delaunayPathValueProp = serializedObject.FindProperty("delaunayPathValue");
            
            EditorGUILayout.PropertyField(delaunayMinRoomSizeProp);
            EditorGUILayout.PropertyField(delaunayMaxRoomSizeProp);
            EditorGUILayout.PropertyField(delaunayPathValueProp);
            
            // 경로 설정
            GUILayout.Space(10);
            EditorGUILayout.LabelField("경로 설정", EditorStyles.boldLabel);
            SerializedProperty delaunayPathTypeProp = serializedObject.FindProperty("delaunayPathType");
            
            if (delaunayPathTypeProp != null)
                EditorGUILayout.PropertyField(delaunayPathTypeProp, new UnityEngine.GUIContent("경로 타입"));
                
            EditorGUI.indentLevel--;
        }
        
        // BSP 맵 생성기 설정
        if (showBSPSettings)
        {
            EditorGUILayout.LabelField("BSP 맵 생성기 설정", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            SerializedProperty bspMinRoomSizeProp = serializedObject.FindProperty("bspMinRoomSize");
            SerializedProperty bspMaxRoomSizeProp = serializedObject.FindProperty("bspMaxRoomSize");
            SerializedProperty bspMaxDepthProp = serializedObject.FindProperty("bspMaxDepth");
            
            EditorGUILayout.PropertyField(bspMinRoomSizeProp);
            EditorGUILayout.PropertyField(bspMaxRoomSizeProp);
            EditorGUILayout.PropertyField(bspMaxDepthProp);
            
            // 경로 설정
            GUILayout.Space(10);
            EditorGUILayout.LabelField("경로 설정", EditorStyles.boldLabel);
            SerializedProperty bspPathTypeProp = serializedObject.FindProperty("bspPathType");
            SerializedProperty bspPathValueProp = serializedObject.FindProperty("bspPathValue");
            
            if (bspPathTypeProp != null)
                EditorGUILayout.PropertyField(bspPathTypeProp, new UnityEngine.GUIContent("경로 타입"));
            if (bspPathValueProp != null)
                EditorGUILayout.PropertyField(bspPathValueProp, new UnityEngine.GUIContent("경로 생성 확률"));
                
            EditorGUI.indentLevel--;
        }
        
        // BSP Full 맵 생성기 설정
        if (showBSPFullSettings)
        {
            EditorGUILayout.LabelField("BSP Full 맵 생성기 설정", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            SerializedProperty bspFullMinSplitSizeProp = serializedObject.FindProperty("bspFullMinSplitSize");
            SerializedProperty bspFullMaxDepthProp = serializedObject.FindProperty("bspFullMaxDepth");
            
            EditorGUILayout.PropertyField(bspFullMinSplitSizeProp);
            EditorGUILayout.PropertyField(bspFullMaxDepthProp);
            
            // 경로 설정
            GUILayout.Space(10);
            EditorGUILayout.LabelField("경로 설정", EditorStyles.boldLabel);
            SerializedProperty bspFullPathTypeProp = serializedObject.FindProperty("bspFullPathType");
            SerializedProperty bspFullPathValueProp = serializedObject.FindProperty("bspFullPathValue");
            
            if (bspFullPathTypeProp != null)
                EditorGUILayout.PropertyField(bspFullPathTypeProp, new UnityEngine.GUIContent("경로 타입"));
            if (bspFullPathValueProp != null)
                EditorGUILayout.PropertyField(bspFullPathValueProp, new UnityEngine.GUIContent("경로 생성 확률"));
                
            EditorGUI.indentLevel--;
        }
        
        GUILayout.Space(20);
        
        // 액션 버튼들
        EditorGUILayout.LabelField("맵 생성 액션", EditorStyles.boldLabel);
        
        // 메인 생성 버튼들
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("현재 설정으로 생성", GUILayout.Height(30)))
        {
            factory.GenerateMap();
        }
        if (GUILayout.Button("자동 모드로 생성", GUILayout.Height(30)))
        {
            factory.GenerateAutoSelectedMap();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("랜덤 생성", GUILayout.Height(30)))
        {
            factory.GenerateRandomMap();
        }
        if (GUILayout.Button("생성기 정리", GUILayout.Height(30)))
        {
            factory.ClearAllGenerators();
        }
        EditorGUILayout.EndHorizontal();
        
        // 맵 관리 버튼들
        if (factory.IsMapGenerated())
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("맵 관리", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("맵 제거", GUILayout.Height(25)))
            {
                factory.ClearMap();
            }
            if (GUILayout.Button("맵 재생성", GUILayout.Height(25)))
            {
                factory.RegenerateMap();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("자동 모드로 재생성", GUILayout.Height(25)))
            {
                factory.RegenerateAutoSelectedMap();
            }
            if (GUILayout.Button("랜덤으로 재생성", GUILayout.Height(25)))
            {
                factory.GenerateRandomMap();
            }
            EditorGUILayout.EndHorizontal();
        }
        
        GUILayout.Space(10);
        
        // 상태 표시
        EditorGUILayout.LabelField("시스템 상태", EditorStyles.boldLabel);
        
        // 현재 설정 표시
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("현재 설정", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.LabelField($"선택된 맵 타입: {factory.CurrentGeneratorType}");
        EditorGUILayout.LabelField($"자동 생성 모드: {factory.GetAutoMapGenerationMode()}");
        
        if (factory.GetAutoMapGenerationMode() != AutoMapGenerationMode.UseCurrentType)
        {
            MapGeneratorType nextType = factory.GetNextAutoMapType();
            EditorGUILayout.LabelField($"다음 생성될 타입: {nextType}");
        }
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
        
        // 맵 상태 표시
        if (factory.IsMapGenerated())
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("맵이 생성되었습니다.", MessageType.Info);
            
            // 맵 데이터 정보 표시
            var mapData = factory.GetCurrentMapData();
            if (mapData != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField($"그리드 크기: {mapData.gridSize.x} x {mapData.gridSize.y}");
                EditorGUILayout.LabelField($"방 개수: {mapData.roomCount}");
                EditorGUILayout.LabelField($"시드: {mapData.seed}");
                EditorGUI.indentLevel--;
                
                // 웨이포인트 정보
                var waypointData = factory.GetCurrentWaypointSystemData();
                if (waypointData != null)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField($"웨이포인트: {waypointData.waypoints?.Count ?? 0}개");
                    EditorGUILayout.LabelField($"패트롤 경로: {waypointData.patrolRoutes?.Count ?? 0}개");
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("⚠️ 맵이 생성되지 않았습니다.", MessageType.Warning);
        }
        
        // 현재 생성기 정보 표시
        if (factory.CurrentGenerator != null)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("생성기 정보", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField($"타입: {factory.CurrentGenerator.GetType().Name}");
            
            // 생성기 상태 확인
            if (factory.CurrentGenerator.HasGeneratedMap())
            {
                EditorGUILayout.LabelField("상태: ✅ 활성화됨");
            }
            else
            {
                EditorGUILayout.LabelField("상태: ⚠️ 대기 중");
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
        
        // 변경사항 적용
        serializedObject.ApplyModifiedProperties();
    }
    
    /// <summary>
    /// 랜덤 타입 프리셋을 설정하는 헬퍼 메서드
    /// </summary>
    private void SetRandomTypesPreset(SerializedProperty randomTypesProp, MapGeneratorType[] types)
    {
        randomTypesProp.ClearArray();
        randomTypesProp.arraySize = types.Length;
        
        for (int i = 0; i < types.Length; i++)
        {
            var element = randomTypesProp.GetArrayElementAtIndex(i);
            element.enumValueIndex = (int)types[i];
        }
    }
}
#endif
