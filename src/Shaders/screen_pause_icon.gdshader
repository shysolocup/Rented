// NOTE: Shader automatically converted from Godot Engine 4.4.1.stable.mono's CanvasItemMaterial.

shader_type canvas_item;
render_mode blend_sub;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;

void fragment() {
	vec4 tex_color = texture(TEXTURE, UV);

	if (tex_color.a > 0.0) {
		// Read the screen color underneath using screen UVs
		vec4 screen_color = textureLod(SCREEN_TEXTURE, SCREEN_UV, 0.0);
		COLOR *= vec4(1.5 - tex_color.rgb, screen_color.a);
	} else {
		COLOR *= vec4(0.0, 0.0, 0.0, 0.0);
	}
}