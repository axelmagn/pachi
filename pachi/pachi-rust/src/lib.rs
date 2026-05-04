use godot::prelude::*;

pub mod ball;
pub mod ball_sink;
pub mod ball_source;
pub mod board;
pub mod boundary;
pub mod game;
pub mod hopper;
pub mod jackpot;
pub mod launcher;
pub mod shape_sprites;
pub mod spawn_patterns;
pub mod ui;

struct PachiExtension;

#[gdextension]
unsafe impl ExtensionLibrary for PachiExtension {}
