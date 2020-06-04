/*Info:
This is an example of alpha blending.

Src: https://stackoverflow.com/questions/746899/how-to-calculate-an-rgb-colour-by-specifying-an-alpha-blending-amount
*/
#version 450 core

in vec2 v_TexCoord;
in vec4 v_TintClr;
in float v_TextureIndex;

out vec4 o_OutputColor;


uniform sampler2D textures[2];


void main ()
{
	int index = int(v_TextureIndex);

//	vec4 theColor = vec4(0, 0, 0, 255);//Black
//
//	if (v_TextureIndex == 0)
//	{
//		theColor = new vec4(255, 0, 0, 255);//Red
//	}
//	else if (v_TextureIndex == 1)
//	{
//		theColor = new vec4(0, 0, 255, 255);//Blue
//	}

	o_OutputColor = texture(textures[index], v_TexCoord) * v_TintClr;
}