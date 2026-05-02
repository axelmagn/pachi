use godot::{
    classes::{CollisionShape2D, IStaticBody2D, StaticBody2D},
    prelude::*,
};

/// Pachinko DynamicBoundary
#[derive(GodotClass)]
#[class(init, tool, base=StaticBody2D)]
struct DynamicBoundary {
    #[export]
    #[init(val = Vector2::new(1080., 1080.))]
    #[var(set=set_boundary_size)]
    boundary_size: Vector2,

    #[export]
    top_boundary: Option<Gd<CollisionShape2D>>,
    #[export]
    bottom_boundary: Option<Gd<CollisionShape2D>>,
    #[export]
    left_boundary: Option<Gd<CollisionShape2D>>,
    #[export]
    right_boundary: Option<Gd<CollisionShape2D>>,

    base: Base<StaticBody2D>,
}

#[godot_api]
impl IStaticBody2D for DynamicBoundary {}

#[godot_api]
impl DynamicBoundary {
    #[func]
    fn set_boundary_size(&mut self, size: Vector2) {
        self.boundary_size = size;
        self.place_boundaries();
    }

    fn place_boundaries(&mut self) {
        assert!(self.boundary_size.x > 0. && self.boundary_size.y > 0.);

        if self.top_boundary.is_some() {
            self.top_boundary
                .as_mut()
                .unwrap()
                .set_position(Vector2::new(0., -self.boundary_size.y / 2.));
        }

        if self.bottom_boundary.is_some() {
            self.bottom_boundary
                .as_mut()
                .unwrap()
                .set_position(Vector2::new(0., self.boundary_size.y / 2.));
        }

        if self.left_boundary.is_some() {
            self.left_boundary
                .as_mut()
                .unwrap()
                .set_position(Vector2::new(-self.boundary_size.x / 2., 0.));
        }

        if self.right_boundary.is_some() {
            self.right_boundary
                .as_mut()
                .unwrap()
                .set_position(Vector2::new(self.boundary_size.x / 2., 0.));
        }
    }
}
