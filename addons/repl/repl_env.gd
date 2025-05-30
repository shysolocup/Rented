class_name ReplEnv
extends Resource

const type_names = {
	TYPE_NIL: 'null',
	TYPE_BOOL: 'bool',
	TYPE_INT: 'int',
	TYPE_FLOAT: 'float',
	TYPE_STRING: 'String',
	TYPE_VECTOR2: 'Vector2',
	TYPE_VECTOR2I: 'Vector2i',
	TYPE_RECT2: 'Rect2',
	TYPE_RECT2I: 'Rect2i',
	TYPE_VECTOR3: 'Vector3',
	TYPE_VECTOR3I: 'Vector3i',
	TYPE_TRANSFORM2D: 'Transform2D',
	TYPE_VECTOR4: 'Vector4',
	TYPE_VECTOR4I: 'Vector4i',
	TYPE_PLANE: 'Plane',
	TYPE_QUATERNION: 'Quaternion',
	TYPE_AABB: 'AABB',
	TYPE_BASIS: 'Basis',
	TYPE_TRANSFORM3D: 'Transform3D',
	TYPE_PROJECTION: 'Projection',
	TYPE_COLOR: 'Color',
	TYPE_STRING_NAME: 'StringName',
	TYPE_NODE_PATH: 'NodePath',
	TYPE_RID: 'RID',
	TYPE_OBJECT: 'Object',
	TYPE_CALLABLE: 'Callable',
	TYPE_SIGNAL: 'Signal',
	TYPE_DICTIONARY: 'Dictionary',
	TYPE_ARRAY: 'Array',
	TYPE_PACKED_BYTE_ARRAY: 'PackedByteArray',
	TYPE_PACKED_INT32_ARRAY: 'PackedInt32Array',
	TYPE_PACKED_INT64_ARRAY: 'PackedInt64Array',
	TYPE_PACKED_FLOAT32_ARRAY: 'PackedFloat32Array',
	TYPE_PACKED_FLOAT64_ARRAY: 'PackedFloat64Array',
	TYPE_PACKED_STRING_ARRAY: 'PackedStringArray',
	TYPE_PACKED_VECTOR2_ARRAY: 'PackedVector2Array',
	TYPE_PACKED_VECTOR3_ARRAY: 'PackedVector3Array',
	TYPE_PACKED_COLOR_ARRAY: 'PackedColorArray'
}

