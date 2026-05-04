use godot::{prelude::*, tools::try_get_autoload_by_name};

use crate::{hopper::Hopper, launcher::Launcher};

const GAME_AUTOLOAD_NAME: &str = "GlobalGame";

/// Game singleton, centralized via scene autoload
#[derive(GodotClass)]
#[class(init, base=Node)]
pub struct Game {
    pub events: Gd<GameEvents>,

    #[export]
    pub launcher: Option<Gd<Launcher>>,

    #[export]
    pub hopper: Option<Gd<Hopper>>,

    #[export]
    #[init(val = 0)]
    pub cash: u32,

    base: Base<Node>,
}

#[godot_api]
impl Game {
    pub fn autoload() -> Gd<Self> {
        try_get_autoload_by_name::<Game>(GAME_AUTOLOAD_NAME).expect("`Game` autoload not found")
    }
}

#[godot_api]
impl INode for Game {
    fn ready(&mut self) {
        assert!(self.launcher.is_some());
    }
}

/// Event bus for global game signals
/// TODO: add events
#[derive(GodotClass)]
#[class(init, base=RefCounted)]
pub struct GameEvents {
    base: Base<RefCounted>,
}

#[godot_api]
impl GameEvents {}
