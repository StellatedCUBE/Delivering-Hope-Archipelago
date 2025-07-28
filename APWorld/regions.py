from enum import Enum

class DHRegion(Enum):
	MENU = -1
	STANDARD = 0
	CLASSIC = 1
	BAELESS = 2
	GACHA = 3
	THE_CELL = 4
	SPACE = 5

	def name(self) -> str:
		match self:
			case DHRegion.MENU: return "Menu"
			case DHRegion.STANDARD: return "Standard"
			case DHRegion.CLASSIC: return "Classic"
			case DHRegion.BAELESS: return "Baeless"
			case DHRegion.GACHA: return "Gacha"
			case DHRegion.THE_CELL: return "The Cell"
			case DHRegion.SPACE: return "Space"
