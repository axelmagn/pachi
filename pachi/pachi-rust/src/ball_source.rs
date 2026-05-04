use godot::{
    classes::{IMarker2D, Marker2D, node::PhysicsInterpolationMode},
    global::randf_range,
    prelude::*,
};

use crate::ball::Ball;

#[derive(GodotClass)]
#[class(init, base=Marker2D)]
pub struct BallSource {
    #[export]
    launch_velocity: Vector2,

    #[export]
    #[init(val = Vector2::new(10., 10.))]
    launch_jitter: Vector2,

    base: Base<Marker2D>,
}

#[godot_api]
impl IMarker2D for BallSource {
    // TODO: launch ball
}

#[godot_api]
impl BallSource {
    pub fn launch_existing_ball(&self, mut ball: Gd<Ball>, strength: f32) {
        let mut parent = self.base().get_parent().unwrap();
        let position = self.base().get_position();
        let prior_interpolation_mode = ball.get_physics_interpolation_mode();
        let jitter = Vector2::new(
            randf_range(-self.launch_jitter.x as f64, self.launch_jitter.x as f64) as f32,
            randf_range(-self.launch_jitter.y as f64, self.launch_jitter.y as f64) as f32,
        );
        ball.set_physics_interpolation_mode(PhysicsInterpolationMode::OFF);
        if ball.get_parent().is_some() {
            ball.reparent(&parent);
        } else {
            parent.add_child(&ball);
        }
        ball.set_position(position);
        ball.set_linear_velocity(self.launch_velocity * strength + jitter);
        ball.set_physics_interpolation_mode(prior_interpolation_mode);
    }
}
