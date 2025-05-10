shader_type canvas_item;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;
uniform float brightness : hint_range(0.0, 2.0) = 1.0;
uniform float contrast : hint_range(0.0, 2.0) = 1.0;
uniform float saturation : hint_range(0.0, 2.0) = 1.0;
uniform float gamma : hint_range(0.1, 2.2) = 1.0;
uniform float bloom_intensity : hint_range(0.0, 1.0) = 0.2;
uniform float bloom_threshold : hint_range(0.0, 1.0) = 0.6;
uniform float bloom_blur : hint_range(0.0, 10.0) = 4.0;
uniform vec4 tint_color : source_color = vec4(1.0, 0.97, 0.9, 1.0);
uniform float tint_strength : hint_range(0.0, 1.0) = 0.2;
uniform float vignette_intensity : hint_range(0.0, 1.0) = 0.2;
uniform float vignette_opacity : hint_range(0.0, 1.0) = 0.5;
uniform float vignette_roundness : hint_range(0.0, 2.0) = 1.0;
uniform vec4 vignette_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);
uniform float film_grain : hint_range(0.0, 0.5) = 0.03;
uniform float film_grain_speed : hint_range(0.0, 10.0) = 1.0;
uniform float chromatic_aberration : hint_range(0.0, 5.0) = 0.5;
uniform bool enable_god_rays = true;
uniform float god_rays_intensity : hint_range(0.0, 1.0) = 0.1;
uniform vec2 light_position = vec2(0.5, 0.5);
uniform bool enable_lens_dirt = true;
uniform sampler2D lens_dirt_texture : hint_default_white;
uniform float lens_dirt_intensity : hint_range(0.0, 1.0) = 0.5;
uniform bool enable_color_grading = true;
uniform sampler2D color_lut : hint_default_white;
uniform float color_lut_mix : hint_range(0.0, 1.0) = 0.8;
uniform bool enable_dithering = true;
uniform float dithering_intensity : hint_range(0.0, 0.1) = 0.02;
uniform bool enable_scanlines = false;
uniform float scanlines_opacity : hint_range(0.0, 1.0) = 0.2;
uniform float scanlines_width : hint_range(1.0, 20.0) = 10.0;
uniform bool enable_sharpen = true;
uniform float sharpen_intensity : hint_range(0.0, 5.0) = 0.5;
uniform bool enable_color_blindness = false;
uniform int color_blindness_type : hint_range(0, 2) = 0;
uniform float color_blindness_intensity : hint_range(0.0, 1.0) = 1.0;
uniform bool enable_pixelate = false;
uniform float pixelate_amount : hint_range(1.0, 512.0) = 64.0;
uniform bool enable_edge_detection = false;
uniform float edge_detection_intensity : hint_range(0.0, 5.0) = 1.0;
uniform vec4 edge_detection_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);
uniform bool enable_night_vision = false;
uniform vec4 night_vision_color : source_color = vec4(0.0, 1.0, 0.0, 1.0);
uniform float night_vision_noise : hint_range(0.0, 1.0) = 0.3;

vec3 rgb_to_hsv(vec3 rgb) {
    float cmax = max(rgb.r, max(rgb.g, rgb.b));
    float cmin = min(rgb.r, min(rgb.g, rgb.b));
    float delta = cmax - cmin;

    vec3 hsv = vec3(0.0, 0.0, cmax);

    if (delta > 0.0001) {
        hsv.y = delta / cmax;

        if (abs(cmax - rgb.r) < 0.0001) {
            hsv.x = (rgb.g - rgb.b) / delta + (rgb.g < rgb.b ? 6.0 : 0.0);
        } else if (abs(cmax - rgb.g) < 0.0001) {
            hsv.x = (rgb.b - rgb.r) / delta + 2.0;
        } else {
            hsv.x = (rgb.r - rgb.g) / delta + 4.0;
        }

        hsv.x /= 6.0;
    }

    return hsv;
}

vec3 hsv_to_rgb(vec3 hsv) {
    if (abs(hsv.y) < 0.0001) {
        return vec3(hsv.z);
    }

    float h = hsv.x * 6.0;
    int i = int(floor(h));
    float f = h - float(i);
    float p = hsv.z * (1.0 - hsv.y);
    float q = hsv.z * (1.0 - hsv.y * f);
    float t = hsv.z * (1.0 - hsv.y * (1.0 - f));

    if (i == 0) return vec3(hsv.z, t, p);
    else if (i == 1) return vec3(q, hsv.z, p);
    else if (i == 2) return vec3(p, hsv.z, t);
    else if (i == 3) return vec3(p, q, hsv.z);
    else if (i == 4) return vec3(t, p, hsv.z);
    else return vec3(hsv.z, p, q);
}

