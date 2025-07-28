from enum import Enum

class DHRecipe(Enum):
	BONK = 2
	SPICY = 3
	RAT_REPELLENT = 4
	POWER = 5
	STRAWBERRY = 6
	REROLL = 7

	def name(self) -> str:
		match self:
			case DHRecipe.BONK: return "bonk potion"
			case DHRecipe.SPICY: return "spicy potion"
			case DHRecipe.RAT_REPELLENT: return "rat repellent"
			case DHRecipe.POWER: return "power potion"
			case DHRecipe.STRAWBERRY: return "strawberry potion"
			case DHRecipe.REROLL: return "reroll potion"
