use godot::{prelude::*, tools::try_get_autoload_by_name};

use crate::board::Board;

const GAME_AUTOLOAD_NAME: &str = "GlobalGame";

/// Game singleton, centralized via scene autoload
#[derive(GodotClass)]
#[class(init, base=Node)]
struct Game {
    // events: Gd<GameEvents>,
    #[export]
    board: Option<Gd<Board>>,

    #[export]
    hopper: Option<Gd<Board>>,

    base: Base<Node>,
}

#[godot_api]
impl Game {
    fn autoload() -> Gd<Self> {
        try_get_autoload_by_name::<Game>(GAME_AUTOLOAD_NAME).expect("`Game` autoload not found")
    }
}

#[godot_api]
impl INode for Game {}

// /// Event bus for global game signals
// /// TODO: add events
// #[derive(GodotClass)]
// #[class(init, base=RefCounted)]
// struct GameEvents {
//     base: Base<RefCounted>,
// }
