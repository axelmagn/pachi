use godot::classes::{IRigidBody2D, RigidBody2D};
use godot::prelude::*;

#[derive(GodotClass)]
#[class(init, base=RigidBody2D)]
pub struct Ball {
    base: Base<RigidBody2D>,
}

#[godot_api]
impl IRigidBody2D for Ball {
    fn ready(&mut self) {
        // let mut self_gd = self.to_gd();
        // self.base()
        //     .signals()
        //     .body_entered()
        //     .connect_other(&mut self_gd, |ball, other| ball.handle_body_entered(other));
    }
}

impl Ball {}
