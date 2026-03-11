shader_type canvas_item;

// 模糊强度
uniform float blur_amount : hint_range(0.0, 5.0) = 2.0;
// 采样步长（用于控制质量）
uniform int sampling_quality : hint_range(1, 10) = 4;

// 屏幕读取（如果是给 ColorRect 用来模糊背景，需要用到这个）
uniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap;

void fragment() {
    vec4 color = vec4(0.0);
    float total_weight = 0.0;
    
    // 获取纹理像素大小
    vec2 size = SCREEN_PIXEL_SIZE;
    
    // 嵌套循环进行卷积采样
    for (int x = -sampling_quality; x <= sampling_quality; x++) {
        for (int y = -sampling_quality; y <= sampling_quality; y++) {
            // 计算偏移量
            vec2 offset = vec2(float(x), float(y)) * blur_amount * size;
            
            // 计算权重（简单的距离权重，模拟高斯分布）
            float weight = 1.0 - (length(vec2(float(x), float(y))) / (float(sampling_quality) * 1.414));
            weight = max(weight, 0.0);
            
            // 累加颜色
            color += texture(screen_texture, SCREEN_UV + offset) * weight;
            total_weight += weight;
        }
    }
    
    COLOR = color / total_weight;
}