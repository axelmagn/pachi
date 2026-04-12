use godot::{classes::Engine, prelude::*, tools::try_get_autoload_by_name};

const GAME_AUTOLOAD_NAME: &str = "GlobalGame";

/// Game singleton, centralized via scene autoload
#[derive(GodotClass)]
#[class(base=Node)]
struct Game {
    events: Gd<GameEvents>,

    base: Base<Node>,
}

#[godot_api]
impl Game {
    fn autoload() -> Gd<Self> {
        try_get_autoload_by_name::<Game>(GAME_AUTOLOAD_NAME).expect("`Game` autoload not found")
    }
}

#[godot_api]
impl INode for Game {
    fn init(base: Base<Node>) -> Self {
        Self {
            events: GameEvents::new_gd(),
            base,
        }
    }
}

/// Event bus for global game signals
#[derive(GodotClass)]
#[class(init, base=RefCounted)]
struct GameEvents {
    base: Base<RefCounted>,
}
