use godot::prelude::*;

mod ball;
mod ball_source;
mod board;
mod boundary;
mod game;
mod shape_sprites;
mod spawn_patterns;

struct PachiExtension;

#[gdextension]
unsafe impl ExtensionLibrary for PachiExtension {}
