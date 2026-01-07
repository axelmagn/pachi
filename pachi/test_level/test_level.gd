class_name TestLevel extends Node

@export var ball_scn: PackedScene
@export var ball_spawn: Node
@export var ball_spawn_vel_x_max: float = 128

@export var peg_scn: PackedScene
@export var peg_spawn: Node
@export var peg_spacing: Vector2 = Vector2(32, 24)
@export var pegs_dim: Vector2i = Vector2i(6, 4)

@export var ball_spawn_timer: Timer

@export var score_label: Label

var score: int = 0:
	set(value):
		score = value
		update_score_label()

var score_history: Array[int] = []
var score_history_idx: int = 0


func _ready() -> void:
	assert(score_label)

	assert(ball_spawn_timer)
	ball_spawn_timer.timeout.connect(spawn_ball)
	# ball_spawn_timer.one_shot = false
	# ball_spawn_timer.start()

	for i in range(10 + 1):
		score_history.append(0)

	spawn_pegs()

	# register baskets
	for child in get_children():
		if child is TestBasket:
			child.scored.connect(_on_scored)

func _physics_process(_delta: float) -> void:
	@warning_ignore("integer_division")
	var cur_sec = (Time.get_ticks_msec() / 1000) % len(score_history)
	if cur_sec != score_history_idx:
		score_history_idx = cur_sec
		score_history[score_history_idx] = 0
		update_score_label()


func spawn_ball() -> void:
	assert(ball_scn)
	assert(ball_spawn)
	var ball: RigidBody2D = ball_scn.instantiate()
	ball.global_position = ball_spawn.global_position
	ball.linear_velocity.x = (randf() * 2.0 - 1.0) * ball_spawn_vel_x_max;
	add_child(ball)

func spawn_pegs() -> void:
	assert(peg_scn)
	assert(peg_spawn)

	var placement_width = peg_spacing.x * (pegs_dim.x - 1)

	for i in range(pegs_dim.y):
		var x = -placement_width / 2
		var y = -peg_spacing.y * i

		var w = pegs_dim.x

		if i % 2 == 1:
			x += peg_spacing.x / 2
			w -= 1

		for _j in range(w):
			var peg = peg_scn.instantiate()
			peg.position = Vector2(x, y)
			peg_spawn.add_child(peg)
			x += peg_spacing.x

func _on_scored():
	score += 1
	score_history[score_history_idx] += 1

func score_per_sec() -> float:
	var sum: float = 0.0
	for i in score_history.size():
		if i == score_history_idx: continue
		var elem = score_history[i]
		sum += elem
	return sum / (score_history.size() - 1)

func update_score_label() -> void:
	score_label.text = "%d\n%.1f per sec" % [score, score_per_sec()]