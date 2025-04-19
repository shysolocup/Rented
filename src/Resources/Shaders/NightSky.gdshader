shader_type sky;

uniform sampler2D sky_tex: source_color, filter_linear, hint_default_black;
uniform vec3 sun_pos = vec3(0.0);

void sky() {
	if (AT_CUBEMAP_PASS) {
		COLOR = vec3(0.2, 0.6, 1.0);
	}
	else {
		COLOR = texture(sky_tex, SKY_COORDS).rgb;
		float ds = max(dot(normalize(sun_pos - POSITION), EYEDIR), 0.0);
		COLOR += pow(
			vec3(
				pow(ds, 40.0),
				pow(ds, 40.0),
				pow(ds, 40.0)
			),
			vec3(2.0)
		);
	}
	COLOR = texture(sky_tex, SKY_COORDS).rgb;
	// Called for every visible pixel in the sky background, as well as all pixels
	// in the radiance cubemap.
}
