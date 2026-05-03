use std::f32::consts::PI as PI_f32;

use godot::{
    builtin::math::ApproxEq,
    classes::{Engine, Range},
    prelude::*,
};

#[derive(GodotClass)]
#[class(init, tool, base=Node2D)]
struct GridSpawnPattern {
    #[export]
    #[init(val=Vector2::new(64., 64.))]
    #[var(set=set_spacing)]
    spacing: Vector2,

    #[export]
    #[init(val=Vector2i::new(8, 4))]
    #[var(set=set_cardinality)]
    cardinality: Vector2i,

    #[export]
    #[var(set=set_spawn_scn)]
    spawn_scn: Option<Gd<PackedScene>>,

    base: Base<Node2D>,
}

#[godot_api]
impl INode2D for GridSpawnPattern {
    fn ready(&mut self) {
        self.update_children();
    }
}

#[godot_api]
impl GridSpawnPattern {
    #[func]
    fn set_spacing(&mut self, spacing: Vector2) {
        self.spacing = spacing;
        self.update_children();
    }

    #[func]
    fn set_cardinality(&mut self, cardinality: Vector2i) {
        self.cardinality = cardinality;
        self.update_children();
    }

    #[func]
    fn set_spawn_scn(&mut self, spawn_scn: Option<Gd<PackedScene>>) {
        self.spawn_scn = spawn_scn;
        self.clear_children();
        self.update_children();
    }

    fn update_children(&mut self) {
        // if there is no spawn scene, delete children
        if self.spawn_scn.is_none()
            || self.cardinality.x < 1
            || self.cardinality.y < 1
            || self.spacing.x == 0.
            || self.spacing.y == 0.
        {
            self.clear_children();
            return;
        }

        let children_count = self.base().get_child_count();
        // we subtract cardinality.y / 2 in order to account for the fact that every other row will
        // have one less peg, leading to triangle pattern
        let requested_count = self.cardinality.x * self.cardinality.y - self.cardinality.y / 2;

        if children_count < requested_count {
            // create more children if needed
            let create_count = requested_count - children_count;
            for _ in 0..(create_count) {
                let child = self.spawn_scn.as_ref().unwrap().instantiate().unwrap();
                self.base_mut().add_child(&child);
            }
        } else if children_count > requested_count {
            let delete_count = children_count - requested_count;
            let mut base = self.base_mut();
            let mut deleted = 0;
            for mut child in base.get_children().iter_shared() {
                base.remove_child(&child);
                child.queue_free();
                deleted += 1;
                if deleted == delete_count {
                    break;
                }
            }
        }

        assert_eq!(requested_count, self.base().get_child_count());

        let offset = Vector2::new(self.spacing.x * (self.cardinality.x as f32 - 1.) / -2., 0.);
        let mut position = Vector2::ZERO;
        let mut row = 0;
        let mut col = 0;
        let mut width = self.cardinality.x;
        let cardinality = self.cardinality;
        let spacing = self.spacing;
        let base = self.base_mut();

        for child in base.get_children().iter_shared() {
            let mut child2d = child.cast::<Node2D>();
            child2d.set_position(offset + position);

            col += 1;
            position.x += spacing.x;
            if col >= width {
                // next row
                row += 1;
                col = 0;
                width = cardinality.x - row % 2;
                position.x = spacing.x / 2. * (row % 2) as f32;
                position.y = row as f32 * spacing.y;
            }
        }
    }

    fn clear_children(&mut self) {
        for mut child in self.base_mut().get_children().iter_shared() {
            child.queue_free();
        }
    }
}

/// Draw an ellipse (or partial ellipse) of overlapping collision objects
#[derive(GodotClass)]
#[class(init, tool, base=Node2D)]
struct EllipseColliderSpawnPattern {
    #[export]
    #[init(val=Vector2::new(256., 128.))]
    #[var(set=set_size)]
    size: Vector2,

    #[export]
    #[init(val = 0.)]
    #[var(set=set_angle_start)]
    angle_start: f32,
    #[export]
    #[init(val = 2. * PI_f32)]
    #[var(set=set_angle_end)]
    angle_end: f32,

