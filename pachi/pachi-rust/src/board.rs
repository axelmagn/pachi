use godot::{classes::CollisionShape2D, prelude::*};

use crate::ball_source::BallSource;

/// Pachinko Board
#[derive(GodotClass)]
#[class(init, base=Node2D)]
pub struct Board {
    #[export]
    launch_source: Option<Gd<BallSource>>,
}

#[godot_api]
impl INode2D for Board {
    fn ready(&mut self) {
        godot_print!("board ready");
    }
}

#[godot_api]
impl Board {}
