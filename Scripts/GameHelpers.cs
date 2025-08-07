namespace KomiChallenge.Scripts;

internal static class GameHelpers
{
	static Character character;
	static CharacterMovement movementComponent;
	
	public static Character GetCharacterComponent()
	{
		if (character == null || !character.isActiveAndEnabled)
			character = Character.localCharacter;
		
		return character;
	}

	public static CharacterMovement GetMovementComponent()
	{
		if (movementComponent == null || !movementComponent.isActiveAndEnabled)
			movementComponent = GetCharacterComponent()?.GetComponent<CharacterMovement>();
		
		return movementComponent;
	}
}
