use godot::{
    classes::{CanvasLayer, Control, IControl, INode, InputEvent, Label, Node, RichTextLabel},
    global::godot_print,
    obj::{Base, Gd, GdRef, WithBaseField, WithUserSignals},
    register::{GodotClass, godot_api},
};

use crate::game::Game;

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
        assert!(self.title.is_some());
        assert!(self.description.is_some());
        self.signals().gui_input().connect_self(Self::on_gui_input);
    }
}

#[godot_api]
impl Card {
    fn on_gui_input(&mut self, event: Gd<InputEvent>) {
        if event.is_action_pressed("mouse_select") {
            godot_print!("card clicked!");
            let game = Game::autoload();
            game.bind()
                .events
                .signals()
                .card_clicked()
                .emit(&self.to_gd());
        }
    }

    fn copy_visuals(&mut self, other_card: GdRef<Card>) {
        let title_text = other_card.title.as_ref().unwrap().get_text();
        self.title.as_mut().unwrap().set_text(&title_text);

        let description_text = other_card.description.as_ref().unwrap().get_text();
        self.description
            .as_mut()
            .unwrap()
            .set_text(&description_text);
    }
}

#[derive(GodotClass)]
#[class(init, base=Node)]
pub struct CardManager {
    #[export]
    pub dragged_card_proxy: Option<Gd<Card>>,

    base: Base<Node>,
}

#[godot_api]
impl INode for CardManager {
    fn ready(&mut self) {
        assert!(self.dragged_card_proxy.is_some());
        let game = Game::autoload();
        game.bind()
            .events
            .signals()
            .card_clicked()
            .connect_other(&self.to_gd(), Self::on_card_clicked);
    }
}

#[godot_api]
impl CardManager {
    fn on_card_clicked(&mut self, card: Gd<Card>) {
        godot_print!("card click detected by manager: {}", card.to_string());
        self.dragged_card_proxy
            .as_mut()
            .unwrap()
            .bind_mut()
            .copy_visuals(card.bind());
    }
}
