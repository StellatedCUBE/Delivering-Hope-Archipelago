from enum import Enum

class DHAbility(Enum):
	DOWNBOOST = 0
	BEACH = 1
	FUSION = 2
	SHOP = 3
	LAB = 4

	REROLL = 10
	SECOND_EQUIP_SLOT = 11

	def name(self) -> str:
		match self:
			case DHAbility.DOWNBOOST: return "down boost"
			case DHAbility.BEACH: return "beach"
			case DHAbility.FUSION: return "fusion"
			case DHAbility.SHOP: return "Phoenix Shop"
			case DHAbility.LAB: return "laboratory"
			case DHAbility.REROLL: return "rerolling"
			case DHAbility.SECOND_EQUIP_SLOT: return "second equip slot"
