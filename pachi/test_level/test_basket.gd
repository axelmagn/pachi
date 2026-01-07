class_name TestBasket extends Node2D

@export var score_area: Area2D

signal scored

func _ready()-> void:
	assert(score_area)
	score_area.body_entered.connect(_on_body_entered)

func _on_body_entered(body) -> void:
	if body is TestBall:
		scored.emit()
		body.queue_free()
