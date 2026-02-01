# Pachi TODO

Basic Types

- [ ] Ball: bounces around, scores points
- [ ] Peg: can be hit by balls
- [ ] BallSpawner: spawns balls at the top of the level
- [ ] BallCollector: collects balls and tallies points
- [ ] Level: contains pegs, balls, spawners, and collectors, organized into sections vertically.
- [ ] LevelSection: a section of a level
- [ ] Hopper: holds balls and releases them one at a time
- [ ] LaunchTrigger: player interacts with this to launch balls

MVP Features

- [ ] Hopper starts with 10 balls
- [ ] clicking the launch trigger launches a ball
- [ ] balls bounce off pegs and walls
- [ ] balls fall down the board
- [ ] balls are collected by the ball collector at the bottom
- [ ] the bottom ball collector scores 1 point per ball
- [ ] there are jackpot collectors that score 10 points per ball
- [ ] when the hopper is empty, the game ends
- [ ] when the game ends, the player can start a new game
- [ ] when the player starts a new game, the hopper is refilled with a number of balls based on the score
- [ ] when the player starts a new game, the score is reset
- [ ] when the player starts a new game, the board is reset