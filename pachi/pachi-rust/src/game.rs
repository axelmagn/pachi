use godot::{prelude::*, tools::try_get_autoload_by_name};

use crate::{
    ball_source::BallSource,
    card::Card,
    hopper::Hopper,
    launcher::LauncherSystem,
    main_scene::{MainScene},
};

const GAME_AUTOLOAD_NAME: &str = "GlobalGame";

/// Game singleton, centralized via scene autoload
#[derive(GodotClass)]
#[class(init, base=Node)]
pub struct Game {
    #[export]
    pub launcher_system: Option<Gd<LauncherSystem>>,

    pub events: Gd<GameEvents>,
    main_scene: Option<Gd<MainScene>>,

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

    pub fn register_main_scene(&mut self, main_scene: Gd<MainScene>) {
        assert!(self.main_scene.is_none());
        self.main_scene = Some(main_scene);
    }

    pub fn unregister_main_scene(&mut self) {
        assert!(self.main_scene.is_some());
        self.main_scene = None;
    }

    pub fn expect_main_scene(&self) -> Gd<MainScene> {
        return self
            .main_scene
            .as_ref()
            .expect("MainScene not registered")
            .clone();
    }

    pub fn expect_hopper(&self) -> Gd<Hopper> {
        self.expect_main_scene().bind().expect_hopper()
    }

    pub fn get_scene_hopper(&self) -> Option<Gd<Hopper>> {
        self.main_scene
            .as_ref()
            .and_then(|main_scene| main_scene.bind().hopper.clone())
    }

    pub fn get_scene_ball_source(&self) -> Option<Gd<BallSource>> {
        self.main_scene
            .as_ref()
            .and_then(|main_scene| main_scene.bind().ball_source.clone())
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
impl GameEvents {
    #[signal]
    pub fn add_default_balls(num_balls: u32);

    #[signal]
    pub fn card_clicked(card: Gd<Card>);
}