    #[export]
    #[init(val = 32)]
    #[var(set=set_segments)]
    segments: u32,

    #[export]
    #[var(set=set_spawn_scn)]
    spawn_scn: Option<Gd<PackedScene>>,

    base: Base<Node2D>,
}

#[godot_api]
impl INode2D for EllipseColliderSpawnPattern {
    fn ready(&mut self) {
        self.update_children();
    }
}

#[godot_api]
impl EllipseColliderSpawnPattern {
    #[func]
    fn set_size(&mut self, size: Vector2) {
        self.size = size;
        self.update_children();
    }

    #[func]
    fn set_angle_start(&mut self, angle_start: f32) {
        self.angle_start = angle_start;
        self.update_children();
    }

    #[func]
    fn set_angle_end(&mut self, angle_start: f32) {
        self.angle_end = angle_start;
        self.update_children();
    }

    #[func]
    fn set_segments(&mut self, segments: u32) {
        self.segments = segments;
        self.update_children();
    }

    #[func]
    fn set_spawn_scn(&mut self, spawn_scn: Option<Gd<PackedScene>>) {
        self.spawn_scn = spawn_scn;
        self.clear_children();
        self.update_children();
    }

    fn calc_points(&self) -> Vec<(Vector2, f32)> {
        let mut out = Vec::new();

        if self.angle_start.approx_eq(&self.angle_end) || self.segments == 0 {
            return out;
        }

        // if angle_start is greater, don't sweat it.  just fix in post.
        let angle_start = f32::min(self.angle_start, self.angle_end);
        let mut angle_end = f32::max(self.angle_start, self.angle_end);
        let step = (angle_end - angle_start) / self.segments as f32;
        let mut t = angle_start;

        if angle_end - angle_start > 2. * PI_f32 {
            angle_end = angle_start + 2. * PI_f32;
        }

        let mut n_points = self.segments + 1;
        if (angle_end - angle_start - 2. * PI_f32).is_zero_approx() {
            n_points -= 1;
        }

        for _i in 0..n_points {
            let x = self.size.x * 0.5 * f32::cos(t);
            let y = self.size.y * 0.5 * f32::sin(t);
            let point = Vector2::new(x, y);

            let t_next = t + step;

            let x_next = self.size.x * 0.5 * f32::cos(t_next);
            let y_next = self.size.y * 0.5 * f32::sin(t_next);
            let point_next = Vector2::new(x_next, y_next);

            let angle = point.angle_to_point(point_next) + PI_f32 * 0.5;
            // godot_print!("angle: {angle} (({t}: {x}, {y}) -> ({t_next}: {x_next},{y_next}))");

            out.push((point, angle));
            t = t_next;
        }

        out
    }

    fn update_children(&mut self) {
        // if there is no spawn scene, delete children
        let points = self.calc_points();
        if self.spawn_scn.is_none() || points.len() < 2 {
            self.clear_children();
            return;
        }

        let children_count = self.base().get_child_count();
        let requested_count: i32 = (points.len()) as i32;

        if children_count < requested_count {
            // create more children if needed
            let create_count = requested_count - children_count;
            for _ in 0..(create_count) {
                let child = self.spawn_scn.as_ref().unwrap().instantiate().unwrap();
                self.base_mut().add_child(&child);
            }
        } else if children_count > requested_count {
            let delete_count = children_count - requested_count;
            let mut base = self.base_mut();
            let mut deleted = 0;
            for mut child in base.get_children().iter_shared() {
                base.remove_child(&child);
                child.queue_free();
                deleted += 1;
                if deleted == delete_count {
                    break;
                }
            }
        }
        assert_eq!(requested_count, self.base().get_child_count());

        let base = self.base_mut();
        for (i, child) in base.get_children().iter_shared().enumerate() {
            let mut child2d = child.cast::<Node2D>();
            child2d.set_position(points[i].0);
            child2d.set_rotation(points[i].1);
        }
    }

    fn clear_children(&mut self) {
        for mut child in self.base_mut().get_children().iter_shared() {
            child.queue_free();
        }
    }
}
