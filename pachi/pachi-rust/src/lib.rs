use godot::prelude::*;

mod game;

struct PachiExtension;

#[gdextension]
unsafe impl ExtensionLibrary for PachiExtension {}
