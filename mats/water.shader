shader_type canvas_item;

uniform sampler2D reflection_viewport;
uniform sampler2D normal_map;
uniform float amount : hint_range(0, 1);

uniform vec4 water_color : hint_color;
uniform vec2 distortion_scale;
uniform float distortion_intensity;
uniform float distortion_speed;

uniform float refraction_mag = 30.0;

void fragment()
{
	
	
	
	// Define the distortion from the normal map
	vec2 offset = texture(normal_map, UV).xy * amount;
	
	float distortion = texture(normal_map, UV*distortion_scale + TIME*distortion_speed).x;
	distortion -= 0.5;
	
	vec3 refraction = - texture(normal_map, UV + TIME*0.08).rgb * vec3(1.0,-1.0,1.0);
	vec4 offsetScreenRead = textureLod(SCREEN_TEXTURE, SCREEN_UV + refraction.rg/refraction_mag, 0.0);
	
	// Offset the viewport texture with the distortion
	vec4 reflection = texture(reflection_viewport, SCREEN_UV -distortion * distortion_intensity);
	vec4 color = texture(TEXTURE, UV - distortion * distortion_intensity);
	// Alpha blend the reflection with the main texture
	vec3 new_water = mix(offsetScreenRead.rgb, water_color.rgb, water_color.a);
	color.rgb = color.rgb * (1.0 - reflection.a) + reflection.rgb * reflection.a;
	color.rgb = mix(color.rgb, new_water.rgb, water_color.a);
    COLOR = color;
	



}
