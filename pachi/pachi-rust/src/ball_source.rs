use godot::{
    classes::{IMarker2D, Marker2D},
    prelude::*,
};

#[derive(GodotClass)]
#[class(init, base=Marker2D)]
struct BallSource {
    #[export]
    launch_velocity: Vector2,

    #[export]
    launch_jitter: Vector2,

    base: Base<Marker2D>,
}

#[godot_api]
impl IMarker2D for BallSource {
    // TODO: launch ball
}
