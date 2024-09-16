
struct Particle {
    position: vec2f,
    velocity: vec2f,
};

@group(0) @binding(0) var<storage, read_write> data: array<Particle>;

@compute @workgroup_size(256,1,1)
fn main(
    @builtin(local_invocation_id) local_id: vec3<u32>,
    @builtin(local_invocation_index) local_index: u32,
    @builtin(global_invocation_id) global_id: vec3<u32>,
) {
    let index = global_id.x;
    var particle = data[index];
    
    particle.position += particle.velocity * 0.1;
    if (particle.position.x <= 0.0)
    {
        particle.velocity.x = -particle.velocity.x;
        particle.position.x = 0.0;
    }
    else if (particle.position.x >= 1600.0)
    {
        particle.velocity.x = -particle.velocity.x;
        particle.position.x = 1600.0;
    }
    if (particle.position.y <= 0.0)
    {
        particle.velocity.y = -particle.velocity.y;
        particle.position.y = 0.0;
    }
    else if (particle.position.y >= 900.0)
    {
        particle.velocity.y = -particle.velocity.y;
        particle.position.y = 900.0;
    }
    
    data[index] = particle;
}