const forbidden_classes = {
	## These classes create compile errors on plugin load.
	'GDScriptLanguageProtocol': null,
	'GDScriptNativeClass': null,
	'GDScriptTextDocument': null,
	'GDScriptFunctionState': null,
	'GDScriptWorkspace': null,
	'GodotPhysicsDirectSpaceState2D': null,
	'SceneCacheInterface': null,
	'SceneReplicationInterface': null,
	'SceneRPCInterface': null,
	'ThemeContext': null,
	
	## these classes create parse errors on plugin load.
	## With ClassDB.can_instantiate(..) in 4.3, they don't load anyway.
	## TODO: Test whether these load in 4.2 and remove this list if not.
	"AbstractPolygon2DEditor": null,
	"AbstractPolygon2DEditorPlugin": null,
	"ActionMapEditor": null,
	"AnchorPresetPicker": null,
	"AnimationBezierTrackEdit": null,
	"AnimationLibraryEditor": null,
	"AnimationNodeBlendSpace1DEditor": null,
	"AnimationNodeBlendSpace2DEditor": null,
	"AnimationNodeBlendTreeEditor": null,
	"AnimationNodeStateMachineEditor": null,
	"AnimationPlayerEditor": null,
	"AnimationPlayerEditorPlugin": null,
	"AnimationTimelineEdit": null,
	"AnimationTrackEditDefaultPlugin": null,
	"AnimationTrackEditPlugin": null,
	"AnimationTrackEditor": null,
	"AnimationTrackKeyEditEditorPlugin": null,
	"AnimationTreeEditor": null,
	"AnimationTreeEditorPlugin": null,
	"AnimationTreeNodeEditorPlugin": null,
	"AssetLibraryEditorPlugin": null,
	"AtlasMergingDialog": null,
	"AtlasTileProxyObject": null,
	"AudioBusesEditorPlugin": null,
	"AudioListener3DGizmoPlugin": null,
	"AudioStreamEditorPlugin": null,
	"AudioStreamImportSettings": null,
	"AudioStreamPlayer3DGizmoPlugin": null,
	"AudioStreamPreviewGenerator": null,
	"AudioStreamRandomizerEditorPlugin": null,
	"BackgroundProgress": null,
	"BitMapEditorPlugin": null,
	"BoneMapEditorPlugin": null,
	"CPUParticles2DEditorPlugin": null,
	"CPUParticles3DEditor": null,
	"CPUParticles3DEditorPlugin": null,
	"CPUParticles3DGizmoPlugin": null,
	"CSGShape3DGizmoPlugin": null,
	"Camera3DEditorPlugin": null,
	"Camera3DGizmoPlugin": null,
	"CanvasItemEditor": null,
	"CanvasItemEditorPlugin": null,
	"CanvasItemEditorSelectedItem": null,
	"CanvasItemEditorViewport": null,
	"CanvasItemMaterialConversionPlugin": null,
	"Cast2DEditor": null,
	"Cast2DEditorPlugin": null,
	"CodeTextEditor": null,
	"CollisionObject3DGizmoPlugin": null,
	"CollisionPolygon2DEditor": null,
	"CollisionPolygon2DEditorPlugin": null,
	"CollisionPolygon3DGizmoPlugin": null,
	"CollisionShape2DEditor": null,
	"CollisionShape2DEditorPlugin": null,
	"CollisionShape3DGizmoPlugin": null,
	"ConnectDialog": null,
	"ConnectDialogBinds": null,
	"ConnectionInfoDialog": null,
	"ConnectionsDock": null,
	"ControlEditorPlugin": null,
	"ControlEditorPopupButton": null,
	"ControlEditorPresetPicker": null,
	"ControlEditorToolbar": null,
	"ControlPositioningWarning": null,
	"CreateDialog": null,
	"CurveEditorPlugin": null,
	"CurvePreviewGenerator": null,
	"DebugAdapterParser": null,
	"DebugAdapterServer": null,
	"DebuggerEditorPlugin": null,
	"DecalGizmoPlugin": null,
	"DefaultThemeEditorPreview": null,
	"DependencyEditor": null,
	"DependencyEditorOwners": null,
	"DependencyErrorDialog": null,
	"DependencyRemoveDialog": null,
	"DirectoryCreateDialog": null,
	"DynamicFontImportSettings": null,
	"DynamicFontImportSettingsData": null,
	"EditorAbout": null,
	"EditorAssetLibrary": null,
	"EditorAudioBus": null,
	"EditorAudioBuses": null,
	"EditorAudioMeterNotches": null,
	"EditorAudioStreamPreviewPlugin": null,
	"EditorAutoloadSettings": null,
	"EditorBitmapPreviewPlugin": null,
	"EditorBuildProfile": null,
	"EditorBuildProfileManager": null,
	"EditorDebuggerInspector": null,
	"EditorDebuggerNode": null,
	"EditorDebuggerRemoteObject": null,
	"EditorDebuggerTree": null,
	"EditorDirDialog": null,
	"EditorExport": null,
	"EditorExportGDScript": null,
	"EditorFeatureProfileManager": null,
	"EditorFileServer": null,
	"EditorFileSystemImportFormatSupportQueryFBX": null,
	"EditorFontPreviewPlugin": null,
	"EditorGradientPreviewPlugin": null,
	"EditorHelp": null,
	"EditorHelpBit": null,
	"EditorHelpSearch": null,
	"EditorHelpTooltip": null,
	"EditorImagePreviewPlugin": null,
	"EditorImportBlendRunner": null,
	"EditorInspectorCategory": null,
	"EditorInspectorDefaultPlugin": null,
	"EditorInspectorPlugin3DTexture": null,
	"EditorInspectorPluginAnimationTrackKeyEdit": null,
	"EditorInspectorPluginAudioStream": null,
	"EditorInspectorPluginBitMap": null,
	"EditorInspectorPluginBoneMap": null,
	"EditorInspectorPluginControl": null,
	"EditorInspectorPluginCurve": null,
	"EditorInspectorPluginFontPreview": null,
	"EditorInspectorPluginFontVariation": null,
	"EditorInspectorPluginGradient": null,
	"EditorInspectorPluginGradientTexture2D": null,
	"EditorInspectorPluginInputEvent": null,
	"EditorInspectorPluginLayeredTexture": null,
	"EditorInspectorPluginMaterial": null,
	"EditorInspectorPluginMesh": null,
	"EditorInspectorPluginPackedScene": null,
	"EditorInspectorPluginSkeleton": null,
	"EditorInspectorPluginStyleBox": null,
	"EditorInspectorPluginSubViewportPreview": null,
	"EditorInspectorPluginSystemFont": null,
	"EditorInspectorPluginTexture": null,
	"EditorInspectorPluginTextureRegion": null,
	"EditorInspectorPluginTileData": null,
	"EditorInspectorRootMotionPlugin": null,
	"EditorInspectorSection": null,
	"EditorInspectorVisualShaderModePlugin": null,
	"EditorJSONSyntaxHighlighter": null,
	"EditorLayoutsDialog": null,
	"EditorLocaleDialog": null,
	"EditorLog": null,
	"EditorMaterialPreviewPlugin": null,
	"EditorMeshPreviewPlugin": null,
	"EditorNativeShaderSourceVisualizer": null,
	"EditorNetworkProfiler": null,
	"EditorNode": null,
	"EditorOBJImporter": null,
	"EditorObjectSelector": null,
	"EditorPackedScenePreviewPlugin": null,
	"EditorPerformanceProfiler": null,
	"EditorPlainTextSyntaxHighlighter": null,
	"EditorPluginCSG": null,
	"EditorPluginSettings": null,
	"EditorProfiler": null,
	"EditorPropertyAnchorsPreset": null,
	"EditorPropertyArray": null,
	"EditorPropertyArrayObject": null,
	"EditorPropertyCheck": null,
	"EditorPropertyColor": null,
	"EditorPropertyDictionaryObject": null,
	"EditorPropertyEnum": null,
	"EditorPropertyFloat": null,
	"EditorPropertyInteger": null,
	"EditorPropertyLayers": null,
	"EditorPropertyLayersGrid": null,
	"EditorPropertyLocalizableString": null,
	"EditorPropertyMultilineText": null,
	"EditorPropertyNameProcessor": null,
	"EditorPropertyNodePath": null,
	"EditorPropertyPath": null,
	"EditorPropertyResource": null,
	"EditorPropertySizeFlags": null,
	"EditorPropertyText": null,
	"EditorPropertyTextEnum": null,
	"EditorPropertyVector2": null,
	"EditorPropertyVector2i": null,
	"EditorPropertyVector3": null,
	"EditorPropertyVectorN": null,
	"EditorQuickOpen": null,
	"EditorRunBar": null,
	"EditorRunNative": null,
	"EditorSceneFormatImporterCollada": null,
	"EditorSceneFormatImporterESCN": null,
	"EditorSceneTabs": null,
	"EditorScriptPreviewPlugin": null,
	"EditorSettingsDialog": null,
	"EditorStandardSyntaxHighlighter": null,
	"EditorTexturePreviewPlugin": null,
	"EditorTextureTooltipPlugin": null,
	"EditorTheme": null,
	"EditorTitleBar": null,
	"EditorToaster": null,
	"EditorValidationPanel": null,
	"EditorVisualProfiler": null,
	"EditorZoomWidget": null,
	"EventListenerLineEdit": null,
	"ExportTemplateManager": null,
	"FBXImporterManager": null,
	"FileSystemList": null,
	"FindBar": null,
	"FindInFiles": null,
	"FindInFilesDialog": null,
	"FindInFilesPanel": null,
	"FindReplaceBar": null,
	"FogMaterialConversionPlugin": null,
	"FogVolumeGizmoPlugin": null,
	"FontEditorPlugin": null,
	"GDScriptLanguageServer": null,
	"GDScriptSyntaxHighlighter": null,
	"GPUParticles2DEditorPlugin": null,
	"GPUParticles3DEditor": null,
	"GPUParticles3DEditorBase": null,
	"GPUParticles3DEditorPlugin": null,
	"GPUParticles3DGizmoPlugin": null,
	"GPUParticlesCollision3DGizmoPlugin": null,
	"GPUParticlesCollisionSDF3DEditorPlugin": null,
	"Gizmo3DHelper": null,
	"GotoLineDialog": null,
	"GradientEditorPlugin": null,
	"GradientTexture2DEditorPlugin": null,
	"GraphEditFilter": null,
	"GraphEditMinimap": null,
	"GridMapEditor": null,
	"GridMapEditorPlugin": null,
	"GroupDialog": null,
	"GroupsEditor": null,
	"HistoryDock": null,
	"ImportDefaultsEditor": null,
	"ImportDefaultsEditorSettings": null,
	"ImportDock": null,
	"ImportDockParameters": null,
	"InputEventConfigurationDialog": null,
	"InputEventEditorPlugin": null,
	"InspectorDock": null,
	"Joint3DGizmoPlugin": null,
	"Label3DGizmoPlugin": null,
	"Light3DGizmoPlugin": null,
	"LightOccluder2DEditor": null,
	"LightOccluder2DEditorPlugin": null,
	"LightmapGIEditorPlugin": null,
	"LightmapGIGizmoPlugin": null,
	"LightmapProbeGizmoPlugin": null,
	"Line2DEditor": null,
	"Line2DEditorPlugin": null,
	"LocalizationEditor": null,
	"Marker3DGizmoPlugin": null,
	"MaterialEditorPlugin": null,
	"MeshEditorPlugin": null,
	"MeshInstance3DEditor": null,
	"MeshInstance3DEditorPlugin": null,
	"MeshInstance3DGizmoPlugin": null,
	"MeshLibraryEditor": null,
	"MeshLibraryEditorPlugin": null,
	"MultiMeshEditor": null,
	"MultiMeshEditorPlugin": null,
	"MultiplayerEditorDebugger": null,
	"MultiplayerEditorPlugin": null,
	"NavigationLink2DEditor": null,
	"NavigationLink2DEditorPlugin": null,
	"NavigationLink3DGizmoPlugin": null,
	"NavigationMeshEditor": null,
	"NavigationMeshEditorPlugin": null,
	"NavigationObstacle2DEditor": null,
	"NavigationObstacle2DEditorPlugin": null,
	"NavigationObstacle3DEditor": null,
	"NavigationObstacle3DEditorPlugin": null,
	"NavigationPolygonEditor": null,
	"NavigationPolygonEditorPlugin": null,
	"NavigationRegion3DGizmoPlugin": null,
	"Node3DEditor": null,
	"Node3DEditorPlugin": null,
	"Node3DEditorViewport": null,
	"Node3DEditorViewportContainer": null,
	"NodeDock": null,
	"NoiseEditorInspectorPlugin": null,
	"NoiseEditorPlugin": null,
	"ORMMaterial3DConversionPlugin": null,
	"OccluderInstance3DEditorPlugin": null,
	"OccluderInstance3DGizmoPlugin": null,
	"OrphanResourcesDialog": null,
	"PackedSceneEditorPlugin": null,
	"PackedSceneEditorTranslationParserPlugin": null,
	"PanoramaSkyMaterialConversionPlugin": null,
	"ParticleProcessMaterialConversionPlugin": null,
	"Path2DEditor": null,
	"Path2DEditorPlugin": null,
	"Path3DEditorPlugin": null,
	"Path3DGizmoPlugin": null,
	"PhysicalBone3DEditorPlugin": null,
	"PhysicalBone3DGizmoPlugin": null,
	"PhysicalSkyMaterialConversionPlugin": null,
	"PluginConfigDialog": null,
	"Polygon2DEditor": null,
	"Polygon2DEditorPlugin": null,
	"Polygon3DEditor": null,
	"Polygon3DEditorPlugin": null,
	"PostImportPluginSkeletonRenamer": null,
	"PostImportPluginSkeletonRestFixer": null,
	"PostImportPluginSkeletonTrackOrganizer": null,
	"ProceduralSkyMaterialConversionPlugin": null,
	"ProgressDialog": null,
	"ProjectExportDialog": null,
	"ProjectExportTextureFormatError": null,
	"ProjectSettingsEditor": null,
	"PropertySelector": null,
	"RayCast3DGizmoPlugin": null,
	"ReflectionProbeGizmoPlugin": null,
	"RenameDialog": null,
	"RenderSceneBuffersGLES3": null,
	"ReparentDialog": null,
	"ReplicationEditor": null,
	"ResourcePreloaderEditor": null,
	"ResourcePreloaderEditorPlugin": null,
	"SceneCreateDialog": null,
	"SceneExporterGLTFPlugin": null,
	"SceneImportSettings": null,
	"SceneImportSettingsData": null,
	"SceneTileProxyObject": null,
	"SceneTreeDialog": null,
	"SceneTreeDock": null,
	"SceneTreeEditor": null,
	"ScreenSelect": null,
	"ScriptEditorDebugger": null,
	"ScriptEditorPlugin": null,
	"ScriptEditorQuickOpen": null,
	"ScriptTextEditor": null,
	"SectionedInspector": null,
	"SectionedInspectorFilter": null,
	"ShaderCreateDialog": null,
	"ShaderEditorPlugin": null,
	"ShaderFileEditor": null,
	"ShaderFileEditorPlugin": null,
	"ShaderGlobalsEditor": null,
	"ShaderGlobalsEditorInterface": null,
	"ShapeCast3DGizmoPlugin": null,
	"ShortcutBin": null,
	"SizeFlagPresetPicker": null,
	"Skeleton2DEditor": null,
	"Skeleton2DEditorPlugin": null,
	"Skeleton3DEditorPlugin": null,
	"Skeleton3DGizmoPlugin": null,
	"SkeletonIK3DEditorPlugin": null,
	"SnapDialog": null,
	"SoftBody3DGizmoPlugin": null,
	"SplitContainerDragger": null,
	"SpringArm3DGizmoPlugin": null,
	"Sprite2DEditor": null,
	"Sprite2DEditorPlugin": null,
	"SpriteBase3DGizmoPlugin": null,
	"SpriteFramesEditor": null,
	"SpriteFramesEditorPlugin": null,
	"StandardMaterial3DConversionPlugin": null,
	"StyleBoxEditorPlugin": null,
	"SubViewportPreviewEditorPlugin": null,
	"SurfaceUpgradeDialog": null,
	"SurfaceUpgradeTool": null,
	"TextEditor": null,
	"TextFile": null,
	"Texture3DEditorPlugin": null,
	"TextureEditorPlugin": null,
	"TextureLayeredEditorPlugin": null,
	"TextureRegionEditor": null,
	"TextureRegionEditorPlugin": null,
	"ThemeEditor": null,
	"ThemeEditorPlugin": null,
	"ThemeEditorPreview": null,
	"ThemeItemEditorDialog": null,
	"ThemeItemImportTree": null,
	"ThemeTypeDialog": null,
	"ThemeTypeEditor": null,
	"TileAtlasView": null,
	"TileMapEditor": null,
	"TileMapEditorPlugin": null,
	"TileMapEditorTerrainsPlugin": null,
	"TileMapEditorTilesPlugin": null,
	"TileProxiesManagerDialog": null,
	"TileSetAtlasSourceEditor": null,
	"TileSetAtlasSourceProxyObject": null,
	"TileSetEditor": null,
	"TileSetEditorPlugin": null,
	"TileSetScenesCollectionProxyObject": null,
	"TileSetScenesCollectionSourceEditor": null,
	"TileSourceInspectorPlugin": null,
	"TilesEditorUtils": null,
	"VehicleWheel3DGizmoPlugin": null,
	"VersionControlEditorPlugin": null,
	"ViewPanner": null,
	"ViewportNavigationControl": null,
	"ViewportRotationControl": null,
	"VisibleOnScreenNotifier3DGizmoPlugin": null,
	"VisualShaderConversionPlugin": null,
	"VoxelGIEditorPlugin": null,
	"VoxelGIGizmoPlugin": null,
	"WindowWrapper": null,
	
	# Godot 4.3 (parse errors)
	"AudioStreamImportSettingsDialog": null,
	"AudioStreamInteractiveEditorPlugin": null,
	"AudioStreamInteractiveTransitionEditor": null,
	"AudioStreamPlayerInternal": null,
	"DockContextPopup": null,
	"DockSplitContainer": null,
	"DynamicFontImportSettingsDialog": null,
	"EditorAudioStreamTooltipPlugin": null,
	"EditorBottomPanel": null,
	"EditorDockManager": null,
	"EditorFileSystemImportFormatSupportQueryBlend": null,
	"EditorInspectorParticleProcessMaterialPlugin": null,
	"EditorInspectorPluginAudioStreamInteractive": null,
	"EditorSceneExporterGLTFSettings": null,
	"GeometryInstance3DGizmoPlugin": null,
	"GroupSettingsEditor": null,
	"ParallaxBackgroundEditorPlugin": null,
	"RunInstancesDialog": null,
	"SceneImportSettingsDialog": null,
	"TileMapLayerEditor": null,
	"TileMapLayerEditorTerrainsPlugin": null,
	"TileMapLayerEditorTilesPlugin": null,
	"UVEditDialog": null,
}

