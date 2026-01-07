class_name TestBall extends RapierRigidBody2D

@export var lifetime_timer: Timer

func _ready() -> void:
	assert(lifetime_timer)
	lifetime_timer.timeout.connect(queue_free)
