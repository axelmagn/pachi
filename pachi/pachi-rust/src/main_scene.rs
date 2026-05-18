use godot::{
    classes::{INode, Node},
    obj::{Base, Gd, WithBaseField},
    register::{GodotClass, godot_api},
};

use crate::{ball_source::BallSource, board::Board, game::Game, hopper::Hopper, launcher::LauncherSystem};

#[derive(GodotClass)]
#[class(init, base=Node)]
pub struct MainScene {
    #[export]
    pub hopper: Option<Gd<Hopper>>,

    #[export]
    pub ball_source: Option<Gd<BallSource>>,

    base: Base<Node>,
}

#[godot_api]
impl INode for MainScene {
    fn ready(&mut self) {}

    fn enter_tree(&mut self) {
        let mut game = Game::autoload();
        game.bind_mut().register_main_scene(self.to_gd());
    }

    fn exit_tree(&mut self) {
        let mut game = Game::autoload();
        game.bind_mut().unregister_main_scene();
    }
}

#[godot_api]
impl MainScene {
    pub fn expect_hopper(&self) -> Gd<Hopper> {
        self.hopper.as_ref().expect("hopper is None").clone()
    }
}