# handles eval calls
var eval_script = GDScript.new()
var vars


func _init(_vars={}):
	# store singletons for lookup later
	var singletons = {}
	for singleton in Engine.get_singleton_list():
		singletons[singleton] = null
	
	for constant in ReplConstants.constants:
		if constant not in _vars:
			_vars[constant] = ReplConstants.constants[constant]
	
	vars = _vars
	for clazz in ClassDB.get_class_list():
		if clazz in vars:
			continue
		if not ClassDB.can_instantiate(clazz) and not singletons.has(clazz):
			continue
		if clazz in forbidden_classes:
			continue
		var result = eval_label(clazz)
		if result[0]:  # error
			continue
		vars[clazz] = result[1]
	
	var autoloads = read_autoloads()
	for clazz in autoloads:
		vars[clazz] = autoloads[clazz]


func read_autoloads():
	var classes = {}
	var f = FileAccess.open("res://addons/repl/autoloads.txt", FileAccess.READ)
	if f == null:
		push_warning("Could not find res://addons/repl/autoloads.txt. Was it deleted?")
		return classes
		
	while not f.eof_reached():
		var line = f.get_line()
		if line.length() == 0:
			continue
		if line.substr(0, 1) == '#':
			continue
		var script_path = line.strip_edges(true, true)
		var script = load(script_path)
		var clazz_name = script.get_global_name()
		if clazz_name == '':
			push_warning("REPL autoloaded a script with no class_name at " + script_path)
			push_warning("Without a class_name, the script can't be referenced in the REPL.")
			continue
		classes[clazz_name] = script
	f.close()
	return classes