vec3 blur(sampler2D tex, vec2 uv, vec2 resolution, float radius) {
    vec3 color = vec3(0.0);
    float total = 0.0;

    for (float x = -radius; x <= radius; x += 1.0) {
        for (float y = -radius; y <= radius; y += 1.0) {
            vec2 offset = vec2(x, y) / resolution;
            float weight = 1.0 - length(offset) / radius;
            if (weight < 0.0) weight = 0.0;
            color += texture(tex, uv + offset).rgb * weight;
            total += weight;
        }
    }

    return color / total;
}

float random(vec2 uv) {
    return fract(sin(dot(uv, vec2(12.9898, 78.233))) * 43758.5453123);
}

vec3 god_rays(sampler2D tex, vec2 uv, vec2 light_pos, float exposure) {
    vec3 color = vec3(0.0);
    float decay = 0.97;
    float density = 0.5;
    float weight = 0.3;

    vec2 delta = (uv - light_pos) * density / 100.0;
    vec2 pos = uv;

    for (int i = 0; i < 100; i++) {
        pos -= delta;
        vec3 sample_color = texture(tex, pos).rgb;
        sample_color *= weight * exposure;
        color += sample_color;
        weight *= decay;
    }

    return color;
}

float apply_dither_effect(vec2 uv, float color_value) {
    float bayer[16] = float[16](
        0.0, 8.0, 2.0, 10.0,
        12.0, 4.0, 14.0, 6.0,
        3.0, 11.0, 1.0, 9.0,
        15.0, 7.0, 13.0, 5.0
    );

    int x = int(mod(uv.x, 4.0));
    int y = int(mod(uv.y, 4.0));
    float dither_value = bayer[y * 4 + x] / 16.0 - 0.5;

    return color_value + dither_value * dithering_intensity;
}

vec3 apply_color_blindness(vec3 color, int type, float intensity) {
    mat3 protanopia = mat3(
        vec3(0.567, 0.433, 0.0),
        vec3(0.558, 0.442, 0.0),
        vec3(0.0, 0.242, 0.758)
    );

    mat3 deuteranopia = mat3(
        vec3(0.625, 0.375, 0.0),
        vec3(0.7, 0.3, 0.0),
        vec3(0.0, 0.3, 0.7)
    );

    mat3 tritanopia = mat3(
        vec3(0.95, 0.05, 0.0),
        vec3(0.0, 0.433, 0.567),
        vec3(0.0, 0.475, 0.525)
    );

    mat3 matrix;
    if (type == 0) matrix = protanopia;
    else if (type == 1) matrix = deuteranopia;
    else matrix = tritanopia;

    vec3 blind_color = matrix * color;
    return mix(color, blind_color, intensity);
}

float edge_detection(sampler2D tex, vec2 uv, vec2 texel_size) {
    float offset = 1.0;

    vec3 a = texture(tex, uv + texel_size * vec2(-offset, -offset)).rgb;
    vec3 b = texture(tex, uv + texel_size * vec2(0.0, -offset)).rgb;
    vec3 c = texture(tex, uv + texel_size * vec2(offset, -offset)).rgb;
    vec3 d = texture(tex, uv + texel_size * vec2(-offset, 0.0)).rgb;
    vec3 f = texture(tex, uv + texel_size * vec2(offset, 0.0)).rgb;
    vec3 g = texture(tex, uv + texel_size * vec2(-offset, offset)).rgb;
    vec3 h = texture(tex, uv + texel_size * vec2(0.0, offset)).rgb;
    vec3 i = texture(tex, uv + texel_size * vec2(offset, offset)).rgb;

    vec3 sobel_x = -a - 2.0 * d - g + c + 2.0 * f + i;
    vec3 sobel_y = -a - 2.0 * b - c + g + 2.0 * h + i;

    float edge = length(sobel_x) + length(sobel_y);
    return edge;
}

vec3 sharpen(sampler2D tex, vec2 uv, vec2 texel_size, float intensity) {
    vec3 center = texture(tex, uv).rgb;
    vec3 top = texture(tex, uv + vec2(0.0, -texel_size.y)).rgb;
    vec3 bottom = texture(tex, uv + vec2(0.0, texel_size.y)).rgb;
    vec3 left = texture(tex, uv + vec2(-texel_size.x, 0.0)).rgb;
    vec3 right = texture(tex, uv + vec2(texel_size.x, 0.0)).rgb;

    vec3 sharpen_color = center * (1.0 + 4.0 * intensity) - (top + bottom + left + right) * intensity;
    return sharpen_color;
}

