shader_type canvas_item;


uniform bool enabled = false;
uniform vec4 flash_color : hint_color = vec4(1.0); // white
uniform float flash_modifier : hint_range(0.0, 1.0) = 0.4;

void fragment() {
	vec4 color = texture(TEXTURE, UV);
	if (enabled) {
		color.rgb = mix(color.rgb, flash_color.rgb, flash_modifier);
	}
	COLOR = color;
}