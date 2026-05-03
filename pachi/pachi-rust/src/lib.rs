use godot::prelude::*;

pub mod ball;
pub mod ball_sink;
pub mod ball_source;
pub mod board;
pub mod boundary;
pub mod game;
pub mod launcher;
pub mod shape_sprites;
pub mod spawn_patterns;

struct PachiExtension;

#[gdextension]
unsafe impl ExtensionLibrary for PachiExtension {}
