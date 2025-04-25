shader_type spatial;

uniform float speed : hint_range(0.1, 25.0) = 1.0;
uniform float frequency: hint_range(0.1, 10.0)=1.0;
uniform float warp_amount : hint_range(0.0, 5.0) = 1.0;

vec2 warp(vec2 uv) {
    vec2 delta = uv - 0.5;
    float delta2 = dot(delta.xy, delta.xy);
    float delta4 = delta2 * delta2;
    return uv + delta * (delta4 * warp_amount);
}

void fragment() {
    float time = TIME * speed;

    // Generate rainbow colors based on the position and time
    vec3 pos = FRAGCOORD.xyz * frequency;
    float r = sin(pos.x + time) * 0.5 + 0.5;
    float g = sin(pos.x + time + 2.0) * 0.5 + 0.5;
    float b = sin(pos.x + time + 4.0) * 0.5 + 0.5;

	vec2 warpstuff = warp(vec2(r, g));

    ALBEDO = vec3(r * warpstuff.r, g * warpstuff.g, b);
}