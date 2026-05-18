use core::num;

use godot::{classes::Timer, prelude::*};

use crate::{ball::Ball, ball_source::BallSource, game::Game};

/// Pachinko Board
#[derive(GodotClass)]
#[class(init, base=Node2D)]
pub struct Hopper {
    #[export]
    launch_sources: Array<Gd<BallSource>>,

    #[export]
    launch_timer: Option<Gd<Timer>>,

    #[export]
    default_ball_scn: Option<Gd<PackedScene>>,

    #[export]
    #[init(val = 20)]
    initial_balls: u32,

    #[init(val = 0)]
    next_launch_source: usize,

    pending_balls: Array<Gd<Ball>>,

    base: Base<Node2D>,
}

#[godot_api]
impl INode2D for Hopper {
    fn ready(&mut self) {
        assert!(self.launch_timer.is_some());
        assert!(self.default_ball_scn.is_some());

        self.launch_timer
            .as_ref()
            .unwrap()
            .signals()
            .timeout()
            .connect_other(&mut self.to_gd(), |hopper| hopper.try_launch_next_ball());

        self.add_default_balls(self.initial_balls as usize);

        let game = Game::autoload();
        game.bind()
            .events
            .signals()
            .add_default_balls()
            .connect_other(&self.to_gd(), |inner_self, num_balls| {
                inner_self.add_default_balls(num_balls as usize)
            });
    }
}

#[godot_api]
impl Hopper {
    fn try_launch_next_ball(&mut self) {
        let next_ball = match self.pending_balls.pop() {
            Some(ball) => ball,
            _ => return,
        };

        assert!(self.next_launch_source < self.launch_sources.len());
        let mut launch_source: Gd<BallSource> =
            self.launch_sources.get(self.next_launch_source).unwrap();
        launch_source.bind_mut().launch_existing_ball(next_ball, 1.);

        self.next_launch_source = (self.next_launch_source + 1) % self.launch_sources.len();
    }

    pub fn add_default_balls(&mut self, num_balls: usize) {
        for _ in 0..num_balls {
            let ball = self
                .default_ball_scn
                .as_ref()
                .unwrap()
                .instantiate()
                .unwrap()
                .cast::<Ball>();
            self.pending_balls.push(&ball);
        }
    }

    // TODO: bookkeeping to make this less expensive
    #[func]
    pub fn ball_count(&self) -> u32 {
        let mut out = 0;
        for child in self.base().get_children().iter_shared() {
            match child.try_cast::<Ball>() {
                Ok(_) => out += 1,
                _ => {}
            }
        }
        out
    }

    pub fn destroy_balls(&mut self, num_balls: usize) {
        self.base_mut()
            .get_children()
            .iter_shared()
            .filter(|child| match child.clone().try_cast::<Ball>() {
                Ok(_) => true,
                _ => false,
            })
            .take(num_balls)
            .for_each(|mut child| child.queue_free());
    }
}
