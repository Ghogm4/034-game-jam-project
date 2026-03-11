shader_type canvas_item;
render_mode unshaded;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_linear_mipmap;

uniform float blur_lod : hint_range(0.0, 8.0, 0.1) = 2.8;
uniform float blur_radius : hint_range(0.0, 0.02, 0.0005) = 0.0035;
uniform vec4 overlay_color : source_color = vec4(0.95, 0.97, 1.0, 0.32);

vec3 sample_blur(vec2 uv, vec2 offset) {
	vec3 color = textureLod(screen_texture, uv, blur_lod).rgb * 0.227027;
	color += textureLod(screen_texture, clamp(uv + offset * 1.384615, vec2(0.0), vec2(1.0)), blur_lod).rgb * 0.316216;
	color += textureLod(screen_texture, clamp(uv - offset * 1.384615, vec2(0.0), vec2(1.0)), blur_lod).rgb * 0.316216;
	color += textureLod(screen_texture, clamp(uv + offset * 3.230769, vec2(0.0), vec2(1.0)), blur_lod).rgb * 0.070270;
	color += textureLod(screen_texture, clamp(uv - offset * 3.230769, vec2(0.0), vec2(1.0)), blur_lod).rgb * 0.070270;
	return color;
}

void fragment() {
	vec2 uv = SCREEN_UV;
	vec2 horizontal_offset = vec2(blur_radius, 0.0);
	vec2 vertical_offset = vec2(0.0, blur_radius);

	vec3 horizontal_blur = sample_blur(uv, horizontal_offset);
	vec3 vertical_blur = sample_blur(uv, vertical_offset);
	vec3 blurred = (horizontal_blur + vertical_blur) * 0.5;
	vec3 final_rgb = mix(blurred, overlay_color.rgb, overlay_color.a);

	COLOR = vec4(final_rgb, overlay_color.a);
}