use godot::{classes::CollisionShape2D, prelude::*};

/// Pachinko Board
#[derive(GodotClass)]
#[class(init, base=Node2D)]
struct Board {}

#[godot_api]
impl INode2D for Board {
    fn ready(&mut self) {}
}

#[godot_api]
impl Board {}
