use godot::prelude::*;

mod board;
mod game;
mod shape_sprites;

struct PachiExtension;

#[gdextension]
unsafe impl ExtensionLibrary for PachiExtension {}
