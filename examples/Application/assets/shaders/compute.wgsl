struct Particle {
    position: vec2f,
    velocity: vec2f,
};

struct SceneData {
    time: f32,
    delta_time: f32,
    width: u32,
    height: u32,
}

@group(0) @binding(0) var<storage, read_write> data: array<Particle>;
@group(0) @binding(1) var<uniform> scene: SceneData;

@compute @workgroup_size(256,1,1)
fn main(
    @builtin(local_invocation_id) local_id: vec3<u32>,
    @builtin(local_invocation_index) local_index: u32,
    @builtin(global_invocation_id) global_id: vec3<u32>,
) {
    let index = global_id.x;
    var particle = data[index];
    
    particle.position += particle.velocity * scene.delta_time;
    
    let width_f = f32(scene.width);
    let height_f = f32(scene.height);
    
    let invert_x = select(1.0, -1.0, (particle.position.x <= 0.0) || (particle.position.x >= width_f));
    particle.velocity.x *= invert_x;
    particle.position.x = clamp(particle.position.x, 0.0, width_f);
    
    let invert_y = select(1.0, -1.0, (particle.position.y <= 0.0) || (particle.position.y >= height_f));
    particle.velocity.y *= invert_y;
    particle.position.y = clamp(particle.position.y, 0.0, height_f);
    
    data[index] = particle;
}