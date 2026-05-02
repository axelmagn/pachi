use godot::{classes::Input, prelude::*};

use crate::{ball::Ball, ball_source::BallSource};

#[derive(GodotClass)]
#[class(init, base=Node)]
pub struct Launcher {
    /// node to draw balls from
    /// TODO: better system for this
    #[export]
    hopper: Option<Gd<Node>>,

    #[export]
    ball_source: Option<Gd<BallSource>>,

    base: Base<Node>,
}

#[godot_api]
impl INode for Launcher {
    fn ready(&mut self) {
        godot_print!("launcher ready");
    }

    fn physics_process(&mut self, _delta_time: f64) {
        if Input::singleton().is_action_just_pressed("ball_launch") {
            godot_print!("ball launch pressed");
            self.handle_launch_input();
        }
    }
}

#[godot_api]
impl Launcher {
    fn handle_launch_input(&mut self) {
        if self.hopper.is_none() || self.ball_source.is_none() {
            return;
        }

        godot_print!("ball launch started");

        if let Some(ball) = Self::recursive_find_ball(self.hopper.as_ref().unwrap().clone()) {
            godot_print!("found ball. launching");
            self.ball_source
                .as_mut()
                .unwrap()
                .bind_mut()
                .launch_existing_ball(ball, 1.0);
        } else {
            godot_print!("ball not found");
        }
    }

    fn recursive_find_ball(node: Gd<Node>) -> Option<Gd<Ball>> {
        if let Ok(ball) = node.clone().try_cast::<Ball>() {
            return Some(ball);
        }

        for child in node.get_children().iter_shared() {
            let res = Self::recursive_find_ball(child);
            if res.is_some() {
                return res;
            }
        }

        None
    }
}
