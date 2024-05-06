shader_type canvas_item;

uniform vec4 color: source_color = vec4(1, 1, 1, 1);
uniform float intensity: hint_range(0.0, 1.0, 0.1) = 1;

void fragment() {
	vec4 sprite = texture(TEXTURE, UV);
	COLOR = vec4(mix(sprite.rgb, color.rgb, intensity), sprite.a);
}
