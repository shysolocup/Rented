@tool
extends EditorPlugin

class_name dc_entry

static var gamenode;
var dock

func _enter_tree():
	scene_changed.connect(func(new_root:Node):
		if new_root:
			gamenode = new_root;
	)
	# Add autoloads
	add_autoload_singleton("debug_console", "res://addons/copper_dc/debug_console.tscn")

func _exit_tree():
	# Remove autoloads
	remove_autoload_singleton("debug_console")
