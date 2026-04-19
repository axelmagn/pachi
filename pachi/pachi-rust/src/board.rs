use godot::{
    classes::CollisionShape2D,
    prelude::*,
};

/// Pachinko Board
#[derive(GodotClass)]
#[class(init, base=Node2D)]
struct Board {
    #[export]
    #[init(val = 1080.)]
    board_width: f32,
    #[export]
    #[init(val = 1080.)]
    board_height: f32,

    #[export]
    top_boundary: Option<Gd<CollisionShape2D>>,
    #[export]
    bottom_boundary: Option<Gd<CollisionShape2D>>,
    #[export]
    left_boundary: Option<Gd<CollisionShape2D>>,
    #[export]
    right_boundary: Option<Gd<CollisionShape2D>>,

    base: Base<Node2D>,
}

#[godot_api]
impl INode2D for Board {
    fn ready(&mut self) {
        assert!(self.top_boundary.is_some());
        assert!(self.bottom_boundary.is_some());
        assert!(self.left_boundary.is_some());
        assert!(self.right_boundary.is_some());

        self.place_boundaries();
        println!("board ready");
    }
}

impl Board {
    fn place_boundaries(&mut self) {
        assert!(self.board_width > 0.);
        assert!(self.board_height > 0.);

        assert!(self.top_boundary.is_some());
        self.top_boundary
            .as_mut()
            .unwrap()
            .set_position(Vector2::new(0., -self.board_height / 2.));

        assert!(self.bottom_boundary.is_some());
        self.bottom_boundary
            .as_mut()
            .unwrap()
            .set_position(Vector2::new(0., self.board_height / 2.));

        assert!(self.left_boundary.is_some());
        self.left_boundary
            .as_mut()
            .unwrap()
            .set_position(Vector2::new(0., -self.board_width / 2.));

        assert!(self.right_boundary.is_some());
        self.right_boundary
            .as_mut()
            .unwrap()
            .set_position(Vector2::new(0., self.board_width / 2.));
    }
}
