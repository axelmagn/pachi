shader_type canvas_item;

uniform vec4 fill_color: source_color = vec4(1.0, 1.0, 1.0, 1.0);

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	// Called for every pixel the material is visible on
	vec2 uv = UV.xy * TEXTURE_PIXEL_SIZE.xy * 2.0 - 1.0;
	float a = uv.x * uv.x + uv.y * uv.y <= 1.0 ? 1.0 : 0.0;
	COLOR = fill_color;
	COLOR.a *= a;
	if (abs(uv.x) < 0.1) COLOR.rgb = vec3(1.0, 0.4, 0.4);
	if (abs(uv.y) < 0.1) COLOR.rgb = vec3(0.4, 0.4, 1.0);
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
