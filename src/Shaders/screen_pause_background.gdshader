shader_type canvas_item;
render_mode blend_sub;

uniform vec2 center = vec2(50.0, 50.0); // Set this to your polygon's center in local space.
uniform float amplitude : hint_range(0.0, 1.0) = 0.1;
uniform float frequency = 1.0;

void vertex() {
    // Translate vertex position so the center is at (0, 0).
    vec2 pos = VERTEX - center;

    // Compute the angle of the vertex relative to the center.
    // This angle will be used to offset the sine wave phase.
    float angle = atan(pos.y, pos.x);

    // Create a sine wave where each vertex is offset by its angle.
    // You can adjust the formula as desired.
    float wave = sin(TIME * frequency - angle);

    // Use the wave value to scale the vertex's distance from the center.
    pos *= 1.0 + amplitude * wave;

    // Move the vertex back to the original coordinate space.
    VERTEX = pos + center;
}
