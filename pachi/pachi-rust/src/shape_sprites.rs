use godot::prelude::*;

#[derive(GodotClass)]
#[class(init, base=Node2D)]
struct CircleSprite {
    #[export]
    #[init(val = 32.0)]
    #[var(set=set_radius)]
    radius: f32,

    #[export]
    #[init(val = Color::WHITE)]
    #[var(set=set_color)]
    color: Color,

    base: Base<Node2D>,
}

#[godot_api]
impl INode2D for CircleSprite {
    fn draw(&mut self) {
        let radius = self.radius;
        let color = self.color;
        self.base_mut().draw_circle(Vector2::ZERO, radius, color);
    }
}

#[godot_api]
impl CircleSprite {
    #[func]
    fn set_radius(&mut self, radius: f32) {
        self.radius = radius;
        self.base_mut().queue_redraw();
    }

    #[func]
    fn set_color(&mut self, color: Color) {
        self.color = color;
        self.base_mut().queue_redraw();
    }
}

#[derive(GodotClass)]
#[class(base=Node2D)]
struct RectSprite {
    #[export]
    #[var(set=set_size)]
    size: Vector2,

    #[export]
    #[var(set=set_color)]
    color: Color,

    base: Base<Node2D>,
}

#[godot_api]
impl INode2D for RectSprite {
    fn init(base: Base<Node2D>) -> Self {
        Self {
            size: Vector2::new(64., 32.),
            color: Color::WHITE,
            base,
        }
    }

    fn draw(&mut self) {
        let rect = Rect2::new(Vector2::ZERO, self.size);
        let color = self.color;
        self.base_mut().draw_rect(rect, color);
    }
}

#[godot_api]
impl RectSprite {
    #[func]
    fn set_size(&mut self, size: Vector2) {
        self.size = size;
        self.base_mut().queue_redraw();
    }

    #[func]
    fn set_color(&mut self, color: Color) {
        self.color = color;
        self.base_mut().queue_redraw();
    }
}
