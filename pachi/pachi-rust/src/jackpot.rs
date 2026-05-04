use godot::{classes::AnimationPlayer, prelude::*};

use crate::{ball::Ball, ball_sink::BallSink, game::Game};

pub enum JackpotGateState {
    Open,
    Closed,
}

#[derive(GodotClass)]
#[class(init, base=Node2D)]
pub struct Jackpot {
    #[export]
    sink: Option<Gd<BallSink>>,

    #[export]
    animation_player: Option<Gd<AnimationPlayer>>,

    #[export]
    open_animation: StringName,

    #[export]
    close_animation: StringName,

    #[export]
    #[init(val = 15)]
    payout: u32,

    #[export]
    #[init(val = false)]
    #[var(set = set_gate_open)]
    gate_open: bool,

    base: Base<Node2D>,
}

#[godot_api]
impl INode2D for Jackpot {
    fn ready(&mut self) {
        assert!(self.sink.is_some());

        self.sink
            .as_ref()
            .unwrap()
            .signals()
            .ball_sunk()
            .connect_other(&mut self.to_gd(), |jackpot, ball| {
                jackpot.handle_ball_sunk(ball)
            });

        // trigger animations
        // TODO: fix it so that initial position is just correct
        let gate_open = self.gate_open;
        self.base_mut()
            .call_deferred("set_gate_open", &[Variant::from(gate_open)]);
    }
}

#[godot_api]
impl Jackpot {
    fn handle_ball_sunk(&mut self, _ball: Gd<Ball>) {
        Game::autoload()
            .bind_mut()
            .hopper
            .as_mut()
            .unwrap()
            .bind_mut()
            .add_default_balls(self.payout as usize);

        self.toggle_gate();
    }

    #[func]
    fn toggle_gate(&mut self) {
        self.gate_open = !self.gate_open;
        self.play_gate_animation();
    }

    #[func]
    fn set_gate_open(&mut self, gate_open: bool) {
        self.gate_open = gate_open;
        self.play_gate_animation();
    }

    #[func]
    fn init_gate_open(&mut self) {
        self.gate_open = !self.gate_open;
        self.play_gate_animation();
        self.gate_open = !self.gate_open;
        self.animation_player
            .as_mut()
            .unwrap()
            .call_deferred("pause", &[]);
    }

    fn play_gate_animation(&mut self) {
        if self.animation_player.is_none() {
            return;
        }

        if self.gate_open {
            self.animation_player
                .as_mut()
                .unwrap()
                .set_current_animation(&self.open_animation);
            self.animation_player.as_mut().unwrap().play();
        } else {
            self.animation_player
                .as_mut()
                .unwrap()
                .set_current_animation(&self.close_animation);
            self.animation_player.as_mut().unwrap().play();
        }
    }
}
