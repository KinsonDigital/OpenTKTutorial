/*Info:
This is an example of alpha blending.

Src: https://stackoverflow.com/questions/746899/how-to-calculate-an-rgb-colour-by-specifying-an-alpha-blending-amount
*/

#version 330 core

out vec4 outputColor;

in vec2 texCoord;

uniform sampler2D u_texture0;
uniform vec4 u_tintClr;

void main ()
{
    vec4 fragClr = texture(u_texture0, texCoord);

	outputColor = texture(u_texture0, texCoord) * u_tintClr;
}