vec3 apply_lut(sampler2D lut_texture, vec3 color) {
    float blue = color.b * 63.0;
    vec2 quad1 = vec2(0.0);
    quad1.y = floor(floor(blue) / 8.0);
    quad1.x = floor(blue) - (quad1.y * 8.0);

    vec2 quad2 = vec2(0.0);
    quad2.y = floor(ceil(blue) / 8.0);
    quad2.x = ceil(blue) - (quad2.y * 8.0);

    vec2 texPos1 = vec2(0.0);
    texPos1.x = (quad1.x * 0.125) + 0.0625 + (0.125 * color.r);
    texPos1.y = (quad1.y * 0.125) + 0.0625 + (0.125 * color.g);

    vec2 texPos2 = vec2(0.0);
    texPos2.x = (quad2.x * 0.125) + 0.0625 + (0.125 * color.r);
    texPos2.y = (quad2.y * 0.125) + 0.0625 + (0.125 * color.g);

    vec3 newColor1 = texture(lut_texture, texPos1).rgb;
    vec3 newColor2 = texture(lut_texture, texPos2).rgb;

    vec3 newColor = mix(newColor1, newColor2, fract(blue));
    return newColor;
}

void fragment() {
    vec2 uv = SCREEN_UV;
    vec2 pixel_size = 1.0 / SCREEN_PIXEL_SIZE;

    if (enable_pixelate) {
        vec2 pixelated_uv = floor(uv * pixelate_amount) / pixelate_amount;
        uv = pixelated_uv;
    }

    vec3 color;
    if (chromatic_aberration > 0.0001) {
        vec2 direction = (uv - 0.5) * 2.0;
        vec2 distortion = direction * chromatic_aberration / 1000.0;

        color.r = texture(SCREEN_TEXTURE, uv - distortion).r;
        color.g = texture(SCREEN_TEXTURE, uv).g;
        color.b = texture(SCREEN_TEXTURE, uv + distortion).b;
    } else {
        color = texture(SCREEN_TEXTURE, uv).rgb;
    }

    if (enable_sharpen) {
        color = sharpen(SCREEN_TEXTURE, uv, 1.0 / pixel_size, sharpen_intensity);
    }

    if (bloom_intensity > 0.0001) {
        vec3 bloom_color = blur(SCREEN_TEXTURE, uv, pixel_size, bloom_blur);
        bloom_color = max(bloom_color - bloom_threshold, 0.0);
        color += bloom_color * bloom_intensity;

        if (enable_lens_dirt && textureSize(lens_dirt_texture, 0).x > 1) {
            vec3 dirt = texture(lens_dirt_texture, uv).rgb;
            color += bloom_color * dirt * lens_dirt_intensity;
        }
    }

    if (enable_god_rays) {
        vec3 rays = god_rays(SCREEN_TEXTURE, uv, light_position, god_rays_intensity * 0.1);
        color += rays;
    }

    if (enable_edge_detection) {
        float edge = edge_detection(SCREEN_TEXTURE, uv, 1.0 / pixel_size);
        color = mix(color, edge_detection_color.rgb, edge * edge_detection_intensity);
    }

    color = (color - 0.5) * contrast + 0.5;
    color *= brightness;

    vec3 hsv = rgb_to_hsv(color);
    hsv.y *= saturation;
    color = hsv_to_rgb(hsv);

    color = pow(color, vec3(1.0 / gamma));

    color = mix(color, color * tint_color.rgb, tint_strength);

    if (enable_color_grading && textureSize(color_lut, 0).x > 1) {
        vec3 graded_color = apply_lut(color_lut, color);
        color = mix(color, graded_color, color_lut_mix);
    }

    if (vignette_intensity > 0.0001) {
        vec2 vignette_uv = uv * (1.0 - uv.yx);
        float vig = vignette_uv.x * vignette_uv.y * 15.0;
        vig = pow(vig, vignette_roundness);
        color = mix(color, mix(color, vignette_color.rgb, vignette_opacity), vignette_intensity * (1.0 - vig));
    }

    if (film_grain > 0.0001) {
        float grain = random(uv + vec2(TIME * film_grain_speed * 0.01, TIME * film_grain_speed * 0.01)) * film_grain;
        color += grain - film_grain/2.0;
    }

    if (enable_scanlines) {
        float scanline = sin(uv.y * scanlines_width * pixel_size.y) * 0.5 + 0.5;
        scanline = pow(scanline, 1.0) * scanlines_opacity;
        color = mix(color, color * (1.0 - scanline), scanlines_opacity);
    }

    if (enable_color_blindness) {
        color = apply_color_blindness(color, color_blindness_type, color_blindness_intensity);
    }

    if (enable_night_vision) {
        float luminance = dot(color, vec3(0.299, 0.587, 0.114));
        vec3 night_color = vec3(luminance) * night_vision_color.rgb;

        float noise = random(uv + vec2(TIME * 0.1, TIME * 0.1)) * night_vision_noise;
        night_color += vec3(noise);

        float scanline = sin(uv.y * pixel_size.y * 200.0) * 0.1 + 0.9;
        night_color *= scanline;

        color = night_color;
    }

    if (enable_dithering) {
        color.r = apply_dither_effect(FRAGCOORD.xy, color.r);
        color.g = apply_dither_effect(FRAGCOORD.xy, color.g);
        color.b = apply_dither_effect(FRAGCOORD.xy, color.b);
    }

    COLOR = vec4(color, 1.0);
}