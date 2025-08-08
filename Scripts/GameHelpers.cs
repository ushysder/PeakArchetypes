namespace KomiChallenge.Scripts;

internal static class GameHelpers
{
	static Character character;
	
	public static Character GetCharacterComponent()
	{
		if (character == null || !character.isActiveAndEnabled)
			character = Character.localCharacter;
		
		return character;
	}
}
