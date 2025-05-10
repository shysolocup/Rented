/* Better solution (credit to Beebe Ray in comments) */
shader_type canvas_item;

uniform float squishedness = 0.0;
uniform sampler2D SCREEN_TEXTURE: hint_screen_texture;

void fragment() {
	COLOR = mix(
		vec4(vec3(0.0), 1.0),
		texture(SCREEN_TEXTURE, SCREEN_UV),
		step(squishedness * 0.5, SCREEN_UV.y) - step(1.0 - squishedness * 0.5, SCREEN_UV.y)
	);
}