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

//	outputColor.r = 0.5;
//	outputColor.r = u_tintClr.r;
//	outputColor.g = u_tintClr.g;
//	outputColor.b = u_tintClr.b;

	//If the frag color alpha channel is 100% transparent, ignore
	if (fragClr.a > 0)
	{
		outputColor.r = u_tintClr.a * u_tintClr.r + (1.0 - u_tintClr.a) * fragClr.r;
		outputColor.g = u_tintClr.a * u_tintClr.g + (1.0 - u_tintClr.a) * fragClr.g;
		outputColor.b = u_tintClr.a * u_tintClr.b + (1.0 - u_tintClr.a) * fragClr.b;
		outputColor.a = u_tintClr.a * u_tintClr.a + (1.0 - u_tintClr.a) * fragClr.a;
	}
}