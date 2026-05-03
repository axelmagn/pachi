use std::io::sink;

use godot::{
    classes::{Area2D, IMarker2D, Marker2D},
    prelude::*,
};

use crate::ball::Ball;

#[derive(GodotClass)]
#[class(init, base=Marker2D)]
pub struct BallSink {
    #[export]
    area: Option<Gd<Area2D>>,

    base: Base<Marker2D>,
}

#[godot_api]
impl IMarker2D for BallSink {
    fn ready(&mut self) {
        assert!(self.area.is_some());
    }

    fn physics_process(&mut self, _delta: f64) {
        for overlapper in self
            .area
            .as_ref()
            .unwrap()
            .get_overlapping_bodies()
            .iter_shared()
        {
            let mut ball = match overlapper.try_cast::<Ball>() {
                Ok(ball) => ball,
                _ => continue,
            };

            // TODO: read the correct fields, don't just hardcode
            let sink_radius = 16.;
            let ball_radius = 10.;
            let distance =
                (self.base().get_global_position() - ball.get_global_position()).length();
            if distance <= sink_radius - ball_radius {
                // TODO: signalling
                self.signals().ball_sunk().emit(&ball);
                ball.queue_free();
            }
        }
    }
}

#[godot_api]
impl BallSink {
    #[signal]
    fn ball_sunk(ball: Gd<Ball>);
}
