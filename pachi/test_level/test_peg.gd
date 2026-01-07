extends RapierStaticBody2D


func _ready() -> void:
	input_event.connect(_on_input_event)

func _on_input_event(_viewport: Viewport, event: InputEvent, _shape_idx: int) -> void:
	if event is InputEventMouseButton:
		if event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
			queue_free()
