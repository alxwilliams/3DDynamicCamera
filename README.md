# Honeydude and The Beast

This is my Paper Mario clone that is structured in a way I am much
more proud of. This is something I want to make a long term project,
so I'm trying my best to keep it as well organized as possible. There
are still a few things I'd still like to do in terms of organization
such as asset bundling (which I've started) and text localization. I have a personal excel
sheet I used to keep tickets for bugs and TODOs so I don't forget
anything along the way.

There are a few things to note while walking around, there are some
fun animations when walking into the villager characters on the map and
into the house.



WARNING: I have started to implement the combat scene, there is an enemy behind the houses on the left side of the map and you can walk into him to start combat. Combat isn't anything more than some animations and a menu at the moment which I'd like to show off, but if you enter combat for the time being, there is no way to escape it without exiting the game. 

The combat scene is dynamic and changes depending on which order the characters are in/who they are. The moves are loaded in from a json file (there is only one at the moment on the honeydude character) and the enemies are chosen randomly from different groups that the overworld enemy has a pool of.



As for controls, this should work with a xbox/ps4 controller, as well and keyboard and mouse

- you can walk around with the left stick/WASD,
- you can switch characters with the left trigger/Q,
- both characters can jump using the A/X button/SPACE unless in an
interactable like the characters (eventually there will be a
dialogue box that comes up when you talk to them),
-there is a pause menu accesible with start button/ESC that is only an animation and an interface that
doesn't do anything currently,

-and when controlling the Honeydew melon character he has some special
jump controls, pressing the A/X button/SPACE while the character is falling down.
If you press A/X button/SPACE while he is going down, you enter a jump state, your
limbs fall off and you fall faster, you then hit the ground, stick in
place quickly, and bounce higher than your regular jump. (There are
some little bugs when you hit into the falling limbs at the moment as
they can affect your velocity, I added these in quickly last night as
a special touch and hopefully can fix that up soon)


Thanks,
Alex


