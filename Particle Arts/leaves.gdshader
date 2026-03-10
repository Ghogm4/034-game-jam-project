shader_type particles;

uniform float emission_width = 500;

float rand(inout uint seed) {
	int k;
	int s = int(seed);
	if (s == 0) s = 305420679;
	k = s / 127773;
	s = 16807 * (s - k * 127773) - 2836 * k;
	if (s < 0) s += 2147483647;
	seed = uint(s);
	return float(seed % uint(65536)) / 65536.0;
}

float rand2(uint index, float offset) {
	return fract(sin(float(index) * 12.9898 + offset) * 43758.5453);
}

uniform float fall_speed = 80;
uniform float flutter_amplitude = 20.0;
uniform float swing_width = 120.0;
uniform float swing_freq = 2.0;
uniform float max_tilt = 0.8;
uniform float scale_min = 0.5;
uniform float scale_max = 1.5;
uniform float wind_speed_min = -10;
uniform float wind_speed_max = -40.0;



void start() {
	// Called when a particle is spawned.
	TRANSFORM = EMISSION_TRANSFORM;

	uint alt_seed = INDEX;

	float rand_pos_val = rand(alt_seed);
	float final_x = (rand_pos_val - 0.5) * emission_width;
	TRANSFORM[3].x += final_x;

	float rand_scale_val = rand(alt_seed);
	CUSTOM.z = mix(scale_min, scale_max, rand_scale_val); // Scale

	float rand_wind_val = rand(alt_seed);
	float final_wind_speed = mix(wind_speed_min, wind_speed_max, rand_wind_val);
	CUSTOM.x = final_wind_speed; // Windspeed

	CUSTOM.y = rand(alt_seed) * 2.0 * PI; // Phase

	CUSTOM.w = mix(0.8, 1.2, rand(alt_seed)); // Freq
	
	
}

void process() {
	// Called every frame on existing particles (according to the Fixed FPS property).
	float leaf_wind = CUSTOM.x;
	float leaf_scale = CUSTOM.z;
	float leaf_freq_mult = CUSTOM.w;

	float rand_amp = rand2(INDEX, 1.0);
	float rand_fall = rand2(INDEX, 2.0);
	
	float personal_swing_width = swing_width * mix(0.5, 1.5, rand_amp);
	float personal_fall_speed = fall_speed * mix(0.5, 1.5, rand_fall);

	float t = TIME * swing_freq * leaf_freq_mult + CUSTOM.y;
	float t_wobble = TIME * (swing_freq * 2.7) * leaf_freq_mult + CUSTOM.y * 1.5;

	float organic_swing = cos(t) + 0.35 * cos(t_wobble);

	VELOCITY.x = personal_swing_width * organic_swing + leaf_wind;
	VELOCITY.y = personal_fall_speed + flutter_amplitude * sin(t * 2.0);

	float angle = max_tilt * organic_swing;

	float c = cos(angle);
	float s = sin(angle);

	TRANSFORM[0].x = c;
	TRANSFORM[0].y = s;
	TRANSFORM[1].x = -s;
	TRANSFORM[1].y = c;
}
