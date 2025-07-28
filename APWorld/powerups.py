from enum import Enum

class DHPowerUp(Enum):
	SODA_BOOST = 0
	EXTRA_CHARGE = 2
	RAT_BLOCKER = 3
	RESIST_SLOW = 5
	SODA_QUANTITY = 7
	HOT_SAUCE = 8
	VERY_HOT_SAUCE = 13
	LAST_HOPE = 16
	DIVORCE_PAPERS = 17
	REBIRTH = 18
	TOFU_RESIST = 48

	def name(self) -> str:
		match self:
			case DHPowerUp.SODA_BOOST: return "soda boost upgrades"
			case DHPowerUp.EXTRA_CHARGE: return "extra charge slot upgrades"
			case DHPowerUp.RAT_BLOCKER: return "rat blocker"
			case DHPowerUp.RESIST_SLOW: return "meatball resist upgrades"
			case DHPowerUp.SODA_QUANTITY: return "soda quantity upgrades"
			case DHPowerUp.HOT_SAUCE: return "hot sauce"
			case DHPowerUp.VERY_HOT_SAUCE: return "very hot sauce"
			case DHPowerUp.LAST_HOPE: return "last hope"
			case DHPowerUp.DIVORCE_PAPERS: return "divorce papers"
			case DHPowerUp.REBIRTH: return "rebirth"
			case DHPowerUp.TOFU_RESIST: return "tofu resist upgrades"
	
	def has_additional_behaviour(self) -> bool:
		return (
			self == DHPowerUp.HOT_SAUCE or
			self == DHPowerUp.VERY_HOT_SAUCE or
			self == DHPowerUp.LAST_HOPE or
			self == DHPowerUp.DIVORCE_PAPERS
		)
	
	def default(self) -> bool:
		return (
			self == DHPowerUp.SODA_BOOST or
			self == DHPowerUp.RESIST_SLOW or
			self == DHPowerUp.TOFU_RESIST
		)
	
