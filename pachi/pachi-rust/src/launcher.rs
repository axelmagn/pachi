use godot::{
    classes::{Input, Timer},
    global::randf_range,
    prelude::*,
};

use crate::{ball::Ball, ball_source::BallSource, game::Game};

#[derive(GodotClass)]
#[class(init, base=Node)]
pub struct Launcher {
    /// node to draw balls from
    /// TODO: better system for this
    #[export]
    hopper: Option<Gd<Node>>,

    #[export]
    ball_source: Option<Gd<BallSource>>,

    #[export]
    #[init(val = 0.2)]
    min_launch_strength: f32,

    #[export]
    #[init(val = true)]
    continuous_launch: bool,

    #[export]
    launch_charge_timer: Option<Gd<Timer>>,

    #[export]
    auto_fire_timer: Option<Gd<Timer>>,

    #[export]
    #[init(val = false)]
    launch_on_release: bool,

    #[init(val = false)]
    max_launch: bool,

    base: Base<Node>,
}

#[godot_api]
impl INode for Launcher {
    fn ready(&mut self) {
        let mut self_gd = self.to_gd();

        godot_print!("launcher ready");
        assert!(self.hopper.is_some());
        assert!(self.ball_source.is_some());
        assert!(self.launch_charge_timer.is_some());
        assert!(self.auto_fire_timer.is_some());

        let launch_timer_ref = self.launch_charge_timer.as_mut().unwrap();
        launch_timer_ref
            .signals()
            .timeout()
            .connect_other(&mut self_gd, |this| this.handle_launch_timeout());

        let auto_fire_timer_ref = self.auto_fire_timer.as_mut().unwrap();
        auto_fire_timer_ref
            .signals()
            .timeout()
            .connect_other(&mut self_gd, |this| this.handle_auto_fire_timeout());
    }

    fn physics_process(&mut self, _delta_time: f64) {
        if Input::singleton().is_action_just_pressed("ball_launch") {
            // godot_print!("launch pressed");
            assert_ne!(None, self.launch_charge_timer);
            self.launch_charge_timer.as_mut().unwrap().start();
            self.launch_on_release = true;
        }
        if Input::singleton().is_action_just_released("ball_launch") {
            // godot_print!("launch released");
            if !self.launch_charge_timer.as_ref().unwrap().is_paused() && self.launch_on_release {
                self.handle_launch_input();
            }
            self.launch_charge_timer.as_mut().unwrap().stop();
        }
    }
}

#[godot_api]
impl Launcher {
    fn handle_launch_timeout(&mut self) {
        if Input::singleton().is_action_pressed("ball_launch") {
            self.max_launch = true;
            self.handle_launch_input();
            self.max_launch = false;
            self.launch_on_release = false;
        }
        if self.continuous_launch {
            self.launch_charge_timer.as_mut().unwrap().start();
        }
    }

    fn handle_auto_fire_timeout(&mut self) {
        let launch_strength = randf_range(0.3, 0.9);
        self.launch(launch_strength as f32);
    }

    fn handle_launch_input(&mut self) {
        if self.hopper.is_none() || self.ball_source.is_none() {
            return;
        }

        assert!(self.launch_charge_timer.is_some());
        let timer_progress = self.get_progress();

        let launch_strength = if self.max_launch {
            1.0
        } else {
            self.min_launch_strength
                + (1. - self.min_launch_strength) * timer_progress.clamp(0.0, 1.0)
        };

        self.launch(launch_strength);
    }

    fn launch(&mut self, launch_strength: f32) {
        godot_print!("ball launch started");

        if let Some(ball) = Self::recursive_find_ball(self.hopper.as_ref().unwrap().clone()) {
            godot_print!("ball launched: {launch_strength}");
            self.ball_source
                .as_mut()
                .unwrap()
                .bind_mut()
                .launch_existing_ball(ball, launch_strength);
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

    pub fn get_progress(&self) -> f32 {
        let launch_charge_timer = self.launch_charge_timer.as_ref().unwrap();
        if launch_charge_timer.is_paused() || launch_charge_timer.is_stopped() {
            return 0.;
        }
        let timer_progress =
            1.0 - launch_charge_timer.get_time_left() / launch_charge_timer.get_wait_time();
        timer_progress as f32
    }
}

#[derive(GodotClass)]
#[class(init, base=Node2D)]
pub struct LauncherHandleView {
    #[export]
    #[init(val = 0.)]
    min_angle_deg: f32,
    #[export]
    #[init(val = 30.)]
    max_angle_deg: f32,

    base: Base<Node2D>,
}

#[godot_api]
impl INode2D for LauncherHandleView {
    fn process(&mut self, _delta: f64) {
        self.update_rotation();
    }
}

impl LauncherHandleView {
    fn update_rotation(&mut self) {
        let game = Game::autoload();
        let launcher_opt = game.bind().launcher.clone();
        if launcher_opt.is_none() {
            return;
        }
        let launcher = launcher_opt.unwrap();
        let progress = launcher.bind().get_progress();
        let angle = self.min_angle_deg + self.max_angle_deg * progress;
        self.base_mut().set_rotation_degrees(angle);
    }
}
