use godot::{
    classes::{Button, Control, IButton, IControl, Label},
    prelude::*,
    register::property::export_fns::export_multiline,
};

use crate::game::Game;

#[derive(GodotClass)]
#[class(init, base=Control)]
struct ResourcesUI {
    #[export]
    cash_label: Option<Gd<Label>>,

    base: Base<Control>,
}

#[godot_api]
impl IControl for ResourcesUI {
    fn process(&mut self, _delta: f64) {
        let game = Game::autoload();

        assert!(self.cash_label.is_some());
        let cash = game.bind().cash;
        self.cash_label
            .as_mut()
            .unwrap()
            .set_text(&cash.to_string());
    }
}

#[derive(GodotClass)]
#[class(init, base=Button)]
struct BuyBallsButton {
    #[export]
    price: u32,

    #[export]
    balls: u32,

    #[init(val = "Buy {balls} balls\n(${price})".into())]
    text_fmt: GString,

    base: Base<Button>,
}

#[godot_api]
impl IButton for BuyBallsButton {
    fn ready(&mut self) {
        self.base()
            .signals()
            .pressed()
            .connect_other(&mut self.to_gd(), |button| button.on_pressed());

        let mut text_args: Dictionary<GString, GString> = Dictionary::new();
        _ = text_args.insert("price", &self.price.to_string());
        _ = text_args.insert("balls", &self.balls.to_string());
        let text = self.text_fmt.format(&text_args.to_variant());
        self.base_mut().set_text(&text);
    }

    fn process(&mut self, _delta: f64) {
        let disabled = self.price > Game::autoload().bind().cash;
        self.base_mut().set_disabled(disabled);
    }
}

#[godot_api]
impl BuyBallsButton {
    fn on_pressed(&mut self) {
        let mut game = Game::autoload();
        if game.bind().cash < self.price {
            return;
        }

        game.bind_mut().cash -= self.price;
        game.bind_mut()
            .hopper
            .as_mut()
            .unwrap()
            .bind_mut()
            .add_default_balls(self.balls as usize);
    }
}

#[derive(GodotClass)]
#[class(init, base=Button)]
struct SellBallsButton {
    #[export]
    price: u32,

    #[export]
    balls: u32,

    #[init(val = "Sell {balls} balls\n(${price})".into())]
    text_fmt: GString,

    base: Base<Button>,
}

#[godot_api]
impl IButton for SellBallsButton {
    fn ready(&mut self) {
        self.base()
            .signals()
            .pressed()
            .connect_other(&mut self.to_gd(), |button| button.on_pressed());

        let mut text_args: Dictionary<GString, GString> = Dictionary::new();
        _ = text_args.insert("price", &self.price.to_string());
        _ = text_args.insert("balls", &self.balls.to_string());
        let text = self.text_fmt.format(&text_args.to_variant());
        self.base_mut().set_text(&text);
    }

    fn process(&mut self, _delta: f64) {
        let ball_count = Game::autoload()
            .bind()
            .hopper
            .as_ref()
            .unwrap()
            .bind()
            .ball_count();

        let disabled = self.balls > ball_count;
        self.base_mut().set_disabled(disabled);
    }
}

#[godot_api]
impl SellBallsButton {
    fn on_pressed(&mut self) {
        let mut game = Game::autoload();
        game.bind_mut().cash += self.price;
        game.bind_mut()
            .hopper
            .as_mut()
            .unwrap()
            .bind_mut()
            .destroy_balls(self.balls as usize);
    }
}
