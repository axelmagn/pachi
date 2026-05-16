use godot::{
    classes::{CanvasLayer, Control, IControl, InputEvent, Label, Node, RichTextLabel},
    global::godot_print,
    obj::{Base, Gd, WithUserSignals},
    register::{GodotClass, godot_api},
};

#[derive(GodotClass)]
#[class(init, base=Control)]
pub struct Card {
    #[export]
    pub title: Option<Gd<Label>>,
    #[export]
    pub description: Option<Gd<RichTextLabel>>,

    base: Base<Control>,
}

#[godot_api]
impl IControl for Card {
    fn ready(&mut self) {
        self.signals().gui_input().connect_self(Self::on_gui_input);
    }
}

#[godot_api]
impl Card {
    fn on_gui_input(&mut self, event: Gd<InputEvent>) {
        if event.is_action_pressed("mouse_select") {
            godot_print!("card clicked!");
        }
    }
}

#[derive(GodotClass)]
#[class(init, base=Node)]
pub struct CardManager {
    #[export]
    pub layer: Option<Gd<CanvasLayer>>,

    pub dragged_card: Option<Gd<Card>>,

    base: Base<Node>,
}
