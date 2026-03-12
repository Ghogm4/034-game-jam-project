shader_type canvas_item;

// 描边颜色
uniform vec4 outline_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);
// 描边宽度
uniform float width : hint_range(0.0, 30.0) = 1.0;
uniform float detail : hint_range(0.1, 0.523598) = 0.25;
void fragment() {
    // 获取当前像素的颜色和透明度
    vec4 sprite_color = texture(TEXTURE, UV);
    float alpha = sprite_color.a;
    
    // 如果已经是完全不透明的内部像素，直接跳过计算以节省性能
    if (alpha < 1.0 && width > 0.0) {
        vec2 pixel_size = TEXTURE_PIXEL_SIZE;
        float max_alpha = alpha;
        
        // 环绕当前像素，进行 12 个方向的圆形采样
        // 6.2831853 是 2 * PI (360度), 0.523598 是 30 度的弧度
        for (float i = 0.0; i < 6.2831853; i += detail) {
            vec2 offset = vec2(cos(i), sin(i)) * width * pixel_size;
            float neighbor_alpha = texture(TEXTURE, UV + offset).a;
            max_alpha = max(max_alpha, neighbor_alpha);
        }
        
        // 生成描边层，并根据采样的最大透明度应用颜色
        vec4 outline = vec4(outline_color.rgb, max_alpha * outline_color.a);
        
        // 将描边与原图混合 (原图盖在描边上面)
        COLOR = mix(outline, sprite_color, sprite_color.a);
    } else {
        COLOR = sprite_color;
    }
}