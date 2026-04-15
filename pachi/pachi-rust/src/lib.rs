use godot::prelude::*;

mod board;
mod game;

struct PachiExtension;

#[gdextension]
unsafe impl ExtensionLibrary for PachiExtension {}
