use godot::classes::RigidBody2D;
use godot::prelude::*;

#[derive(GodotClass)]
#[class(init, base=RigidBody2D)]
pub struct Ball {
    base: Base<RigidBody2D>,
}
