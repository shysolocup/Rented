shader_type canvas_item;

void fragment() {
	vec2 pos = UV;
	pos.x = pos.x + TIME * -0.10;
	pos.y = pos.y + TIME * -0.10;
	COLOR = texture(TEXTURE, pos);
}