func type_name(val):
	var name = type_names[typeof(val)]
	if name == 'Object':
		return val.get_class()
	return name


func dir(obj):
	match typeof(obj):
		TYPE_CALLABLE:
			var name = obj.get_method()
			if name == null:
				return obj
			
			for method in obj.get_object().get_method_list():
				if method['name'] == name:
					return method
			return obj
			
		TYPE_OBJECT:
			var items = {
				'class_name': obj.get_class(),
				'properties':[],
				'signals':[],
				'methods':[],
			}
			for property in obj.get_property_list():
				items['properties'].append(property['name'])
			for zignal in obj.get_signal_list():
				items['signals'].append(zignal['name'])
			for method in obj.get_method_list():
				items['methods'].append(method['name'])
			return items
		_:
			return obj

func pprint(obj):
	## pretty print
	return JSON.stringify(obj, '  ')

func eval_label(label:String):
	## load a script into memory just to get the Type/Enum/whatever
	var contents = "static func eval(): return %s" % label
	eval_script.set_source_code(contents)
	var error := eval_script.reload()
	if(error == OK):
		return [false, eval_script.eval()]
	else:
		return [true, "Identifier '%s' not declared in the current scope." % label]


func char(char):
	return char(char)

func convert(what, type):
	return convert(what, type)

func dict_to_inst(dictionary):
	return dict_to_inst(dictionary)

func get_stack():
	return get_stack()

func inst_to_dict(instance):
	return inst_to_dict(instance)

func is_instance_of(value, type):
	return is_instance_of(value, type)

func len(variant):
	return len(variant)

func load(path):
	return load(path)

func print_stack():
	print_stack()

func range(a, b=null, c=null):
	if b == null:
		return range(a)
	if c == null:
		return range(a, b)
	return range(a, b, c)

func type_exists(type):
	return type_exists(type)
