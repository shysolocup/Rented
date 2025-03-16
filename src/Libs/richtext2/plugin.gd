@tool
extends EditorPlugin

func _enter_tree() -> void:
	FontHelper.new()
	var fonts := {}
	FontHelper._scan_for_fonts(fonts, "res://src/Fonts", true)
	ProjectSettings.set("richer_text_label/fonts", fonts)
	ProjectSettings.add_property_info({ "name": "richer_text_label/fonts", "type": TYPE_DICTIONARY })
	
	if not ProjectSettings.has_setting("richer_text_label/colors"):
		if Engine.get_version_info().hex >= 0x040400:
			ProjectSettings.add_property_info({ 
				"name": "richer_text_label/colors", 
				"type": TYPE_DICTIONARY,
				"hint": PROPERTY_HINT_TYPE_STRING,
				"hint_string": "%d:;%d:" % [TYPE_STRING, TYPE_COLOR]
			})
		else:
			ProjectSettings.add_property_info({
				"name": "richer_text_label/colors",
				"type": TYPE_DICTIONARY
			})

func _exit_tree() -> void:
	pass
